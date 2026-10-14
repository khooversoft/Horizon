//using System.Reflection;
//using System.Text;
//using Azure;
//using Azure.Storage.Files.DataLake;
//using Azure.Storage.Files.DataLake.Models;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Internal;
//using Microsoft.Extensions.Logging.Abstractions;
//using Toolbox.Data;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.test.Store;

//public class DatalakeFileStoreTests
//{
//    [Fact]
//    public async Task Get_WithoutCache_ShouldReadEveryTime()
//    {
//        var (store, client, _) = CreateStore();
//        using var subject = new DatalakeFileStore(store);

//        (await subject.Get("file.txt")).BeOk();
//        (await subject.Get("file.txt")).BeOk();

//        VerifyInt.Be(client.ReadCount, 2);
//    }

//    [Fact]
//    public async Task Get_ShouldCacheSuccessfulReadsUsingRealPath()
//    {
//        var (store, client, option) = CreateStore();
//        using var cache = new MemoryCache(new MemoryCacheOptions());
//        using var subject = new DatalakeFileStore(store, cache, option);

//        var first = (await subject.Get("Folder/File.txt")).BeOk().Return();
//        client.Content = "changed";
//        var second = (await subject.Get("ROOT/folder/file.txt")).BeOk().Return();

//        VerifyBool.BeTrue(second.Data.SequenceEqual(first.Data));
//        VerifyString.Be(second.ETag, first.ETag);
//        VerifyInt.Be(client.ReadCount, 1);
//        VerifyString.Be(client.LastPath, "root/folder/file.txt");
//    }

//    [Fact]
//    public async Task Get_ShouldExpireFiveMinutesAfterInsertionDespiteCacheHits()
//    {
//        var (store, client, option) = CreateStore();
//        var clock = new TestClock();
//        using var cache = new MemoryCache(new MemoryCacheOptions { Clock = clock });
//        using var subject = new DatalakeFileStore(store, cache, option);

//        (await subject.Get("file.txt")).BeOk();
//        client.Content = "changed";
//        clock.UtcNow += TimeSpan.FromMinutes(4);
//        (await subject.Get("file.txt")).BeOk();
//        VerifyInt.Be(client.ReadCount, 1);

//        clock.UtcNow += TimeSpan.FromMinutes(1);
//        var result = (await subject.Get("file.txt")).BeOk().Return();

//        VerifyString.Be(Encoding.UTF8.GetString(result.Data.AsSpan()), "changed");
//        VerifyInt.Be(client.ReadCount, 2);
//    }

//    [Fact]
//    public async Task Get_ShouldNotCacheMissingFiles()
//    {
//        var (store, client, option) = CreateStore();
//        using var cache = new MemoryCache(new MemoryCacheOptions());
//        using var subject = new DatalakeFileStore(store, cache, option);
//        client.Content = null;

//        (await subject.Get("file.txt")).BeNotFound();
//        client.Content = "created";
//        var result = (await subject.Get("file.txt")).BeOk().Return();

//        VerifyString.Be(Encoding.UTF8.GetString(result.Data.AsSpan()), "created");
//    }

//    [Fact]
//    public async Task Upsert_ShouldInvalidateTheNormalizedPath()
//    {
//        var (store, client, option) = CreateStore();
//        using var cache = new MemoryCache(new MemoryCacheOptions());
//        using var subject = new DatalakeFileStore(store, cache, option);

//        (await subject.Get("file.txt")).BeOk();
//        (await subject.Upsert("ROOT/FILE.TXT", new DataETag(Encoding.UTF8.GetBytes("updated")))).BeOk();
//        var result = (await subject.Get("file.txt")).BeOk().Return();

//        VerifyString.Be(Encoding.UTF8.GetString(result.Data.AsSpan()), "updated");
//        VerifyInt.Be(client.ReadCount, 2);
//    }

//    [Theory]
//    [InlineData(false)]
//    [InlineData(true)]
//    public async Task Delete_ShouldInvalidateEvenWhenFileIsAlreadyMissing(bool alreadyMissing)
//    {
//        var (store, client, option) = CreateStore();
//        using var cache = new MemoryCache(new MemoryCacheOptions());
//        using var subject = new DatalakeFileStore(store, cache, option);

//        (await subject.Get("file.txt")).BeOk();
//        if (alreadyMissing) client.Content = null;

//        var result = await subject.Delete("ROOT/FILE.TXT");
//        if (alreadyMissing) result.BeNotFound();
//        else result.BeOk();

//        (await subject.Get("file.txt")).BeNotFound();
//    }

//    [Fact]
//    public async Task SearchDelete_ShouldClearOnlyThisStoresEntriesEvenWhenNothingIsFound()
//    {
//        var (store, client, option) = CreateStore();
//        var (otherStore, otherClient, otherOption) = CreateStore();
//        using var cache = new MemoryCache(new MemoryCacheOptions());
//        using var subject = new DatalakeFileStore(store, cache, option);
//        using var otherSubject = new DatalakeFileStore(otherStore, cache, otherOption);
//        cache.Set("unrelated", "value");

//        (await subject.Get("file.txt")).BeOk();
//        (await otherSubject.Get("file.txt")).BeOk();
//        client.Content = "changed";
//        otherClient.Content = "other change";

//        (await subject.SearchDelete("folder")).BeNotFound();
//        var result = (await subject.Get("file.txt")).BeOk().Return();
//        var otherResult = (await otherSubject.Get("file.txt")).BeOk().Return();

//        VerifyString.Be(Encoding.UTF8.GetString(result.Data.AsSpan()), "changed");
//        VerifyString.Be(Encoding.UTF8.GetString(otherResult.Data.AsSpan()), "original");
//        VerifyInt.Be(client.ReadCount, 2);
//        VerifyInt.Be(otherClient.ReadCount, 1);
//        VerifyString.Be(cache.Get<string>("unrelated"), "value");
//    }

//    [Fact]
//    public void CachedConstructor_ShouldRejectNullDependencies()
//    {
//        var (store, _, option) = CreateStore();
//        using var cache = new MemoryCache(new MemoryCacheOptions());

//        Verify.Throws<ArgumentNullException>(() => new DatalakeFileStore(null!, cache, option));
//        Verify.Throws<ArgumentNullException>(() => new DatalakeFileStore(store, null!, option));
//        Verify.Throws<ArgumentNullException>(() => new DatalakeFileStore(store, cache, null!));
//    }

//    private static (DatalakeStore, TestFileClient, DatalakeOption) CreateStore()
//    {
//        var option = new DatalakeOption
//        {
//            Account = "testaccount",
//            Container = "files",
//            BasePath = "root",
//            Credentials = new ClientSecretOption
//            {
//                TenantId = "00000000-0000-0000-0000-000000000001",
//                ClientId = "00000000-0000-0000-0000-000000000002",
//                ClientSecret = "test-secret",
//            },
//        };
//        var store = new DatalakeStore(option, NullLogger<DatalakeStore>.Instance);
//        var client = new TestFileClient();
//        typeof(DatalakeStore).GetField("_fileSystem", BindingFlags.Instance | BindingFlags.NonPublic)
//            .NotNull().SetValue(store, new TestFileSystemClient(client));

//        return (store, client, option);
//    }

//    private sealed class TestClock : ISystemClock
//    {
//        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
//    }

//    private sealed class TestFileSystemClient(TestFileClient client) : DataLakeFileSystemClient
//    {
//        public override DataLakeFileClient GetFileClient(string fileName)
//        {
//            client.LastPath = fileName;
//            return client;
//        }

//        public override DataLakeDirectoryClient GetDirectoryClient(string directoryName) => new TestDirectoryClient();

//        public override AsyncPageable<PathItem> GetPathsAsync(DataLakeGetPathsOptions options, CancellationToken cancellationToken = default)
//            => AsyncPageable<PathItem>.FromPages([Page<PathItem>.FromValues([], null, null!)]);
//    }

//    private sealed class TestDirectoryClient : DataLakeDirectoryClient
//    {
//        public override Task<Response<bool>> ExistsAsync(CancellationToken cancellationToken = default)
//            => Task.FromResult(Response.FromValue(false, null!));
//    }

//    private sealed class TestFileClient() : DataLakeFileClient(new Uri("https://testaccount.dfs.core.windows.net/files/root/file.txt"))
//    {
//        public string? Content { get; set; } = "original";
//        public string? LastPath { get; set; }
//        public int ReadCount { get; private set; }

//        public override Task<Response<bool>> ExistsAsync(CancellationToken cancellationToken = default)
//            => Task.FromResult(Response.FromValue(Content != null, null!));

//        public override Task<Response<FileDownloadInfo>> ReadAsync(CancellationToken cancellationToken = default)
//        {
//            ReadCount++;
//            byte[] data = Encoding.UTF8.GetBytes(Content.NotNull());
//            var info = DataLakeModelFactory.FileDownloadInfo(
//                contentLength: data.Length,
//                content: new MemoryStream(data),
//                properties: DataLakeModelFactory.FileDownloadDetails(
//                    default, new Dictionary<string, string>(), null!, new ETag("test-etag"),
//                    null!, null!, null!, null!, default, null!, null!, null!, null!,
//                    default, default, default, default, null!, false, null!, null!));

//            return Task.FromResult(Response.FromValue(info, null!));
//        }

//        public override async Task<Response<PathInfo>> UploadAsync(Stream content, bool overwrite = false, CancellationToken cancellationToken = default)
//        {
//            using var reader = new StreamReader(content, leaveOpen: true);
//            Content = await reader.ReadToEndAsync(cancellationToken);
//            return Response.FromValue(DataLakeModelFactory.PathInfo(new ETag("updated-etag"), DateTimeOffset.UtcNow), null!);
//        }

//        public override Task<Response<bool>> DeleteIfExistsAsync(DataLakeRequestConditions conditions = null!, CancellationToken cancellationToken = default)
//        {
//            bool exists = Content != null;
//            Content = null;
//            return Task.FromResult(Response.FromValue(exists, null!));
//        }
//    }
//}
