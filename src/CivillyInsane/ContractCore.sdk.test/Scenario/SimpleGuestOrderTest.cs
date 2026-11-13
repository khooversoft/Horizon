using ContractCore.sdk.test.Application;
using Toolbox.Tools;

namespace ContractCore.sdk.test.Scenario;

/// <summary>
/// Test a simple order scenario with a single product, single variant, and a guest single customer.
/// The order is created, paid, and shipped successfully.
/// </summary>
public class SimpleGuestOrderTest
{
    private const int _basePriceCents = 2500;
    private const int _initialStock = 10;
    private const int _quantity = 2;
    private const int _shippingCents = 500;
    private const int _taxCents = 200;
    private const int _discountCents = 100;
    private const int _expectedSubtotal = _basePriceCents * _quantity;
    private const int _expectedTotal = _expectedSubtotal + _shippingCents + _taxCents - _discountCents;

    private readonly ITestOutputHelper _outputHelper;
    public SimpleGuestOrderTest(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task CreateGuestOrder()
    {
        using var host = new TestHost(_outputHelper);
        var (_, variant, order) = await CreateOrder(host);

        order.CustomerId.BeNull();
        order.Status.Be(OrderStatus.Pending);
        order.Items.Count.Be(1);
        order.Items[0].VariantId.Be(variant.VariantId!.Value);
        order.Items[0].UnitPriceCents.Be(_basePriceCents);
        order.Items[0].Quantity.Be(_quantity);
        order.SubtotalCents.Be(_expectedSubtotal);
        order.TotalCents.Be(_expectedTotal);

        (await host.Store.Variants.Get(variant.VariantId!.Value)).BeOk().Value.StockQty.Be(_initialStock - _quantity);
        (await host.Store.Customers.Get(order.Email)).BeNotFound();
    }

    [Fact]
    public async Task PayGuestOrder()
    {
        using var host = new TestHost(_outputHelper);
        var (_, _, order) = await CreateOrder(host);

        var paid = await PayOrder(host, order);

        paid.Status.Be(OrderStatus.Paid);
        (paid.PlacedUtc != null).BeTrue();

        var payments = (await host.Store.Payments.List(order.OrderId!.Value)).BeOk().Value;
        payments.Count.Be(1);
        payments[0].Status.Be(PaymentStatus.Captured);
        payments[0].AmountCents.Be(_expectedTotal);
    }

    [Fact]
    public async Task CreatePayAndShipGuestOrder()
    {
        using var host = new TestHost(_outputHelper);
        var (_, variant, order) = await CreateOrder(host);
        long orderId = order.OrderId!.Value;

        await PayOrder(host, order);

        // Create shipment and add the full order item quantity
        var shipment = (await host.Store.Shipments.Save(new ShipmentRecord { OrderId = orderId, Carrier = "UPS", ServiceLevel = "Ground" })).BeOk().Value;
        long shipmentId = shipment.ShipmentId!.Value;

        shipment = (await host.Store.Shipments.AddItem(shipmentId, order.Items[0].OrderItemId, _quantity)).BeOk().Value;
        shipment.Items.Count.Be(1);
        shipment.Items[0].Quantity.Be(_quantity);
        shipment.Items[0].Sku.Be(variant.Sku);

        // Ship it
        shipment = (await host.Store.Shipments.Save(shipment with { Status = ShipmentStatus.Shipped, TrackingNumber = TestHost.Unique("1Z") })).BeOk().Value;
        shipment.Status.Be(ShipmentStatus.Shipped);
        (shipment.ShippedUtc != null).BeTrue();

        (await host.Store.Shipments.AddTrackingEvent(new TrackingEventRecord
        {
            ShipmentId = shipmentId,
            StatusCode = "PICKED_UP",
            Description = "Picked up by carrier",
            Location = "Seattle, WA",
            EventUtc = DateTime.UtcNow,
        })).BeOk();

        var shippedOrder = (await host.Store.Orders.SetStatus(orderId, OrderStatus.Shipped)).BeOk().Value;
        shippedOrder.Status.Be(OrderStatus.Shipped);
        shippedOrder.Items[0].ShippedQty.Be(_quantity);

        // Deliver it
        shipment = (await host.Store.Shipments.Save(shipment with { Status = ShipmentStatus.Delivered })).BeOk().Value;
        (shipment.DeliveredUtc != null).BeTrue();
        (await host.Store.Orders.SetStatus(orderId, OrderStatus.Delivered)).BeOk().Value.Status.Be(OrderStatus.Delivered);

        // Final state
        var finalShipment = (await host.Store.Shipments.Get(shipmentId)).BeOk().Value;
        finalShipment.Status.Be(ShipmentStatus.Delivered);
        finalShipment.TrackingEvents.Count.Be(1);

        var finalOrder = (await host.Store.Orders.Get(orderId)).BeOk().Value;
        finalOrder.Status.Be(OrderStatus.Delivered);
        finalOrder.TotalCents.Be(_expectedTotal);
        finalOrder.CustomerId.BeNull();

        (await host.Store.Variants.Get(variant.VariantId!.Value)).BeOk().Value.StockQty.Be(_initialStock - _quantity);

        // Shipped order can no longer be cancelled or modified
        (await host.Store.Orders.SetStatus(orderId, OrderStatus.Cancelled)).BeConflict();
        (await host.Store.Orders.AddItem(orderId, variant.VariantId!.Value, 1)).BeNotFound();
    }

    private static async Task<(ProductRecord Product, VariantRecord Variant, OrderRecord Order)> CreateOrder(TestHost host)
    {
        var product = await host.CreateProduct(basePriceCents: _basePriceCents, status: ProductStatus.Active);
        var variant = await host.CreateVariant(product.ProductId!.Value, stockQty: _initialStock);

        // Guest checkout: no customer record, email only
        var order = await host.CreateOrder(customerId: null);
        order = (await host.Store.Orders.AddItem(order.OrderId!.Value, variant.VariantId!.Value, _quantity)).BeOk().Value;

        return (product, variant, order);
    }

    private static async Task<OrderRecord> PayOrder(TestHost host, OrderRecord order)
    {
        var payment = (await host.Store.Payments.Save(new PaymentRecord
        {
            OrderId = order.OrderId!.Value,
            Provider = "stripe",
            ProviderRef = TestHost.Unique("pi"),
            Status = PaymentStatus.Authorized,
            AmountCents = order.TotalCents,
        })).BeOk().Value;

        (await host.Store.Payments.Save(payment with { Status = PaymentStatus.Captured })).BeOk().Value.Status.Be(PaymentStatus.Captured);

        return (await host.Store.Orders.SetStatus(order.OrderId!.Value, OrderStatus.Paid)).BeOk().Value;
    }
}
