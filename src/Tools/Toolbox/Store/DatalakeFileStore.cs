using System.Collections.Immutable;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public class DatalakeFileStore : IFileStore
{
    private readonly DatalakeStore _datalakeStore;

    public DatalakeFileStore(DatalakeStore datalakeStore) => _datalakeStore = datalakeStore.NotNull();

    public async Task<Option<string>> Add(string path, DataETag data) => await _datalakeStore.Add(path, data);
    public async Task<Option> CreateFolder(string path) => await _datalakeStore.CreateFolder(path);
    public async Task<Option> Delete(string path) => await _datalakeStore.Delete(path);
    public async Task<Option<DataETag>> Get(string path) => await _datalakeStore.Get(path);

    public async Task<IReadOnlyList<StorePathDetail>> Search(string path, bool includeFolder = false)
    {
        var result = await _datalakeStore.Search(path, includeFolder: includeFolder);
        return result.Select(x => x.ConvertTo()).ToImmutableArray();
    }

    public async Task<Option> SearchDelete(string pattern) => await _datalakeStore.SearchDelete(pattern);
    public async Task<Option<string>> Upsert(string path, DataETag data) => await _datalakeStore.Upsert(path, data);
}