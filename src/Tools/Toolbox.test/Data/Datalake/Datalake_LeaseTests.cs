using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.test.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.test.Data.Datalake;

public class Datalake_LeaseTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public Datalake_LeaseTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(Datalake_LeaseTests) + "/" + function;
        var option = TestApplication.ReadOption(basePath);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddDatalakeStore(option.Datalake);
            })
            .Build();

        // Clear the store before running tests, this includes any locked files
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();
        await store.SearchDelete(basePath);
        (await store.Search($"{basePath}/**")).Count().Be(0);
        return host;
    }


    [Fact]
    public async Task AcquireLease_WithDuration_ShouldLockFile()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "lease-test.json";
        var testRecord = new TestRecord("Leased User", 40);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();

        var leaseOption = await store.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();
        leaseId.NotEmpty();

        (await store.Delete(path)).StatusCode.Be(StatusCode.Locked);

        (await store.ReleaseLease(path, leaseId)).BeOk();
        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task AcquireExclusiveLock_ShouldLockFileIndefinitely()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "exclusive-lock-test.json";
        var testRecord = new TestRecord("Locked User", 45);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();

        var leaseOption = await store.AcquireExclusiveLock(path, breakLeaseIfExist: false);
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();
        leaseId.NotEmpty();

        (await store.Delete(path)).StatusCode.Be(StatusCode.Locked);

        (await store.ReleaseLease(path, leaseId)).BeOk();
        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task RenewLease_WithValidLease_ShouldKeepFileLocked()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "renew-lease-valid.json";
        var testRecord = new TestRecord("Renew User", 55);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();
        string leaseId = (await store.AcquireLease(path, TimeSpan.FromSeconds(15))).BeOk().Return();

        (await store.RenewLease(path, leaseId)).BeOk();

        (await store.Delete(path)).StatusCode.Be(StatusCode.Locked);

        (await store.ReleaseLease(path, leaseId)).BeOk();
        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task RenewLease_WithInvalidLeaseId_ShouldReturnNotFound()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "renew-lease-invalid.json";
        var testRecord = new TestRecord("Renew Invalid User", 60);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();
        string leaseId = (await store.AcquireLease(path, TimeSpan.FromSeconds(15))).BeOk().Return();

        (await store.RenewLease(path, Guid.NewGuid().ToString())).BeNotFound();

        (await store.ReleaseLease(path, leaseId)).BeOk();
        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task RenewLease_OnNonExistentFile_ShouldReturnNotFound()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "renew-lease-missing.json";

        (await store.RenewLease(path, Guid.NewGuid().ToString())).BeNotFound();
    }

    [Fact]
    public async Task BreakLease_ShouldReleaseLockedFile()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "break-lease-test.json";
        var testRecord = new TestRecord("Locked User", 50);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();

        var leaseOption = await store.AcquireExclusiveLock(path, breakLeaseIfExist: false);
        leaseOption.BeOk();

        (await store.Delete(path)).StatusCode.Be(StatusCode.Locked);

        (await store.BreakLease(path)).BeOk();

        await Task.Delay(1000); // Wait for lease to break

        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Set_WithLease_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "set-lease-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await store.Add(path, record1.ToJson().ToDataETag())).BeOk();

        var leaseOption = await store.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        (await store.Set(path, record2.ToJson().ToDataETag(), leaseId)).BeOk();
        var result = (await store.Get(path)).BeOk().Return();
        result.ToObject<TestRecord>().Be(record2);

        (await store.ReleaseLease(path, leaseId)).BeOk();
        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task AcquireLease_OnNonExistentFile_ShouldCreateAndLock()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "non-existent-lease.json";

        (await store.Exists(path)).BeNotFound();

        string leaseId = (await store.AcquireLease(path, TimeSpan.FromSeconds(30))).BeOk().Return();
        leaseId.NotEmpty();

        (await store.Exists(path)).BeOk();

        (await store.ReleaseLease(path, leaseId)).BeOk();
        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task AcquireExclusiveLock_WithBreakLease_ShouldBreakExistingLease()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "break-existing-lock.json";
        var testRecord = new TestRecord("Test User", 30);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();

        var firstLeaseOption = await store.AcquireExclusiveLock(path, breakLeaseIfExist: false);
        firstLeaseOption.BeOk();
        string firstLeaseId = firstLeaseOption.Return();

        var secondLeaseOption = await store.AcquireExclusiveLock(path, breakLeaseIfExist: true);
        secondLeaseOption.BeOk();
        string secondLeaseId = secondLeaseOption.Return();
        secondLeaseId.NotEmpty();
        secondLeaseId.Assert(x => x != firstLeaseId, "Second lease should be different from first");

        (await store.ReleaseLease(path, secondLeaseId)).BeOk();
        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Delete_WithValidLease_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "delete-with-lease.json";
        var testRecord = new TestRecord("Delete Test", 25);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();
        string leaseId = (await store.AcquireLease(path, TimeSpan.FromSeconds(30))).BeOk().Return();

        (await store.Delete(path, leaseId)).BeOk();
        (await store.Exists(path)).BeNotFound();
    }

    [Fact]
    public async Task Delete_WithInvalidLease_ShouldReturnLocked()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "delete-invalid-lease.json";
        var testRecord = new TestRecord("Invalid Lease Test", 35);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();

        var leaseOption = await store.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        (await store.Delete(path, Guid.NewGuid().ToString())).StatusCode.Be(StatusCode.Locked);

        (await store.ReleaseLease(path, leaseId)).BeOk();
        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task BreakLease_OnNonExistentFile_ShouldReturnNotFound()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "non-existent-break.json";

        (await store.BreakLease(path)).BeNotFound();
    }

    [Fact]
    public async Task BreakLease_OnUnlockedFile_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "unlocked-break.json";
        var testRecord = new TestRecord("Unlocked User", 40);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();

        (await store.BreakLease(path)).BeOk();

        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task ReleaseLease_OnNonExistentFile_ShouldReturnNotFound()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "non-existent-release.json";

        (await store.ReleaseLease(path, Guid.NewGuid().ToString())).BeNotFound();
    }

    [Fact]
    public async Task Set_WithoutLease_OnLockedFile_ShouldReturnLocked()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "locked-set-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await store.Add(path, record1.ToJson().ToDataETag())).BeOk();

        var leaseOption = await store.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        (await store.Set(path, record2.ToJson().ToDataETag())).StatusCode.Be(StatusCode.Locked);

        (await store.ReleaseLease(path, leaseId)).BeOk();
        (await store.Delete(path)).BeOk();
    }

}
