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

public class Datalake_SearchTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public Datalake_SearchTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(Datalake_SearchTests) + "/" + function;
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
    public async Task Search_ShouldTrimBasePathAndReturnRelativePaths()
    {
        using var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        var record = new TestRecord("Normalize", 1);
        string[] paths = ["normalized/file1.json", "normalized/sub/file2.json", "other/file3.json"];

        foreach (var path in paths)
        {
            (await store.Add(path, record.ToJson().ToDataETag())).BeOk();
        }

        var results = await store.Search("**/*.json");
        results.Count.Be(paths.Length);

        var returnedPaths = results.Select(x => x.Path).OrderBy(x => x).ToArray();
        returnedPaths.SequenceEqual(paths.OrderBy(x => x)).BeTrue();
        returnedPaths.All(x => !x.StartsWith("azuretest-datalake-test", StringComparison.OrdinalIgnoreCase)).BeTrue();
    }

    [Fact]
    public async Task Search_WithPaging_ShouldSkipAndTakeExpectedResults()
    {
        using var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        var record = new TestRecord("Paging", 2);
        var paths = Enumerable.Range(0, 6).Select(i => $"paging/page-{i:D2}.json").ToArray();

        foreach (var path in paths)
        {
            (await store.Add(path, record.ToJson().ToDataETag())).BeOk();
        }

        var allResults = (await store.Search("paging/*.json")).ToArray();
        allResults.Length.Be(paths.Length);

        var page = (await store.Search("paging/*.json", index: 2, size: 3)).ToArray();
        page.Length.Be(3);

        var expectedPaths = allResults.Skip(2).Take(3).Select(x => x.Path).OrderBy(x => x).ToArray();
        var pagePaths = page.Select(x => x.Path).OrderBy(x => x).ToArray();
        pagePaths.SequenceEqual(expectedPaths).BeTrue();
    }

    [Fact]
    public async Task Search_ShouldIncludeFoldersWhenPatternMatches()
    {
        using var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        var record = new TestRecord("Folder", 3);

        (await store.Add("folders/root/file1.json", record.ToJson().ToDataETag())).BeOk();
        (await store.Add("folders/root/nested/file2.json", record.ToJson().ToDataETag())).BeOk();

        var results = await store.Search("folders/**");

        results.Any(x => x.Path.Equals("folders/root/file1.json", StringComparison.OrdinalIgnoreCase)).BeTrue();
        results.Any(x => x.Path.Equals("folders/root/nested/file2.json", StringComparison.OrdinalIgnoreCase)).BeTrue();
    }

    [Fact]
    public async Task Search_IncludeFolder_ShouldReturnFolders()
    {
        using var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        var record = new TestRecord("Diff", 5);

        (await store.Add("diff/a/file1.json", record.ToJson().ToDataETag())).BeOk();
        (await store.Add("diff/a/sub/file2.json", record.ToJson().ToDataETag())).BeOk();

        var doubleStar = await store.Search("diff/**", includeFolder: false);
        doubleStar.Count.Assert(x => x >= 2);
        doubleStar.Any(x => x.IsFolder).BeFalse();

        var tripleStar = await store.Search("diff/**", includeFolder: true);
        tripleStar.Count.Assert(x => x >= doubleStar.Count);
        tripleStar.Any(x => x.IsFolder).BeTrue();
    }

    [Fact]
    public async Task Search_NonRecursivePattern_ShouldNotReturnNestedFiles()
    {
        using var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        var record = new TestRecord("Scope", 4);

        (await store.Add("nonrecursive/level1.json", record.ToJson().ToDataETag())).BeOk();
        (await store.Add("nonrecursive/sub/level2.json", record.ToJson().ToDataETag())).BeOk();

        var nonRecursive = await store.Search("nonrecursive/*.json");
        nonRecursive.Count.Be(1);
        nonRecursive.Any(x => x.Path.Equals("nonrecursive/level1.json", StringComparison.OrdinalIgnoreCase)).BeTrue();

        var recursive = await store.Search("nonrecursive/**/*.json");
        recursive.Count.Be(2);
        recursive.Any(x => x.Path.Equals("nonrecursive/sub/level2.json", StringComparison.OrdinalIgnoreCase)).BeTrue();
    }
}
