using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace ContractCore.sdk.test.Application;

/// <summary>
/// Builds a service provider against the local SQL Server database published by
/// ContractCore.db (see ContractCore.db.publish.xml, database "ContractCore" on localhost).
/// </summary>
public sealed class TestHost : IDisposable
{
    public const string ConnectionString = "Data Source=localhost;Initial Catalog=ContractCore;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=60";

    private readonly ServiceProvider _serviceProvider;

    public TestHost(ITestOutputHelper outputHelper)
    {
        outputHelper.NotNull();

        var services = new ServiceCollection();
        services.AddLogging(c => c.AddLambda(outputHelper.WriteLine).AddFilter(_ => true));
        services.AddContractCore(new ContractCoreOption { ConnectionString = ConnectionString });

        _serviceProvider = services.BuildServiceProvider();
        Store = _serviceProvider.GetRequiredService<ContractCoreStore>();
    }

    public ContractCoreStore Store { get; }

    public void Dispose() => _serviceProvider.Dispose();

    public static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    public static string UniqueEmail() => $"test-{Guid.NewGuid():N}@example.com";

    public async Task<ProductRecord> CreateProduct(int basePriceCents = 2500, string status = ProductStatus.Active)
    {
        var record = new ProductRecord
        {
            Slug = Unique("product"),
            Name = "Test Product",
            Description = "Test product description",
            BasePriceCents = basePriceCents,
            Status = status,
        };

        return (await Store.Products.Save(record)).BeOk().Value;
    }

    public async Task<VariantRecord> CreateVariant(long productId, int stockQty = 10, int? priceCents = null, string status = VariantStatus.Active)
    {
        var record = new VariantRecord
        {
            ProductId = productId,
            Sku = Unique("sku"),
            Size = Guid.NewGuid().ToString("N")[..8],
            Color = "Black",
            PriceCents = priceCents,
            StockQty = stockQty,
            Status = status,
        };

        return (await Store.Variants.Save(record)).BeOk().Value;
    }

    public async Task<CustomerRecord> CreateCustomer()
    {
        var record = new CustomerRecord
        {
            Email = UniqueEmail(),
            FirstName = "Test",
            LastName = "Customer",
            Phone = "555-0100",
        };

        return (await Store.Customers.Save(record)).BeOk().Value;
    }

    public async Task<OrderRecord> CreateOrder(long? customerId = null, string? email = null)
    {
        var record = new OrderRecord
        {
            CustomerId = customerId,
            Email = email ?? UniqueEmail(),
            ShippingCents = 500,
            TaxCents = 200,
            DiscountCents = 100,
            ShipName = "Test Customer",
            ShipLine1 = "1 Main St",
            ShipCity = "Seattle",
            ShipRegion = "WA",
            ShipPostalCode = "98101",
            ShipCountryCode = "US",
        };

        return (await Store.Orders.Save(record)).BeOk().Value;
    }
}
