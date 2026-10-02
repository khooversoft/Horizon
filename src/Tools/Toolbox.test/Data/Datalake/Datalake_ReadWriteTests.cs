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

public class Datalake_ReadWriteTests
{
    private ITestOutputHelper _outputHelper;
    private record TestRecord(string Name, int Age);

    public Datalake_ReadWriteTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(Datalake_ReadWriteTests) + "/" + function;
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
    public async Task SingleFileReadWriteSearchDeleteNoExtensions()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "testrecord.json";
        var testRecord = new TestRecord("John Doe", 30);

        (await store.Exists(path)).BeNotFound();
        (await store.Get(path)).BeNotFound();
        (await store.Delete(path)).BeNotFound();
        (await store.Search("**")).Count().Be(0);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeConflict();
        (await store.Search("**")).Count().Be(1);

        (await store.Exists(path)).BeOk();
        (await store.Get(path)).BeOk().Return().Action(x =>
        {
            var o = x.ToObject<TestRecord>();
            o.Be(testRecord);
        });

        (await store.Search("testrecord.json")).Count().Be(1);
        (await store.Search("testrecord.*")).Count().Be(1);
        (await store.Search("*.json")).Count().Be(1);
        (await store.Search("*.*")).Count().Be(1);
        (await store.Search("**")).Count().Be(1);

        (await store.Delete(path)).BeOk();

        (await store.Search("testrecord.json")).Count().Be(0);
        (await store.Search("testrecord.*")).Count().Be(0);
        (await store.Search("*.json")).Count().Be(0);
        (await store.Search("*.*")).Count().Be(0);
        (await store.Search("**")).Count().Be(0);
    }

    [Fact]
    public async Task SingleFileReadWriteSearchDeleteExtensions()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "testrecord.json";
        var testRecord = new TestRecord("John Doe", 30);

        (await store.Exists(path)).BeNotFound();
        (await store.Get(path)).BeNotFound();
        (await store.Delete(path)).BeNotFound();

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeConflict();

        (await store.Exists(path)).BeOk();
        (await store.Get(path)).BeOk().Return().ToObject<TestRecord>().Be(testRecord);

        (await store.Search("testrecord.json")).Count().Be(1);
        (await store.Search("testrecord.*")).Count().Be(1);
        (await store.Search("*.json")).Count().Be(1);
        (await store.Search("*.*")).Count().Be(1);
        (await store.Search("**.*")).Count().Be(1);

        (await store.Delete(path)).BeOk();

        (await store.Search("testrecord.json")).Count().Be(0);
        (await store.Search("testrecord.*")).Count().Be(0);
        (await store.Search("*.json")).Count().Be(0);
        (await store.Search("*.*")).Count().Be(0);
        (await store.Search("**.*")).Count().Be(0);
    }

    [Fact]
    public async Task ClearFolder()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "testrecord.json";
        int count = 15;

        var list = new Sequence<string>();

        for (int i = 0; i < count; i++)
        {
            string fullPath = i switch
            {
                < 5 => $"{i}-{path}",
                < 10 => $"folder1/{i}-{path}",
                _ => $"folder2/{i}-{path}"
            };
            list += fullPath;

            var testRecord = new TestRecord($"John Doe {i}", 30 + i);
            await store.Add(fullPath, testRecord.ToJson().ToDataETag());
        }

        foreach (var p in list)
        {
            (await store.Exists(p)).BeOk();
        }

        (await store.Search("testrecord.json")).Count().Be(0);
        (await store.Search("testrecord.*")).Count().Be(0);
        (await store.Search("*.json")).Count().Be(5);
        (await store.Search("*.*")).Count().Be(5);

        (await store.Search("**", includeFolder: true)).Action(x =>
        {
            x.Count().Be(17);
            x.Where(x => !x.IsFolder).Count().Be(15);
        });

        (await store.Search("**", includeFolder: false)).Count().Be(15);

        (await store.SearchDelete("**")).BeOk();
        (await store.Search("**")).Count().Be(0);

        (await store.Search("testrecord.json")).Count().Be(0);
        (await store.Search("testrecord.*")).Count().Be(0);
        (await store.Search("*.json")).Count().Be(0);
        (await store.Search("*.*")).Count().Be(0);
    }

    [Fact]
    public async Task SetFile_UpdateExistingFile_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "testrecord.json";
        var testRecord1 = new TestRecord("John Doe", 30);
        var testRecord2 = new TestRecord("Jane Smith", 35);

        (await store.Add(path, testRecord1.ToJson().ToDataETag())).BeOk();
        (await store.Get(path)).BeOk().Return().ToObject<TestRecord>().Be(testRecord1);

        (await store.Set(path, testRecord2.ToJson().ToDataETag())).BeOk();
        (await store.Get(path)).BeOk().Return().ToObject<TestRecord>().Be(testRecord2);

        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task GetDetails_ExistingFile_ReturnsPathDetails()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "details-test.json";
        var testRecord = new TestRecord("Test User", 25);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Exists(path)).BeOk();

        var detailsOption = await store.GetDetails(path);
        detailsOption.BeOk();
        var details = detailsOption.Return();

        details.Path.NotEmpty();
        details.IsFolder.BeFalse();
        details.ContentLength.Assert(x => x > 0);
        details.ETag.NotEmpty();

        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task GetDetails_NonExistingFile_ReturnsNotFound()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "nonexistent.json";
        var detailsOption = await store.GetDetails(path);
        detailsOption.BeNotFound();
    }

    [Fact]
    public async Task DeleteFolder_ShouldRemoveAllFilesInFolder()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string folderPath = "test-folder";
        var testRecord = new TestRecord("Test", 30);

        await store.Add($"{folderPath}/file1.json", testRecord.ToJson().ToDataETag());
        await store.Add($"{folderPath}/file2.json", testRecord.ToJson().ToDataETag());
        await store.Add($"{folderPath}/subfolder/file3.json", testRecord.ToJson().ToDataETag());

        (await store.Search($"{folderPath}/**", includeFolder: false)).Count().Be(3);
        (await store.Search($"{folderPath}/**", includeFolder: true)).Count().Be(4);

        (await store.SearchDelete(folderPath)).BeOk();

        (await store.Search($"{folderPath}/**")).Count().Be(0);
    }

    [Fact]
    public async Task Search_WithComplexPatterns_ShouldReturnMatchingFiles()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        var testRecord = new TestRecord("Test", 30);

        (await store.Add("root1.json", testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Add("root2.txt", testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Add("folder1/file1.json", testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Add("folder1/file2.json", testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Add("folder2/subfolder/file3.json", testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Add("hashfiles/a0/b3/hashfile4.json", testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Add("hashfiles/a0/b3/hashfile5.json", testRecord.ToJson().ToDataETag())).BeOk();
        (await store.Add("hashfiles/a5/b2/hashfile6.json", testRecord.ToJson().ToDataETag())).BeOk();

        (await store.Search("**")).Count().Be(8);
        (await store.Search("**", includeFolder: true)).Count().Be(16);
        (await store.Search("root*.json")).Count().Be(1);
        (await store.Search("folder1/*.json")).Count().Be(2);
        (await store.Search("folder2/**/*.json")).Count().Be(1);
        (await store.Search("**/file*.json")).Count().Be(3);
        (await store.Search("hashfiles/*/*/hashfile*.json")).Count().Be(3);

        await store.SearchDelete("**");
        (await store.Search("**")).Count().Be(0);
    }

    [Fact]
    public async Task ReleaseLease_NonExistentLease_ShouldSucceed()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "release-test.json";
        var testRecord = new TestRecord("Test", 30);

        (await store.Add(path, testRecord.ToJson().ToDataETag())).BeOk();

        (await store.ReleaseLease(path, Guid.NewGuid().ToString())).BeNotFound();

        (await store.Delete(path)).BeOk();
    }

    [Fact]
    public async Task Set_OnNonExistentFile_ShouldCreateFile()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        string path = "set-new-file.json";
        var testRecord = new TestRecord("New File", 25);

        (await store.Exists(path)).BeNotFound();

        (await store.Set(path, testRecord.ToJson().ToDataETag())).BeOk();

        (await store.Exists(path)).BeOk();
        (await store.Get(path)).BeOk().Return().ToObject<TestRecord>().Be(testRecord);

        (await store.Delete(path)).BeOk();
    }


    [Fact]
    public async Task Search_EmptyStore_ShouldReturnEmptyList()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        var results = await store.Search("**.*");
        results.Count.Be(0);

        results = await store.Search("nonexistent/*.json");
        results.Count.Be(0);
    }

    [Fact]
    public async Task Search_NonExistentFolder_ShouldReturnEmptyList()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        var testRecord = new TestRecord("Test", 30);
        await store.Add("existing/file.json", testRecord.ToJson().ToDataETag());

        var results = await store.Search("nonexistent/**/*");
        results.Count.Be(0);

        await store.SearchDelete("**");
    }

    [Fact]
    public async Task DeleteFolder_NonExistentFolder_ShouldBeNotFound()
    {
        var host = await BuildService();
        DatalakeStore store = host.Services.GetRequiredService<DatalakeStore>();

        (await store.SearchDelete("non-existent-folder")).BeNotFound();
    }
}
