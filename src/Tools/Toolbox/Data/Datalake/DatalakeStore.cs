using System.Collections.Immutable;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public partial class DatalakeStore
{
    private readonly DataLakeFileSystemClient _fileSystem;
    private readonly ILogger<DatalakeStore> _logger;
    private readonly DatalakeOption _datalakeOption;

    public DatalakeStore(DatalakeOption datalakeOption, ILogger<DatalakeStore> logger)
    {
        _datalakeOption = datalakeOption.NotNull().Action(x => x.Validate().ThrowOnError());
        _logger = logger.NotNull();

        DataLakeServiceClient serviceClient = datalakeOption.CreateDataLakeServiceClient();
        _fileSystem = serviceClient.GetFileSystemClient(datalakeOption.Container);
        //_fileSystem.Exists().Assert(x => x == true, $"Datalake file system does not exist, containerName={datalakeOption.Container}");
    }

    public async Task<Option> CreateFolder(string path)
    {
        path.NotEmpty();
        path = _datalakeOption.WithBasePath(path);

        using var metric = _logger.LogDuration("dataLakeStore-CreateFolder", "path={path}", path);
        _logger.LogDebug("Creating directory {path}", path);

        try
        {
            string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string currentPath = string.Empty;

            foreach (string part in parts)
            {
                currentPath = currentPath.Length == 0 ? part : $"{currentPath}/{part}";

                DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(currentPath);
                Response<PathInfo?> response = await directoryClient.CreateIfNotExistsAsync();

                if (response != null)
                {
                    int status = response.GetRawResponse().Status;
                    if (status is not 200 and not 201)
                    {
                        return (StatusCode.Conflict, response.GetRawResponse().ReasonPhrase);
                    }
                }
            }

            return StatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create directory for {path}", path);
        }

        return StatusCode.BadRequest;
    }

    public async Task<Option> SearchDelete(string path)
    {
        path.NotEmpty();
        using var metric = _logger.LogDuration("dataLakeStore-deleteDirectory", "path={path}", path);

        path = _datalakeOption.WithBasePath(path);
        _logger.LogDebug("Deleting directory {path}", path);

        var queue = new Queue<Func<Task<Option>>>();

        switch (path.IndexOf("*/*/"))
        {
            case -1:
                queue.Enqueue(() => InternalDeleteFolder(path));
                queue.Enqueue(() => InternalSearchDelete(path));
                break;

            default:
                queue.Enqueue(() => InternalSearchDelete(path));
                queue.Enqueue(() => InternalDeleteFolder(path));
                break;
        }

        while (queue.Count > 0)
        {
            var action = queue.Dequeue();
            Option result = await action().ConfigureAwait(false);
            if (result.IsOk()) return result;
        }

        return StatusCode.NotFound;
    }

    public async Task<Option<DatalakeStorePathDetail>> GetDetails(string path)
    {
        var fileClient = GetFileClient(path);

        _logger.LogDebug("Getting path {path} properties", fileClient.Path);
        using var metric = _logger.LogDuration("dataLakeStore-getPathProperties");

        try
        {
            Response<bool> exist = await fileClient.ExistsAsync();
            if (!exist.HasValue || !exist.Value)
            {
                _logger.LogDebug("File does not exist, path={path}", fileClient.Path);
                return new Option<DatalakeStorePathDetail>(StatusCode.NotFound);
            }

            var result = await fileClient.GetPropertiesAsync();
            return result.Value.ConvertTo(fileClient.Path).ToOption();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to GetPathProperties for file {path}", fileClient.Path);
            return (StatusCode.NotFound, ex.Message);
        }
    }

    public async Task<IReadOnlyList<DatalakeStorePathDetail>> Search(string pattern, int index = 0, int size = -1, bool includeFolder = false)
    {
        var basePattern = _datalakeOption.WithBasePath(pattern);

        var matcher = new PathMatching(basePattern);
        using var metric = _logger.LogDuration("dataLakeStore-search", "pattern={pattern}", basePattern);

        var list = new Sequence<DatalakeStorePathDetail>();
        string basePath = StorePathTool.GetRootPath(basePattern);
        bool recurse = matcher.IsRecursive;
        int maxSize = size < 1 ? int.MaxValue : size;

        int count = 0;
        int scanCount = 0;
        try
        {
            var options = new DataLakeGetPathsOptions { Path = basePath, Recursive = recurse };
            await foreach (PathItem pathItem in _fileSystem.GetPathsAsync(options))
            {
                scanCount++;
                if (!matcher.IsMatch(pathItem.Name)) continue;
                if (!includeFolder && (pathItem.IsDirectory ?? false)) continue;
                if (count++ < index) continue;

                string trimmedPath = _datalakeOption.RemoveBaseRoot(pathItem.Name);
                list += pathItem.ConvertTo(trimmedPath);
                if (list.Count >= maxSize) break;
            }

            return list.ToImmutableArray();
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PathNotFound")
        {
            return Array.Empty<DatalakeStorePathDetail>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to search, pattern={pattern}", basePattern);
            return Array.Empty<DatalakeStorePathDetail>();
        }
    }

    private DataLakeFileClient GetFileClient(string path)
    {
        path.NotEmpty();

        var p1 = _datalakeOption.WithBasePath(path);
        var fullPath = _fileSystem.GetFileClient(p1);
        return fullPath;
    }

    public async Task<Option> InternalDeleteFolder(string path)
    {
        path.NotEmpty();
        path = _datalakeOption.WithBasePath(path);
        using var metric = _logger.LogDuration("dataLakeStore-deleteDirectory", "path={path}", path);
        _logger.LogDebug("Deleting directory {path}", path);

        try
        {
            DataLakeDirectoryClient directoryClient = _fileSystem.GetDirectoryClient(path);
            var existResponse = await directoryClient.ExistsAsync();
            if (!existResponse.Value)
            {
                _logger.LogDebug("Directory does not exist, path={path}", path);
                return StatusCode.NotFound;
            }

            var response = await directoryClient.DeleteAsync(recursive: true);
            if (response.Status != 200) return (StatusCode.Conflict, response.ReasonPhrase);

            return StatusCode.OK;
        }
        catch (RequestFailedException ex) when (ex.Status == 401 || ex.ErrorCode == "PathNotFound")
        {
            _logger.LogError(ex, "Failed to delete directory for {path}", path);
            return StatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete directory for {path}", path);
        }

        return StatusCode.BadRequest;
    }

    private async Task<Option> InternalSearchDelete(string path)
    {
        path.NotEmpty();
        var fullPath = StorePathTool.AddRecursiveSafe(path);
        IReadOnlyList<DatalakeStorePathDetail> pathItems = await Search(fullPath);

        IReadOnlyList<DatalakeStorePathDetail> orderedItems = pathItems
            .OrderBy(x => x.IsFolder) // files first, folders last
            .ThenByDescending(x => x.Path.Count(c => c == '/')) // deepest paths first
            .ToList();

        if (orderedItems.Count == 0) return StatusCode.NotFound;

        foreach (DatalakeStorePathDetail item in orderedItems)
        {
            if (item.IsFolder)
            {
                Option deleteFolderOption = await InternalDeleteFolder(item.Path);
                if (deleteFolderOption.IsError()) return deleteFolderOption;
            }
            else
            {
                Option deleteOption = await Delete(item.Path).ConfigureAwait(false);
                if (deleteOption.IsError()) return deleteOption;
            }
        }

        return StatusCode.OK;
    }
}
