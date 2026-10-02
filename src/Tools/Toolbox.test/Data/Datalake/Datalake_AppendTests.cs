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

public class Datalake_AppendTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public Datalake_AppendTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(Datalake_AppendTests) + "/" + function;
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
    public async Task Append_WithoutLease_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "append-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await store.Add(path, record1.ToJson().ToDataETag())).BeOk();

        string appendData = "\n" + record2.ToJson();
        (await store.Append(path, appendData.ToDataETag())).BeOk();

        var result = (await store.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record1.ToJson()).BeTrue();
        content.Contains(record2.ToJson()).BeTrue();

        (await store.ForceDelete(path)).BeOk();
    }

    [Fact]
    public async Task Append_StartWithLease_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "append-lease-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await store.Append(path, record1.ToJson().ToDataETag())).BeOk();

        var leaseOption = await store.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        string appendData = "\n" + record2.ToJson();
        (await store.Append(path, appendData.ToDataETag(), leaseId)).BeOk();

        (await store.ReleaseLease(path, leaseId)).BeOk();

        var result = (await store.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record1.ToJson()).BeTrue();
        content.Contains(record2.ToJson()).BeTrue();

        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Append_WithLease_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "append-with-lease-test.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await store.Add(path, record1.ToJson().ToDataETag())).BeOk();

        var leaseOption = await store.AcquireLease(path, TimeSpan.FromSeconds(30));
        leaseOption.BeOk();
        string leaseId = leaseOption.Return();

        string appendData = "\n" + record2.ToJson();
        (await store.Append(path, appendData.ToDataETag(), leaseId)).BeOk();

        (await store.ReleaseLease(path, leaseId)).BeOk();

        var result = (await store.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record1.ToJson()).BeTrue();
        content.Contains(record2.ToJson()).BeTrue();

        (await store.Delete(path)).BeOk();
    }


    [Fact]
    public async Task ConcurrentAppend_MultipleCalls_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "concurrent-append.json";
        int count = 10;

        (await store.Add(path, "initial".ToDataETag())).BeOk();

        var tasks = Enumerable.Range(0, count)
            .Select(async i =>
            {
                var record = new TestRecord($"User{i}", 20 + i);
                return await store.Append(path, $"{record.ToJson()}\n".ToDataETag());
            })
            .ToArray();

        var results = await Task.WhenAll(tasks);
        results.ForEach(x => x.BeOk());

        var content = (await store.Get(path)).BeOk().Return().DataToString();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Assert(x => x >= count);

        (await store.ForceDelete(path)).BeOk();
    }

    [Fact]
    public async Task ConcurrentAppend_Stress_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "concurrent-append.json";
        int count = 100;
        var list = Enumerable.Range(0, count)
            .Select(i => new TestRecord($"User{i}", 20 + i))
            .ToList();

        (await store.Add(path, "initial".ToDataETag())).BeOk();

        await Parallel.ForEachAsync(list, async (record, _) =>
        {
            var r = await store.Append(path, $"{record.ToJson()}\n".ToDataETag());
            r.BeOk();
        });

        var content = (await store.Get(path)).BeOk().Return().DataToString();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Assert(x => x >= count);

        (await store.ForceDelete(path)).BeOk();
    }

    [Fact]
    public async Task Append_ToNonExistentFile_ShouldCreateFile()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "append-new-file.json";
        var record = new TestRecord("New File Record", 25);

        (await store.Exists(path)).BeNotFound();

        (await store.Append(path, record.ToJson().ToDataETag())).BeOk();

        (await store.Exists(path)).BeOk();

        var result = (await store.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record.ToJson()).BeTrue();

        (await store.ForceDelete(path)).BeOk();
    }

    [Fact]
    public async Task Append_WithoutLease_OnLockedFile_ShouldAcquireLeaseAndSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "Append_WithoutLease_OnLockedFile_ShouldAcquireLeaseAndSucceed.json";
        var record1 = new TestRecord("First", 10);
        var record2 = new TestRecord("Second", 20);

        (await store.Add(path, record1.ToJson().ToDataETag())).BeOk();

        string appendData = "\n" + record2.ToJson();
        (await store.Append(path, appendData.ToDataETag())).BeOk();

        var result = (await store.Get(path)).BeOk().Return();
        string content = result.DataToString();
        content.Contains(record1.ToJson()).BeTrue();
        content.Contains(record2.ToJson()).BeTrue();

        (await store.ForceDelete(path)).BeOk();
    }

    [Fact]
    public async Task Append_ReturnsETag()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "append-etag-test.json";
        var record = new TestRecord("ETag Test", 30);

        (await store.Add(path, "initial".ToDataETag())).BeOk();

        var data = record.ToJson().ToDataETag();
        var appendResult = await store.Append(path, data);
        appendResult.BeOk();
        string etag = appendResult.Return();
        etag.NotEmpty();

        (await store.ForceDelete(path)).BeOk();
    }

    [Fact]
    public async Task Append_MultipleSequential_ShouldAccumulateContent()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "sequential-append.json";
        int count = 5;

        (await store.Add(path, "start\n".ToDataETag())).BeOk();

        for (int i = 0; i < count; i++)
        {
            var record = new TestRecord($"User{i}", 20 + i);
            (await store.Append(path, $"{record.ToJson()}\n".ToDataETag())).BeOk();
        }

        var result = (await store.Get(path)).BeOk().Return();
        string content = result.DataToString();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Be(count + 1);

        (await store.ForceDelete(path)).BeOk();
    }
}
