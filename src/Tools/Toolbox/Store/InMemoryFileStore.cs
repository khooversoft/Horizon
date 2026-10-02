using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class InMemoryFileStore : IFileStore
{
    private readonly MemoryStore _memoryStore;

    public InMemoryFileStore() : this(new MemoryStore(NullLogger<MemoryStore>.Instance))
    {
    }

    public InMemoryFileStore(MemoryStore memoryStore)
    {
        _memoryStore = memoryStore.NotNull();
    }

    public Task<Option<string>> Add(string path, DataETag data) => _memoryStore.Add(path.NotEmpty(), data.NotNull()).ToTaskResult();
    public Task<Option> CreateFolder(string path) => _memoryStore.CreateFolder(path.NotEmpty()).ToTaskResult();
    public Task<Option> Delete(string path) => _memoryStore.Delete(path, null).ToTaskResult();
    public Task<Option<DataETag>> Get(string path) => _memoryStore.Get(path.NotEmpty()).ToTaskResult();
    public Task<IReadOnlyList<StorePathDetail>> Search(string pattern, bool includeFolder = false) => _memoryStore.Search(pattern.NotEmpty(), includeFolder).ToTaskResult();
    public Task<Option> SearchDelete(string pattern) => _memoryStore.SearchDelete(pattern.NotEmpty()).ToTaskResult();
    public Task<Option<string>> Upsert(string path, DataETag data) => _memoryStore.Set(path.NotEmpty(), data.NotNull(), null).ToTaskResult();
}
