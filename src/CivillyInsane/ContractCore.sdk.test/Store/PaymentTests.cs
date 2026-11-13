using ContractCore.sdk.test.Application;
using Toolbox.Tools;

namespace ContractCore.sdk.test.Store;

public class PaymentTests
{
    private readonly ITestOutputHelper _outputHelper;
    public PaymentTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    [Fact]
    public async Task CreateGetAndUpdate()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();

        var record = new PaymentRecord
        {
            OrderId = order.OrderId!.Value,
            Provider = "stripe",
            ProviderRef = TestHost.Unique("pi"),
            AmountCents = 600,
        };

        var payment = (await host.Store.Payments.Save(record)).BeOk().Value;
        payment.PaymentId.NotNull();
        payment.Status.Be(PaymentStatus.Pending);
        payment.AmountCents.Be(600);

        var read = (await host.Store.Payments.Get(payment.PaymentId!.Value)).BeOk().Value;
        read.ProviderRef.Be(record.ProviderRef);

        var captured = (await host.Store.Payments.Save(payment with { Status = PaymentStatus.Captured })).BeOk().Value;
        captured.PaymentId.Be(payment.PaymentId);
        captured.Status.Be(PaymentStatus.Captured);
    }

    [Fact]
    public async Task UpdateNotFound()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();

        var record = new PaymentRecord { PaymentId = long.MaxValue, OrderId = order.OrderId!.Value, Provider = "stripe", AmountCents = 1 };
        (await host.Store.Payments.Save(record)).BeNotFound();
    }

    [Fact]
    public async Task ListByOrder()
    {
        using var host = new TestHost(_outputHelper);
        var order = await host.CreateOrder();
        long orderId = order.OrderId!.Value;

        (await host.Store.Payments.Save(new PaymentRecord { OrderId = orderId, Provider = "stripe", AmountCents = 100, Status = PaymentStatus.Failed })).BeOk();
        (await host.Store.Payments.Save(new PaymentRecord { OrderId = orderId, Provider = "stripe", AmountCents = 100, Status = PaymentStatus.Captured })).BeOk();

        var list = (await host.Store.Payments.List(orderId)).BeOk().Value;
        list.Count.Be(2);
        list.All(x => x.OrderId == orderId).BeTrue();
    }

    [Fact]
    public async Task GetNotFound()
    {
        using var host = new TestHost(_outputHelper);
        (await host.Store.Payments.Get(long.MaxValue)).BeNotFound();
    }

    [Fact]
    public async Task InvalidRecordThrows()
    {
        using var host = new TestHost(_outputHelper);

        var record = new PaymentRecord { OrderId = 1, Provider = "stripe", AmountCents = 1, Status = "bogus" };
        await Verify.ThrowsAsync<ArgumentException>(() => host.Store.Payments.Save(record));
    }
}
