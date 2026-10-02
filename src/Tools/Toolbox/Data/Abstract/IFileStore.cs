using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IFileStore
{
    Task<Option<string>> Add(string path, DataETag data);
    Task<Option> CreateFolder(string path);
    Task<Option> Delete(string path);
    Task<Option> SearchDelete(string pattern);
    Task<Option<DataETag>> Get(string path);
    Task<IReadOnlyList<StorePathDetail>> Search(string pattern, bool includeFolder = false);
    Task<Option<string>> Upsert(string path, DataETag data);
}

public static class IFileSystemExtensions
{
    public static async Task<Option<string>> Add<T>(this IFileStore fileSystem, string path, T value) where T : class
    {
        path.NotEmpty();
        value.NotNull();

        DataETag data = value.ToDataETag();
        return await fileSystem.Add(path, data);
    }
    public static async Task<Option<string>> Upsert<T>(this IFileStore fileSystem, string path, T value) where T : class
    {
        path.NotEmpty();
        value.NotNull();

        DataETag data = value.ToDataETag();
        return await fileSystem.Upsert(path, data);
    }
}
