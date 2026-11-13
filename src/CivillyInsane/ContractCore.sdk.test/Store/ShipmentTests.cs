using ContractCore.sdk.test.Application;
using Toolbox.Tools;

namespace ContractCore.sdk.test.Store;

public class ShipmentTests
{
    private readonly ITestOutputHelper _outputHelper;
    public ShipmentTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private static async Task<OrderRecord> CreateOrderWithItem(TestHost host, int quantity)
    {
        var product = await host.CreateProduct(basePriceCents: 1000);
        var variant = await host.CreateVariant(product.ProductId!.Value, stockQty: 10);
        var order = await host.CreateOrder();

        return (await host.Store.Orders.AddItem(order.OrderId!.Value, variant.VariantId!.Value, quantity)).BeOk().Value;
    }

    [Fact]
    public async Task CreateAndGet()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();

        var record = new ShipmentRecord { OrderId = order.OrderId!.Value, Carrier = "UPS", ServiceLevel = "Ground" };
        var shipment = (await host.Store.Shipments.Save(record)).BeOk().Value;

        shipment.ShipmentId.NotNull();
        shipment.Status.Be(ShipmentStatus.Pending);
        (shipment.ShippedUtc == null).BeTrue();
        (shipment.DeliveredUtc == null).BeTrue();
        shipment.Items.Count.Be(0);
        shipment.TrackingEvents.Count.Be(0);

        var read = (await host.Store.Shipments.Get(shipment.ShipmentId!.Value)).BeOk().Value;
        read.Carrier.Be("UPS");
        read.ServiceLevel.Be("Ground");
    }

    [Fact]
    public async Task UpdateStatusSetsTimestamps()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();
        var shipment = (await host.Store.Shipments.Save(new ShipmentRecord { OrderId = order.OrderId!.Value })).BeOk().Value;

        var shipped = (await host.Store.Shipments.Save(shipment with { Status = ShipmentStatus.Shipped, TrackingNumber = "1Z999" })).BeOk().Value;
        shipped.Status.Be(ShipmentStatus.Shipped);
        shipped.TrackingNumber.Be("1Z999");
        (shipped.ShippedUtc != null).BeTrue();
        (shipped.DeliveredUtc == null).BeTrue();

        var delivered = (await host.Store.Shipments.Save(shipped with { Status = ShipmentStatus.Delivered })).BeOk().Value;
        delivered.ShippedUtc.Be(shipped.ShippedUtc);
        (delivered.DeliveredUtc != null).BeTrue();
    }

    [Fact]
    public async Task UpdateNotFound()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();

        (await host.Store.Shipments.Save(new ShipmentRecord { ShipmentId = long.MaxValue, OrderId = order.OrderId!.Value })).BeNotFound();
    }

    [Fact]
    public async Task AddItems()
    {
        using var host = new TestHost(_outputHelper);
        var order = await CreateOrderWithItem(host, 3);
        var orderItem = order.Items[0];
        var shipment = (await host.Store.Shipments.Save(new ShipmentRecord { OrderId = order.OrderId!.Value })).BeOk().Value;
        long shipmentId = shipment.ShipmentId!.Value;

        var result = (await host.Store.Shipments.AddItem(shipmentId, orderItem.OrderItemId, 2)).BeOk().Value;
        result.Items.Count.Be(1);
        result.Items[0].OrderItemId.Be(orderItem.OrderItemId);
        result.Items[0].Quantity.Be(2);
        result.Items[0].Sku.Be(orderItem.Sku);

        var second = (await host.Store.Shipments.Save(new ShipmentRecord { OrderId = order.OrderId!.Value })).BeOk().Value;
        long secondId = second.ShipmentId!.Value;

        (await host.Store.Shipments.AddItem(secondId, orderItem.OrderItemId, 2)).BeConflict();

        result = (await host.Store.Shipments.AddItem(secondId, orderItem.OrderItemId, 1)).BeOk().Value;
        result.Items.Count.Be(1);
        result.Items[0].Quantity.Be(1);

        var readOrder = (await host.Store.Orders.Get(order.OrderId!.Value)).BeOk().Value;
        readOrder.Items[0].ShippedQty.Be(3);

        var third = (await host.Store.Shipments.Save(new ShipmentRecord { OrderId = order.OrderId!.Value })).BeOk().Value;
        (await host.Store.Shipments.AddItem(third.ShipmentId!.Value, orderItem.OrderItemId, 1)).BeConflict();
    }

    [Fact]
    public async Task AddItemFromOtherOrderNotFound()
    {
        using var host = new TestHost(_outputHelper);
        var order = await CreateOrderWithItem(host, 1);
        var otherOrder = await host.CreateOrder();
        var shipment = (await host.Store.Shipments.Save(new ShipmentRecord { OrderId = otherOrder.OrderId!.Value })).BeOk().Value;

        (await host.Store.Shipments.AddItem(shipment.ShipmentId!.Value, order.Items[0].OrderItemId, 1)).BeNotFound();
    }

    [Fact]
    public async Task AddItemInvalidQuantityThrows()
    {
        using var host = new TestHost(_outputHelper);
        await Verify.ThrowsAsync<ArgumentException>(() => host.Store.Shipments.AddItem(1, 1, 0));
    }

    [Fact]
    public async Task AddTrackingEvent()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();
        var shipment = (await host.Store.Shipments.Save(new ShipmentRecord { OrderId = order.OrderId!.Value, Status = ShipmentStatus.Shipped })).BeOk().Value;
        long shipmentId = shipment.ShipmentId!.Value;

        var trackingEvent = new TrackingEventRecord
        {
            ShipmentId = shipmentId,
            StatusCode = "IN_TRANSIT",
            Description = "Departed facility",
            Location = "Seattle, WA",
            EventUtc = DateTime.UtcNow,
        };

        var result = (await host.Store.Shipments.AddTrackingEvent(trackingEvent)).BeOk().Value;
        result.TrackingEventId.NotNull();
        result.ShipmentId.Be(shipmentId);
        result.StatusCode.Be("IN_TRANSIT");
        result.Location.Be("Seattle, WA");

        var read = (await host.Store.Shipments.Get(shipmentId)).BeOk().Value;
        read.TrackingEvents.Count.Be(1);
        read.TrackingEvents[0].TrackingEventId.Be(result.TrackingEventId);
    }

    [Fact]
    public async Task InvalidTrackingEventThrows()
    {
        using var host = new TestHost(_outputHelper);
        await Verify.ThrowsAsync<ArgumentException>(() => host.Store.Shipments.AddTrackingEvent(new TrackingEventRecord { ShipmentId = 0, StatusCode = "X" }));
    }

    [Fact]
    public async Task ListByOrder()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();
        long orderId = order.OrderId!.Value;

        (await host.Store.Shipments.Save(new ShipmentRecord { OrderId = orderId })).BeOk();
        (await host.Store.Shipments.Save(new ShipmentRecord { OrderId = orderId })).BeOk();

        var list = (await host.Store.Shipments.List(orderId)).BeOk().Value;
        list.Count.Be(2);
        list.All(x => x.OrderId == orderId).BeTrue();
    }

    [Fact]
    public async Task GetNotFound()
    {
        using var host = new TestHost(_outputHelper);
        (await host.Store.Shipments.Get(long.MaxValue)).BeNotFound();
    }
}
