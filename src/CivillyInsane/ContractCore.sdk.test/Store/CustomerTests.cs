using ContractCore.sdk.test.Application;
using Toolbox.Tools;

namespace ContractCore.sdk.test.Store;

public class CustomerTests
{
    private readonly ITestOutputHelper _outputHelper;
    public CustomerTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task CreateAndGet()
    {
        using var host = new TestHost(_outputHelper);
        var customer = await host.CreateCustomer();

        customer.CustomerId.NotNull();
        customer.FirstName.Be("Test");
        customer.LastName.Be("Customer");

        var byId = (await host.Store.Customers.Get(customer.CustomerId!.Value)).BeOk().Value;
        byId.Email.Be(customer.Email);

        var byEmail = (await host.Store.Customers.Get(customer.Email)).BeOk().Value;
        byEmail.CustomerId.Be(customer.CustomerId);
    }

    [Fact]
    public async Task Update()
    {
        using var host = new TestHost(_outputHelper);
        var customer = await host.CreateCustomer();

        var updated = customer with { FirstName = "Updated", Phone = "555-0199" };
        var result = (await host.Store.Customers.Save(updated)).BeOk().Value;

        result.CustomerId.Be(customer.CustomerId);
        result.FirstName.Be("Updated");
        result.Phone.Be("555-0199");
    }

    [Fact]
    public async Task GetNotFound()
    {
        using var host = new TestHost(_outputHelper);

        (await host.Store.Customers.Get(long.MaxValue)).BeNotFound();
        (await host.Store.Customers.Get(TestHost.UniqueEmail())).BeNotFound();
    }

    [Fact]
    public async Task InvalidEmailThrows()
    {
        using var host = new TestHost(_outputHelper);

        var record = new CustomerRecord { Email = "not-an-email" };
        await Verify.ThrowsAsync<ArgumentException>(() => host.Store.Customers.Save(record));
    }

    [Fact]
    public async Task List()
    {
        using var host = new TestHost(_outputHelper);
        await host.CreateCustomer();
        await host.CreateCustomer();

        var list = (await host.Store.Customers.List(0, 1)).BeOk().Value;
        list.Count.Be(1);
    }
}
