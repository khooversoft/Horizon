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

public class Datalake_TypedTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public Datalake_TypedTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(Datalake_TypedTests) + "/" + function;
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
    public async Task SingleFileReadWriteSearchDelete()
    {
        using var host = await BuildService();
        var store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "testrecord.json";
        var testRecord = new TestRecord("John Doe", 30);

        (await store.Exists(path)).BeNotFound();
        (await store.Get<TestRecord>(path)).BeNotFound();
        (await store.Delete(path)).BeNotFound();

        (await store.Add(path, testRecord)).BeOk();
        (await store.Add(path, testRecord)).BeConflict();

        (await store.Exists(path)).BeOk();
        (await store.Get<TestRecord>(path)).BeOk().Return().Action(x =>
        {
            x.Be(testRecord);
        });

        (await store.Search("**.*")).Count().Be(1);
        (await store.Delete(path)).BeOk();
        (await store.Search("**.*")).Count().Be(0);
    }

    [Fact]
    public async Task SetTyped_CreateNewFile_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "typed-set-new.json";
        var record = new TestRecord("New User", 18);

        (await store.Exists(path)).BeNotFound();

        var setResult = await store.Set(path, record);
        setResult.BeOk();
        setResult.Return().NotEmpty();

        (await store.Get<TestRecord>(path)).BeOk().Return().Be(record);

        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task SetTyped_UpdateExistingFile_ShouldOverwriteAndReturnNewETag()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "typed-set-update.json";
        var original = new TestRecord("Original", 20);
        var updated = new TestRecord("Updated", 25);

        var addResult = await store.Add(path, original);
        addResult.BeOk();
        string originalETag = addResult.Return();

        var setResult = await store.Set(path, updated);
        setResult.BeOk();
        string newETag = setResult.Return();
        newETag.NotEmpty().NotBe(originalETag);

        (await store.Get<TestRecord>(path)).BeOk().Return().Be(updated);

        (await store.Delete(path)).BeOk();
    }

}