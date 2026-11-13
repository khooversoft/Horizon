using ContractCore.sdk.test.Application;
using Toolbox.Tools;

namespace ContractCore.sdk.test.Store;

public class ProductTests
{
    private readonly ITestOutputHelper _outputHelper;
    public ProductTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task CreateAndGet()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct(basePriceCents: 1999, status: ProductStatus.Draft);

        product.ProductId.NotNull();
        product.Name.Be("Test Product");
        product.BasePriceCents.Be(1999);
        product.CurrencyCode.Be("USD");
        product.Status.Be(ProductStatus.Draft);

        var byId = (await host.Store.Products.Get(product.ProductId!.Value)).BeOk().Value;
        byId.Slug.Be(product.Slug);

        var bySlug = (await host.Store.Products.Get(product.Slug)).BeOk().Value;
        bySlug.ProductId.Be(product.ProductId);
    }

    [Fact]
    public async Task Update()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct();

        var updated = product with { Name = "Updated Product", BasePriceCents = 3000, Status = ProductStatus.Archived, JsonData = "{\"a\":1}" };
        var result = (await host.Store.Products.Save(updated)).BeOk().Value;

        result.ProductId.Be(product.ProductId);
        result.Name.Be("Updated Product");
        result.BasePriceCents.Be(3000);
        result.Status.Be(ProductStatus.Archived);
        result.JsonData.Be("{\"a\":1}");
    }

    [Fact]
    public async Task UpdateNotFound()
    {
        using var host = new TestHost(_outputHelper);

        var record = new ProductRecord { ProductId = long.MaxValue, Slug = TestHost.Unique("product"), Name = "Missing", BasePriceCents = 100 };
        (await host.Store.Products.Save(record)).BeNotFound();
    }

    [Fact]
    public async Task GetNotFound()
    {
        using var host = new TestHost(_outputHelper);

        (await host.Store.Products.Get(long.MaxValue)).BeNotFound();
        (await host.Store.Products.Get(TestHost.Unique("missing"))).BeNotFound();
    }

    [Fact]
    public async Task InvalidRecordThrows()
    {
        using var host = new TestHost(_outputHelper);

        var record = new ProductRecord { Slug = TestHost.Unique("product"), Name = "Bad", BasePriceCents = 100, Status = "bogus" };
        await Verify.ThrowsAsync<ArgumentException>(() => host.Store.Products.Save(record));
    }

    [Fact]
    public async Task ListByStatus()
    {
        using var host = new TestHost(_outputHelper);
        var product = await host.CreateProduct(status: ProductStatus.Active);
        product = (await host.Store.Products.Save(product with { Name = $"000-{Guid.NewGuid():N}" })).BeOk().Value;

        var list = (await host.Store.Products.List(ProductStatus.Active, 0, 500)).BeOk().Value;
        list.Any(x => x.ProductId == product.ProductId).BeTrue();
        list.All(x => x.Status == ProductStatus.Active).BeTrue();

        var archived = (await host.Store.Products.List(ProductStatus.Archived, 0, 500)).BeOk().Value;
        archived.Any(x => x.ProductId == product.ProductId).BeFalse();
    }

    [Fact]
    public async Task ListInvalidPagination()
    {
        using var host = new TestHost(_outputHelper);

        (await host.Store.Products.List(null, -1, 10)).BeBadRequest();
        (await host.Store.Products.List(null, 0, 0)).BeBadRequest();
        (await host.Store.Products.List(null, 0, 501)).BeBadRequest();
    }
}
