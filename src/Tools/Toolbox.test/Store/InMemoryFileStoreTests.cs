using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.test.Store;

public class InMemoryFileStoreTests
{
    private record TestData(string Name, int Age);

    [Fact]
    public async Task AddGetDelete_RoundTrip_WithConflictAndNotFound()
    {
        IFileStore fileStore = new InMemoryFileStore();
        string path = "files/test.json";
        var payload = new TestData("name", 15).ToDataETag(currentETag: "client-etag");

        (await fileStore.Get(path)).BeNotFound();

        string addETag = (await fileStore.Add(path, payload)).BeOk().Return();
        addETag.NotEmpty();

        (await fileStore.Add(path, payload)).BeConflict();

        var stored = (await fileStore.Get(path)).BeOk().Return();
        stored.DataToString().ToObject<TestData>().Be(new TestData("name", 15));
        stored.ETag.NotEmpty().Be(addETag);

        (await fileStore.Delete(path)).BeOk();
        (await fileStore.Get(path)).BeNotFound();
        (await fileStore.Delete(path)).BeNotFound();
    }

    [Fact]
    public async Task Upsert_CreateThenUpdate_ShouldOverwriteDataAndReturnNewETag()
    {
        IFileStore fileStore = new InMemoryFileStore();
        string path = "files/upsert.json";

        var original = new TestData("first", 1).ToDataETag();
        var updated = new TestData("second", 2).ToDataETag();

        string firstETag = (await fileStore.Upsert(path, original)).BeOk().Return();
        firstETag.NotEmpty();

        (await fileStore.Get(path)).BeOk().Return().DataToString().ToObject<TestData>().Be(new TestData("first", 1));

        string secondETag = (await fileStore.Upsert(path, updated)).BeOk().Return();
        secondETag.NotEmpty().NotBe(firstETag);

        var updatedData = (await fileStore.Get(path)).BeOk().Return();
        updatedData.DataToString().ToObject<TestData>().Be(new TestData("second", 2));
        updatedData.ETag.Be(secondETag);
    }

    [Fact]
    public async Task Search_ShouldReturnMatchingEntries_WithMetadata()
    {
        IFileStore fileStore = new InMemoryFileStore();

        var data1 = "one".ToDataETag();
        var data2 = "two".ToDataETag();
        var data3 = "three".ToDataETag();

        (await fileStore.Add("root/one.json", data1)).BeOk();
        (await fileStore.Add("root/two.txt", data2)).BeOk();
        (await fileStore.Add("root/nested/three.json", data3)).BeOk();

        var topJson = await fileStore.Search("root/*.json");
        topJson.Count().Be(1);
        var item1 = topJson.Single();
        item1.Path.Be("root/one.json");
        item1.ContentLength.Be(data1.Data.Length);
        item1.ETag.NotEmpty();

        var recursiveJson = await fileStore.Search("root/**/*.json");
        recursiveJson.Count().Be(2);
        recursiveJson.Select(x => x.Path).OrderBy(x => x).ToArray().Be(["root/nested/three.json", "root/one.json"]);

        var none = await fileStore.Search("root/*.xml");
        none.Count().Be(0);
    }

    [Fact]
    public async Task CreateFolder_ShouldCreateNestedFolderEntries()
    {
        IFileStore fileStore = new InMemoryFileStore();

        (await fileStore.CreateFolder("docs/nested")).BeOk();

        var rootFolder = (await fileStore.Search("docs", true)).Single();
        rootFolder.Path.Be("docs");
        rootFolder.IsFolder.BeTrue();

        var nestedFolder = (await fileStore.Search("docs/nested", true)).Single();
        nestedFolder.Path.Be("docs/nested");
        nestedFolder.IsFolder.BeTrue();
    }

    [Fact]
    public async Task CreateFolder_ShouldReturnConflict_WhenPathExistsAsFile()
    {
        IFileStore fileStore = new InMemoryFileStore();

        (await fileStore.Add("docs", "1".ToDataETag())).BeOk();

        (await fileStore.CreateFolder("docs/nested")).BeConflict();
    }

    [Fact]
    public async Task Add_WithNullPath_ShouldThrowArgumentNullException()
    {
        IFileStore fileStore = new InMemoryFileStore();
        var payload = new TestData("name", 10).ToDataETag();

        await Verify.ThrowsAsync<ArgumentNullException>(() => fileStore.Add(null!, payload));
    }

    [Fact]
    public async Task Add_WithNullData_ShouldThrowArgumentNullException()
    {
        IFileStore fileStore = new InMemoryFileStore();

        await Verify.ThrowsAsync<ArgumentNullException>(() => fileStore.Add("file.json", null!));
    }

    [Fact]
    public async Task Upsert_WithNullInputs_ShouldThrowArgumentNullException()
    {
        IFileStore fileStore = new InMemoryFileStore();
        var payload = new TestData("name", 10).ToDataETag();

        await Verify.ThrowsAsync<ArgumentNullException>(() => fileStore.Upsert(null!, payload));
        await Verify.ThrowsAsync<ArgumentNullException>(() => fileStore.Upsert("file.json", null!));
    }

    [Fact]
    public void Search_WithNullOrEmptyPattern_ShouldThrowArgumentNullException()
    {
        IFileStore fileStore = new InMemoryFileStore();

        Verify.Throws<ArgumentNullException>(() => fileStore.Search(null!));
        Verify.Throws<ArgumentNullException>(() => fileStore.Search(string.Empty));
    }
}
