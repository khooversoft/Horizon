using ContractCore.sdk.test.Application;
using Toolbox.Tools;

namespace ContractCore.sdk.test.Store;

public class OrderTests
{
    private readonly ITestOutputHelper _outputHelper;
    public OrderTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task CreateAndGet()
    {
        using var host = new TestHost(_outputHelper);
        var customer = await host.CreateCustomer();
        var order = await host.CreateOrder(customer.CustomerId, customer.Email);

        order.OrderId.NotNull();
        order.Status.Be(OrderStatus.Pending);
        order.SubtotalCents.Be(0);
        order.TotalCents.Be(500 + 200 - 100);
        order.Items.Count.Be(0);

        var read = (await host.Store.Orders.Get(order.OrderId!.Value)).BeOk().Value;
        read.Email.Be(customer.Email);
        read.CustomerId.Be(customer.CustomerId);
    }

    [Fact]
    public async Task UpdatePendingOrder()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();

        var updated = order with { ShipCity = "Portland", ShipRegion = "OR", ShippingCents = 1000 };
        var result = (await host.Store.Orders.Save(updated)).BeOk().Value;

        result.ShipCity.Be("Portland");
        result.ShipRegion.Be("OR");
        result.ShippingCents.Be(1000);
    }

    [Fact]
    public async Task UpdateNonPendingOrderNotFound()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();
        (await host.Store.Orders.SetStatus(order.OrderId!.Value, OrderStatus.Paid)).BeOk();

        (await host.Store.Orders.Save(order with { ShipCity = "Nowhere" })).BeNotFound();
    }

    [Fact]
    public async Task AddItemUpdatesTotalsAndStock()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct(basePriceCents: 1000);
        var variant = await host.CreateVariant(product.ProductId!.Value, stockQty: 10);
        var order = await host.CreateOrder();

        var result = (await host.Store.Orders.AddItem(order.OrderId!.Value, variant.VariantId!.Value, 3)).BeOk().Value;

        result.Items.Count.Be(1);
        var item = result.Items[0];
        item.VariantId.Be(variant.VariantId!.Value);
        item.Sku.Be(variant.Sku);
        item.UnitPriceCents.Be(1000);
        item.Quantity.Be(3);
        item.LineTotalCents.Be(3000);
        item.ShippedQty.Be(0);

        result.SubtotalCents.Be(3000);
        result.TotalCents.Be(3000 + 500 + 200 - 100);

        (await host.Store.Variants.Get(variant.VariantId!.Value)).BeOk().Value.StockQty.Be(7);
    }

    [Fact]
    public async Task AddItemInsufficientStock()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct();
        var variant = await host.CreateVariant(product.ProductId!.Value, stockQty: 2);
        var order = await host.CreateOrder();

        (await host.Store.Orders.AddItem(order.OrderId!.Value, variant.VariantId!.Value, 3)).BeConflict();
        (await host.Store.Variants.Get(variant.VariantId!.Value)).BeOk().Value.StockQty.Be(2);
    }

    [Fact]
    public async Task AddItemInactiveVariant()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct();
        var variant = await host.CreateVariant(product.ProductId!.Value, status: VariantStatus.Inactive);
        var order = await host.CreateOrder();

        (await host.Store.Orders.AddItem(order.OrderId!.Value, variant.VariantId!.Value, 1)).BeConflict();
    }

    [Fact]
    public async Task AddItemToNonPendingOrder()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct();
        var variant = await host.CreateVariant(product.ProductId!.Value);
        var order = await host.CreateOrder();
        (await host.Store.Orders.SetStatus(order.OrderId!.Value, OrderStatus.Paid)).BeOk();

        (await host.Store.Orders.AddItem(order.OrderId!.Value, variant.VariantId!.Value, 1)).BeNotFound();
        (await host.Store.Orders.AddItem(long.MaxValue, variant.VariantId!.Value, 1)).BeNotFound();
    }

    [Fact]
    public async Task AddItemInvalidQuantityThrows()
    {
        using var host = new TestHost(_outputHelper);
        await Verify.ThrowsAsync<ArgumentException>(() => host.Store.Orders.AddItem(1, 1, 0));
    }

    [Fact]
    public async Task SetStatusPaidSetsPlacedUtc()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();
        (order.PlacedUtc == null).BeTrue();

        var result = (await host.Store.Orders.SetStatus(order.OrderId!.Value, OrderStatus.Paid)).BeOk().Value;
        result.Status.Be(OrderStatus.Paid);
        (result.PlacedUtc != null).BeTrue();
    }

    [Fact]
    public async Task CancelRestoresStock()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct();
        var variant = await host.CreateVariant(product.ProductId!.Value, stockQty: 10);
        var order = await host.CreateOrder();
        long orderId = order.OrderId!.Value;

        (await host.Store.Orders.AddItem(orderId, variant.VariantId!.Value, 4)).BeOk();
        (await host.Store.Orders.AddItem(orderId, variant.VariantId!.Value, 2)).BeOk();
        (await host.Store.Variants.Get(variant.VariantId!.Value)).BeOk().Value.StockQty.Be(4);

        (await host.Store.Orders.SetStatus(orderId, OrderStatus.Cancelled)).BeOk().Value.Status.Be(OrderStatus.Cancelled);
        (await host.Store.Variants.Get(variant.VariantId!.Value)).BeOk().Value.StockQty.Be(10);
    }

    [Fact]
    public async Task CancelShippedOrderConflict()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();
        long orderId = order.OrderId!.Value;

        (await host.Store.Orders.SetStatus(orderId, OrderStatus.Shipped)).BeOk();
        (await host.Store.Orders.SetStatus(orderId, OrderStatus.Cancelled)).BeConflict();
    }

    [Fact]
    public async Task SetStatusNotFound()
    {
        using var host = new TestHost(_outputHelper);
        (await host.Store.Orders.SetStatus(long.MaxValue, OrderStatus.Paid)).BeNotFound();
    }

    [Fact]
    public async Task SetStatusInvalidThrows()
    {
        using var host = new TestHost(_outputHelper);
        await Verify.ThrowsAsync<ArgumentException>(() => host.Store.Orders.SetStatus(1, "bogus"));
    }

    [Fact]
    public async Task GetNotFound()
    {
        using var host = new TestHost(_outputHelper);
        (await host.Store.Orders.Get(long.MaxValue)).BeNotFound();
    }

    [Fact]
    public async Task ListByCustomerAndEmail()
    {
        using var host = new TestHost(_outputHelper);
        var customer = await host.CreateCustomer();
        var o1 = await host.CreateOrder(customer.CustomerId, customer.Email);
        var o2 = await host.CreateOrder(customer.CustomerId, customer.Email);

        var byCustomer = (await host.Store.Orders.List(customerId: customer.CustomerId)).BeOk().Value;
        byCustomer.Count.Be(2);
        byCustomer.Any(x => x.OrderId == o1.OrderId).BeTrue();
        byCustomer.Any(x => x.OrderId == o2.OrderId).BeTrue();

        var byEmail = (await host.Store.Orders.List(email: customer.Email)).BeOk().Value;
        byEmail.Count.Be(2);

        (await host.Store.Orders.SetStatus(o1.OrderId!.Value, OrderStatus.Paid)).BeOk();
        var paid = (await host.Store.Orders.List(customerId: customer.CustomerId, status: OrderStatus.Paid)).BeOk().Value;
        paid.Count.Be(1);
        paid[0].OrderId.Be(o1.OrderId);
    }
}
