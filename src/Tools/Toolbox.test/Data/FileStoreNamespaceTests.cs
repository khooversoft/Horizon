using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.test.Data;

public class FileStoreNamespaceTests
{
    private record TestData(string Name, int Age);

    private sealed class SearchStubFileStore(IReadOnlyList<StorePathDetail> result) : IFileStore
    {
        public Task<Option<string>> Add(string path, DataETag data) => throw new NotSupportedException();
        public Task<Option> CreateFolder(string path) => throw new NotSupportedException();
        public Task<Option> Delete(string path) => throw new NotSupportedException();
        public Task<Option> SearchDelete(string pattern) => throw new NotSupportedException();
        public Task<Option<DataETag>> Get(string path) => throw new NotSupportedException();
        public Task<IReadOnlyList<StorePathDetail>> Search(string pattern, bool includeFolder = false) => Task.FromResult(result);
        public Task<Option<string>> Upsert(string path, DataETag data) => throw new NotSupportedException();
    }

    [Fact]
    public void Constructor_WithInvalidArguments_ShouldThrow()
    {
        var fileStore = new InMemoryFileStore();

        Verify.Throws<ArgumentNullException>(() => new FileStoreNamespace(null!, fileStore));
        Verify.Throws<ArgumentNullException>(() => new FileStoreNamespace(string.Empty, fileStore));
        Verify.Throws<ArgumentException>(() => new FileStoreNamespace("/", fileStore));
        Verify.Throws<ArgumentNullException>(() => new FileStoreNamespace("tenant-a", null!));
    }

    [Fact]
    public async Task AddGetAndDelete_ShouldUseNamespacedPhysicalPath()
    {
        IFileStore backingStore = new InMemoryFileStore();
        IFileStore subject = new FileStoreNamespace("tenant-a", backingStore);
        string path = "files/test.json";
        var payload = new TestData("name", 15).ToDataETag();

        (await subject.Get(path)).BeNotFound();

        string etag = (await subject.Add(path, payload)).BeOk().Return();
        etag.NotEmpty();

        (await backingStore.Get("tenant-a/files/test.json")).BeOk().Return().DataToString().ToObject<TestData>().Be(new TestData("name", 15));
        (await backingStore.Get(path)).BeNotFound();

        (await subject.Get(path)).BeOk().Return().DataToString().ToObject<TestData>().Be(new TestData("name", 15));

        (await subject.Delete(path)).BeOk();
        (await backingStore.Get("tenant-a/files/test.json")).BeNotFound();
    }

    [Fact]
    public async Task Upsert_ShouldCreateThenUpdateWithinNamespace()
    {
        IFileStore backingStore = new InMemoryFileStore();
        IFileStore subject = new FileStoreNamespace("tenant-a", backingStore);
        string path = "files/upsert.json";

        string firstETag = (await subject.Upsert(path, new TestData("first", 1).ToDataETag())).BeOk().Return();
        firstETag.NotEmpty();

        string secondETag = (await subject.Upsert(path, new TestData("second", 2).ToDataETag())).BeOk().Return();
        secondETag.NotEmpty().NotBe(firstETag);

        (await backingStore.Get("tenant-a/files/upsert.json")).BeOk()
            .Return().DataToString().ToObject<TestData>().Be(new TestData("second", 2));

        (await subject.Get(path)).BeOk().Return().DataToString().ToObject<TestData>().Be(new TestData("second", 2));
    }

    [Fact]
    public async Task Search_ShouldReturnNamespaceRelativePaths()
    {
        IFileStore backingStore = new InMemoryFileStore();
        IFileStore subject = new FileStoreNamespace("tenant-a", backingStore);

        (await backingStore.Add("tenant-a/root/one.json", "one".ToDataETag())).BeOk();
        (await backingStore.Add("tenant-a/root/nested/two.json", "two".ToDataETag())).BeOk();
        (await backingStore.Add("tenant-a/root/three.txt", "three".ToDataETag())).BeOk();
        (await backingStore.Add("tenant-b/root/four.json", "four".ToDataETag())).BeOk();

        var result = await subject.Search("root/**/*.json");

        result.Count.Be(2);
        result.Select(x => x.Path).OrderBy(x => x).SequenceEqual(["root/nested/two.json", "root/one.json"]).BeTrue();
        result.All(x => !x.Path.StartsWith("tenant-a/", StringComparison.Ordinal)).BeTrue();
    }

    [Fact]
    public async Task Search_WithExactNamespacePath_ShouldThrowInvalidOperationException()
    {
        IFileStore subject = new FileStoreNamespace("tenant-a", new SearchStubFileStore([
            new StorePathDetail { Path = "tenant-a", IsFolder = true, ETag = "etag" }
        ]));

        await Verify.ThrowsAsync<InvalidOperationException>(() => subject.Search("**"));
    }

    [Fact]
    public async Task Search_WithNamespaceRootPath_ShouldThrowInvalidOperationException()
    {
        IFileStore subject = new FileStoreNamespace("tenant-a", new SearchStubFileStore([
            new StorePathDetail { Path = "tenant-a/", IsFolder = true, ETag = "etag" }
        ]));

        await Verify.ThrowsAsync<InvalidOperationException>(() => subject.Search("**"));
    }

    [Fact]
    public async Task Search_WithRelativePathEndingInSlash_ShouldTrimTrailingSlash()
    {
        IFileStore subject = new FileStoreNamespace("tenant-a", new SearchStubFileStore([
            new StorePathDetail { Path = "tenant-a/root/", IsFolder = true, ETag = "etag" }
        ]));

        var result = await subject.Search("**");

        result.Single().Path.Be("root");
    }

    [Fact]
    public async Task Search_WithNamespacePrefixWithoutSlash_ShouldThrowInvalidOperationException()
    {
        IFileStore subject = new FileStoreNamespace("tenant-a", new SearchStubFileStore([
            new StorePathDetail { Path = "tenant-aroot/one.json", ETag = "etag" }
        ]));

        await Verify.ThrowsAsync<InvalidOperationException>(() => subject.Search("**"));
    }

    [Fact]
    public async Task CreateFolder_ShouldCreateNamespacedFoldersAndReturnRelativePaths()
    {
        IFileStore backingStore = new InMemoryFileStore();
        IFileStore subject = new FileStoreNamespace("tenant-a", backingStore);

        (await subject.CreateFolder("docs/nested")).BeOk();

        (await backingStore.Search("tenant-a/docs", true)).Single().Path.Be("tenant-a/docs");
        (await backingStore.Search("tenant-a/docs/nested", true)).Single().Path.Be("tenant-a/docs/nested");

        (await subject.Search("docs", true)).Single().Path.Be("docs");
        (await subject.Search("docs/nested", true)).Single().Path.Be("docs/nested");
    }

    [Fact]
    public async Task SearchDelete_ShouldOnlyRemoveMatchingFilesWithinNamespace()
    {
        IFileStore backingStore = new InMemoryFileStore();
        IFileStore subject = new FileStoreNamespace("tenant-a", backingStore);

        (await backingStore.Add("tenant-a/docs/one.json", "1".ToDataETag())).BeOk();
        (await backingStore.Add("tenant-a/docs/two.txt", "2".ToDataETag())).BeOk();
        (await backingStore.Add("tenant-b/docs/one.json", "3".ToDataETag())).BeOk();

        (await subject.SearchDelete("docs/*.json")).BeOk();

        (await backingStore.Get("tenant-a/docs/one.json")).BeNotFound();
        (await backingStore.Get("tenant-a/docs/two.txt")).BeOk();
        (await backingStore.Get("tenant-b/docs/one.json")).BeOk();
    }

    [Fact]
    public async Task Methods_WithInvalidRelativePaths_ShouldThrowArgumentNullException()
    {
        IFileStore subject = new FileStoreNamespace("tenant-a", new InMemoryFileStore());
        var payload = new TestData("name", 10).ToDataETag();

        await Verify.ThrowsAsync<ArgumentException>(() => subject.Add(null!, payload));
        await Verify.ThrowsAsync<ArgumentException>(() => subject.Get(string.Empty));
        await Verify.ThrowsAsync<ArgumentException>(() => subject.Upsert(null!, payload));
        await Verify.ThrowsAsync<ArgumentException>(() => subject.Delete(string.Empty));
        await Verify.ThrowsAsync<ArgumentException>(() => subject.CreateFolder(null!));
        await Verify.ThrowsAsync<ArgumentException>(() => subject.Search(null!));
        await Verify.ThrowsAsync<ArgumentException>(() => subject.SearchDelete(string.Empty));
    }
}
