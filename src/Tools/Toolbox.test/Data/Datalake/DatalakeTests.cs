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

public class DatalakeTests
{
    private ITestOutputHelper _outputHelper;

    public DatalakeTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private async Task<IHost> BuildService([CallerMemberName] string function = "")
    {
        string basePath = nameof(DatalakeTests) + "/" + function;
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
    public async Task SimpleWriteAndRead()
    {
        using var host = await BuildService();
        var fileSystem = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/data.txt";

        var content = "Hello, World!".ToBytes();
        var setResult = await fileSystem.Set(path, new DataETag(content));
        setResult.BeOk();

        var readOption = await fileSystem.Get(path);
        readOption.BeOk();
        var readData = readOption.Return().Data;
        content.SequenceEqual(readData).BeTrue();

        var s1 = await fileSystem.Search("**/*.*");
        s1.Count.Be(1);
        s1[0].Path.Be(path);

        s1 = await fileSystem.Search("test/*.txt");
        s1.Count.Be(1);
        s1[0].Path.Be(path);

        var deleteOption = await fileSystem.Delete(path);
        deleteOption.BeOk();

        var s2 = await fileSystem.Search("**.*");
        s2.Count.Be(0);
    }

    [Fact]
    public async Task AddOperation_ShouldCreateNewKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/add-data.txt";
        var content = "Add operation test".ToBytes();

        var addResult = await keyStore.Add(path, new DataETag(content));
        addResult.BeOk();

        var readOption = await keyStore.Get(path);
        readOption.BeOk();
        content.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.Delete(path);
    }

    [Fact]
    public async Task AddOperation_ShouldFailWhenKeyExists()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/duplicate-data.txt";
        var content = "Duplicate test".ToBytes();

        var addResult = await keyStore.Add(path, new DataETag(content));
        addResult.BeOk();

        var duplicateResult = await keyStore.Add(path, new DataETag(content));
        duplicateResult.IsError().BeTrue();

        await keyStore.Delete(path);
    }

    [Fact]
    public async Task AppendOperation_ShouldAppendData()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/append-data.txt";
        var content1 = "First line".ToBytes();
        var content2 = "Second line".ToBytes();

        await keyStore.Set(path, new DataETag(content1));

        var appendResult = await keyStore.Append(path, new DataETag(content2));
        appendResult.BeOk();

        var readOption = await keyStore.Get(path);
        readOption.BeOk();
        var combined = content1.Concat(content2).ToArray();
        combined.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.Delete(path);
    }

    [Fact]
    public async Task SetOperation_ShouldOverwriteExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/overwrite-data.txt";
        var content1 = "Original content".ToBytes();
        var content2 = "Updated content".ToBytes();

        await keyStore.Set(path, new DataETag(content1));
        var setResult = await keyStore.Set(path, new DataETag(content2));
        setResult.BeOk();

        var readOption = await keyStore.Get(path);
        readOption.BeOk();
        content2.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.Delete(path);
    }

    [Fact]
    public async Task ExistsOperation_ShouldReturnTrueForExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/exists-data.txt";
        var content = "Exists test".ToBytes();

        await keyStore.Set(path, new DataETag(content));

        var existsResult = await keyStore.Exists(path);
        existsResult.BeOk();

        await keyStore.Delete(path);
    }

    [Fact]
    public async Task ExistsOperation_ShouldFailForNonExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/non-existing.txt";

        var existsResult = await keyStore.Exists(path);
        existsResult.IsError().BeTrue();
    }

    [Fact]
    public async Task GetDetailsOperation_ShouldReturnPathDetails()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/details-data.txt";
        var content = "Details test".ToBytes();

        await keyStore.Set(path, new DataETag(content));

        var detailsOption = await keyStore.GetDetails(path);
        detailsOption.BeOk();

        var details = detailsOption.Return();
        details.Path.EndsWith(path);
        details.IsFolder.BeFalse();

        await keyStore.Delete(path);
    }

    [Fact]
    public async Task DeleteFolderOperation_ShouldRemoveFolder()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string folder = "test/folder";
        string path1 = $"{folder}/file1.txt";
        string path2 = $"{folder}/file2.txt";

        await keyStore.Set(path1, "File 1".ToDataETag());
        await keyStore.Set(path2, "File 2".ToDataETag());

        (await keyStore.Search("**")).Count.Be(2);
        (await keyStore.SearchDelete(folder)).BeOk();
        (await keyStore.Search("**")).Count.Be(0);
        (await keyStore.Search("**", includeFolder: true)).Count.Be(1);
        (await keyStore.Search("**")).Where(x => !x.IsFolder).Count().Be(0);

        (await keyStore.Search($"{folder}/**")).Count.Be(0);
        (await keyStore.Search("**")).Where(x => !x.IsFolder).Count().Be(0);
    }

    [Fact]
    public async Task DeleteFolderOperation_ShouldRemoveOnlyFolder()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string folder = "test/folder";
        string path1 = $"{folder}/file1.txt";
        string path2 = $"{folder}/file2.txt";

        string folder2 = "test/folder2";
        string path3 = $"{folder2}/file3.txt";
        string path4 = $"{folder2}/file4.txt";

        await keyStore.Set(path1, "File 1".ToDataETag());
        await keyStore.Set(path2, "File 2".ToDataETag());
        await keyStore.Set(path3, "File 3".ToDataETag());
        await keyStore.Set(path4, "File 4".ToDataETag());

        (await keyStore.SearchDelete(folder)).BeOk();

        (await keyStore.Search("**")).Count.Be(2);
        (await keyStore.Search("**", includeFolder: true)).Count.Be(4);
        (await keyStore.Search("**")).Count.Assert(x => x >= 2, x => $"{x} is invalid");
        (await keyStore.Search($"{folder}/**")).Count.Be(0);
        (await keyStore.Search($"{folder2}/**")).Count.Be(2);
    }

    [Fact]
    public async Task SearchOperation_ShouldFindMultipleFiles()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        await keyStore.Set("test/search/file1.txt", "Data 1".ToDataETag());
        await keyStore.Set("test/search/file2.txt", "Data 2".ToDataETag());
        await keyStore.Set("test/search/file3.json", "Data 3".ToDataETag());

        var searchResult = await keyStore.Search("test/search/*.txt");
        searchResult.Count.Be(2);

        var allFiles = await keyStore.Search("test/search/*.*");
        allFiles.Count.Be(3);

        await keyStore.SearchDelete("test/search");
    }

    [Fact]
    public async Task AcquireLeaseOperation_ShouldLockKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/lease-data.txt";
        var content = "Lease test".ToBytes();

        await keyStore.Set(path, new DataETag(content));

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1));
        leaseOption.BeOk();
        var leaseId = leaseOption.Return();
        leaseId.NotEmpty();

        var releaseResult = await keyStore.ReleaseLease(path, leaseId);
        releaseResult.BeOk();

        await keyStore.Delete(path);
    }

    [Fact]
    public async Task AcquireExclusiveLockOperation_ShouldLockKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/exclusive-lock.txt";
        var content = "Exclusive lock test".ToBytes();

        await keyStore.Set(path, new DataETag(content));

        var lockOption = await keyStore.AcquireExclusiveLock(path, false);
        var leaseId = lockOption.BeOk().Return().NotEmpty();

        var releaseResult = await keyStore.ReleaseLease(path, leaseId);
        releaseResult.BeOk();

        await keyStore.Delete(path);
    }

    [Fact]
    public async Task BreakLeaseOperation_ShouldReleaseLockedKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/break-lease.txt";
        var content = "Break lease test".ToBytes();

        await keyStore.Set(path, new DataETag(content));

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1));
        leaseOption.BeOk();

        var breakResult = await keyStore.BreakLease(path);
        breakResult.BeOk();

        await keyStore.Delete(path);
    }

    [Fact]
    public async Task SetWithLeaseId_ShouldUpdateLockedKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/lease-update.txt";
        var content1 = "Original".ToBytes();
        var content2 = "Updated".ToBytes();

        await keyStore.Set(path, new DataETag(content1));

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1));
        leaseOption.BeOk();
        var leaseId = leaseOption.Return();

        var setResult = await keyStore.Set(path, new DataETag(content2), leaseId: leaseId);
        setResult.BeOk();

        var readOption = await keyStore.Get(path);
        readOption.BeOk();
        content2.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.ReleaseLease(path, leaseId);
        await keyStore.Delete(path);
    }

    [Fact]
    public async Task DeleteWithLeaseId_ShouldRemoveLockedKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/lease-delete.txt";
        var content = "Delete with lease".ToBytes();

        await keyStore.Set(path, new DataETag(content));

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1));
        leaseOption.BeOk();
        var leaseId = leaseOption.Return();

        var deleteResult = await keyStore.Delete(path, leaseId: leaseId);
        deleteResult.BeOk();

        var existsResult = await keyStore.Exists(path);
        existsResult.IsError().BeTrue();
    }

    [Fact]
    public async Task AppendWithLeaseId_ShouldAppendToLockedKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/lease-append.txt";
        var content1 = "First".ToBytes();
        var content2 = " Second".ToBytes();

        await keyStore.Set(path, new DataETag(content1));

        var leaseOption = await keyStore.AcquireLease(path, TimeSpan.FromMinutes(1));
        leaseOption.BeOk();
        var leaseId = leaseOption.Return();

        var appendResult = await keyStore.Append(path, new DataETag(content2), leaseId: leaseId);
        appendResult.BeOk();

        var readOption = await keyStore.Get(path);
        readOption.BeOk();
        var combined = content1.Concat(content2).ToArray();
        combined.SequenceEqual(readOption.Return().Data).BeTrue();

        await keyStore.ReleaseLease(path, leaseId);
        await keyStore.Delete(path);
    }

    [Fact]
    public async Task GetOperation_ShouldFailForNonExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/non-existing-get.txt";

        var readOption = await keyStore.Get(path);
        readOption.IsError().BeTrue();
    }

    [Fact]
    public async Task DeleteOperation_ShouldFailForNonExistingKey()
    {
        using var host = await BuildService();
        var keyStore = host.Services.GetRequiredService<DatalakeStore>();

        string path = "test/non-existing-delete.txt";

        var deleteResult = await keyStore.Delete(path);
        deleteResult.IsError().BeTrue();
    }
}
