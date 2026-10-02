using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.test.Store;

public class FileStoreInMemoryTests
{
    private record TestData(string Name, int Age);

    private IHost BuildHost()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddInMemoryStore();
            })
            .Build();

        return host;
    }

    private IFileStore BuildStore()
    {
        var host = BuildHost();
        return host.Services.GetRequiredService<IFileStore>();
    }

    [Fact]
    public void ResolveFromContainer()
    {
        var fileStore = BuildStore();
        fileStore.NotNull();
        fileStore.GetType().Be(typeof(InMemoryFileStore));
    }

    [Fact]
    public async Task SimpleRoundTrip()
    {
        IFileStore fileStore = BuildStore();

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
        IFileStore fileStore = BuildStore();
        string path = "files/upsert.json";

        var original = new TestData("first", 1).ToDataETag();
        var updated = new TestData("second", 2).ToDataETag();

        string firstETag = (await fileStore.Upsert(path, original)).BeOk().Return();
        firstETag.NotEmpty();

        (await fileStore.Get(path)).BeOk().Return().DataToString().ToObject<TestData>().Be(new TestData("first", 1));

        string secondETag = (await fileStore.Upsert(path, updated)).BeOk().Return();
        secondETag.NotEmpty().NotBe(firstETag);

        var stored = (await fileStore.Get(path)).BeOk().Return();
        stored.DataToString().ToObject<TestData>().Be(new TestData("second", 2));
        stored.ETag.Be(secondETag);
    }

    [Fact]
    public async Task Search_ShouldReturnMatchingEntries_WithMetadata()
    {
        IFileStore fileStore = BuildStore();

        var data1 = "one".ToDataETag();
        var data2 = "two".ToDataETag();
        var data3 = "three".ToDataETag();

        (await fileStore.Add("root/one.json", data1)).BeOk();
        (await fileStore.Add("root/two.txt", data2)).BeOk();
        (await fileStore.Add("root/nested/three.json", data3)).BeOk();

        var topJson = await fileStore.Search("root/*.json");
        topJson.Count.Be(1);
        var item = topJson.Single();
        item.Path.Be("root/one.json");
        item.ContentLength.Be(data1.Data.Length);
        item.ETag.NotEmpty();

        var recursiveJson = await fileStore.Search("root/**/*.json");
        recursiveJson.Count.Be(2);
        recursiveJson.Select(x => x.Path).OrderBy(x => x).ToArray().Be(["root/nested/three.json", "root/one.json"]);

        (await fileStore.Search("root/*.xml")).Count.Be(0);
    }

    [Fact]
    public async Task Delete_ByPattern_ShouldRemoveAllMatchingEntries()
    {
        IFileStore fileStore = BuildStore();

        (await fileStore.Add("docs/one.json", "1".ToDataETag())).BeOk();
        (await fileStore.Add("docs/two.json", "2".ToDataETag())).BeOk();
        (await fileStore.Add("docs/three.txt", "3".ToDataETag())).BeOk();

        (await fileStore.SearchDelete("docs/*.json")).BeOk();

        (await fileStore.Get("docs/one.json")).BeNotFound();
        (await fileStore.Get("docs/two.json")).BeNotFound();
        (await fileStore.Get("docs/three.txt")).BeOk();
    }

    [Fact]
    public async Task Delete_WithNoMatch_ShouldReturnNotFound()
    {
        IFileStore fileStore = BuildStore();

        (await fileStore.Add("docs/one.json", "1".ToDataETag())).BeOk();

        (await fileStore.Delete("docs/*.txt")).BeNotFound();
        (await fileStore.Get("docs/one.json")).BeOk();
    }

    [Fact]
    public async Task GenericExtensions_AddAndUpsert()
    {
        IFileStore fileStore = BuildStore();
        string path = "entity/person.json";

        string firstETag = (await fileStore.Add(path, new TestData("one", 1))).BeOk().Return();
        firstETag.NotEmpty();
        (await fileStore.Add(path, new TestData("two", 2))).BeConflict();

        string secondETag = (await fileStore.Upsert(path, new TestData("two", 2))).BeOk().Return();
        secondETag.NotEmpty().NotBe(firstETag);

        (await fileStore.Get(path)).BeOk().Return().DataToString().ToObject<TestData>().Be(new TestData("two", 2));
    }

    [Fact]
    public async Task GenericExtensions_WithInvalidInput_ShouldThrow()
    {
        IFileStore fileStore = BuildStore();

        await Verify.ThrowsAsync<ArgumentNullException>(() => fileStore.Add<TestData>(null!, new TestData("one", 1)));
        await Verify.ThrowsAsync<ArgumentNullException>(() => fileStore.Add<TestData>("entity/person.json", null!));

        await Verify.ThrowsAsync<ArgumentNullException>(() => fileStore.Upsert<TestData>(null!, new TestData("one", 1)));
        await Verify.ThrowsAsync<ArgumentNullException>(() => fileStore.Upsert<TestData>("entity/person.json", null!));
    }
}
