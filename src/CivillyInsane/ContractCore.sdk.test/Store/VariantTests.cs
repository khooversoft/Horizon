using ContractCore.sdk.test.Application;
using Toolbox.Tools;

namespace ContractCore.sdk.test.Store;

public class VariantTests
{
    private readonly ITestOutputHelper _outputHelper;
    public VariantTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task CreateAndGet()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct(basePriceCents: 2500);
        var variant = await host.CreateVariant(product.ProductId!.Value, stockQty: 5);

        variant.VariantId.NotNull();
        variant.ProductId.Be(product.ProductId!.Value);
        variant.StockQty.Be(5);
        variant.EffectivePriceCents.Be(2500);

        var byId = (await host.Store.Variants.Get(variant.VariantId!.Value)).BeOk().Value;
        byId.Sku.Be(variant.Sku);

        var bySku = (await host.Store.Variants.Get(variant.Sku)).BeOk().Value;
        bySku.VariantId.Be(variant.VariantId);
    }

    [Fact]
    public async Task PriceOverride()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct(basePriceCents: 2500);
        var variant = await host.CreateVariant(product.ProductId!.Value, priceCents: 3100);

        variant.PriceCents.Be(3100);
        variant.EffectivePriceCents.Be(3100);
    }

    [Fact]
    public async Task Update()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct();
        var variant = await host.CreateVariant(product.ProductId!.Value);

        var updated = variant with { Size = "XL", Color = "White", Status = VariantStatus.Inactive };
        var result = (await host.Store.Variants.Save(updated)).BeOk().Value;

        result.VariantId.Be(variant.VariantId);
        result.Size.Be("XL");
        result.Color.Be("White");
        result.Status.Be(VariantStatus.Inactive);
    }

    [Fact]
    public async Task AdjustStock()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct();
        var variant = await host.CreateVariant(product.ProductId!.Value, stockQty: 10);
        long variantId = variant.VariantId!.Value;

        (await host.Store.Variants.AdjustStock(variantId, 5)).BeOk().Value.StockQty.Be(15);
        (await host.Store.Variants.AdjustStock(variantId, -15)).BeOk().Value.StockQty.Be(0);
        (await host.Store.Variants.AdjustStock(variantId, -1)).BeConflict();
        (await host.Store.Variants.AdjustStock(long.MaxValue, 1)).BeConflict();
    }

    [Fact]
    public async Task ListByProduct()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct();
        var v1 = await host.CreateVariant(product.ProductId!.Value);
        var v2 = await host.CreateVariant(product.ProductId!.Value);

        var list = (await host.Store.Variants.List(product.ProductId!.Value)).BeOk().Value;
        list.Count.Be(2);
        list.Any(x => x.VariantId == v1.VariantId).BeTrue();
        list.Any(x => x.VariantId == v2.VariantId).BeTrue();
    }

    [Fact]
    public async Task GetNotFound()
    {
        using var host = new TestHost(_outputHelper);

        (await host.Store.Variants.Get(long.MaxValue)).BeNotFound();
        (await host.Store.Variants.Get(TestHost.Unique("missing"))).BeNotFound();
    }

    [Fact]
    public async Task InvalidRecordThrows()
    {
        using var host = new TestHost(_outputHelper);

        var record = new VariantRecord { ProductId = 0, Sku = TestHost.Unique("sku"), Size = "M", Color = "Black" };
        await Verify.ThrowsAsync<ArgumentException>(() => host.Store.Variants.Save(record));
    }
}
