using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

/// <summary>
/// Namespace identifies a root path and all folders under as part of a namespace.
/// Namespace is used to isolate data between different applications or users.
/// 
/// App paths are prefixing the namespace.  Example...
/// {namespace}/{path}
/// 
/// Search returns virtualize listed paths under the namespace.
/// 
/// file.json 
/// </summary>
public class FileStoreNamespace : IFileStore
{
    private readonly string _fileNamespace;
    private readonly string _namespacePrefix;
    private readonly IFileStore _fileStore;

    public FileStoreNamespace(string fileNamespace, IFileStore fileStore)
    {
        _fileNamespace = fileNamespace.NotEmpty();
        _fileStore = fileStore.NotNull();

        StorePathTool.IsPathValid(_fileNamespace).Assert(x => x, $"Invalid namespace path: {_fileNamespace}");
        _namespacePrefix = $"{_fileNamespace}".ToLowerInvariant();
    }

    public Task<Option<string>> Add(string path, DataETag data) => _fileStore.Add(BuildPath(path), data.NotNull());

    public Task<Option> CreateFolder(string path) => _fileStore.CreateFolder(BuildPath(path));

    public Task<Option> Delete(string pathOrPattern) => _fileStore.Delete(BuildPath(pathOrPattern));

    public Task<Option<DataETag>> Get(string path) => _fileStore.Get(BuildPath(path));

    public async Task<IReadOnlyList<StorePathDetail>> Search(string pattern, bool includeFolder = false)
    {
        var result = await _fileStore.Search(BuildPath(pattern, true), includeFolder);

        return result
            .Select(x => x with { Path = TrimNamespace(x.Path) })
            .ToArray();
    }

    public Task<Option> SearchDelete(string pattern) => _fileStore.SearchDelete(BuildPath(pattern, true));

    public Task<Option<string>> Upsert(string path, DataETag data) => _fileStore.Upsert(BuildPath(path), data.NotNull());

    private string BuildPath(string path, bool wildCard = false)
    {
        string newPath = StorePathTool.ToSafePath(path, wildCard);
        var relativePath = StorePathTool.ToSafePath($"{_fileNamespace}/{newPath}", wildCard);
        return relativePath;
    }

    /// <summary>
    /// Trim the namespace prefix from the path.  If the path does not start with the namespace prefix, throw an exception.
    /// 
    /// Valid patterns:
    /// {namespacePrefix} => throw exception
    /// {namespacePrefix}/ => throw exception
    /// {namespacePrefix}/path => path
    /// {namespacePrefix}/path/ => path
    /// {namespacePrefix}/path/p2 => path/p2
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string TrimNamespace(string path)
    {
        if (!path.StartsWith(_namespacePrefix + "/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Path '{path}' does not start with namespace prefix '{_namespacePrefix}'");
        }

        string relativePath = path[(_namespacePrefix.Length + 1)..].TrimEnd('/');
        if (relativePath.Length == 0)
        {
            throw new InvalidOperationException($"Path '{path}' does not start with namespace prefix '{_namespacePrefix}'");
        }

        return relativePath;
    }
}
