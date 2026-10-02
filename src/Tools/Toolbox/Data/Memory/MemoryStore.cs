using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public partial class MemoryStore
{
    private readonly ConcurrentDictionary<string, DirectoryDetail> _store = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, LeaseRecord> _leaseStore = new(StringComparer.Ordinal);
    private readonly object _lock = new object();
    private readonly ILogger<MemoryStore> _logger;

    public MemoryStore(ILogger<MemoryStore> logger) => _logger = logger.NotNull();

    public Option<string> Add(string path, DataETag data)
    {
        if (!StorePathTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        _logger.LogDebug("Adding path={path}", path);
        data = data.WithHash();

        lock (_lock)
        {
            if (IsLeased(path).IsLocked()) return (StatusCode.Conflict, "Path is leased");

            DirectoryDetail detail = new(data.ConvertTo(path), data, null);

            Option<string> result = _store.TryAdd(path, detail) switch
            {
                true => detail.PathDetail.ETag.NotEmpty().ToOption(),
                false => StatusCode.Conflict,
            };

            _logger.LogDebug("Add Path={path}, length={length}", path, data.Data.Length);
            return result;
        }
    }

    public Option<string> Append(string path, DataETag data, string? leaseId)
    {
        if (!StorePathTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        _logger.LogDebug("Appending path={path}, leaseId={leaseId}", path, leaseId);
        data = data.WithHash();

        lock (_lock)
        {
            if (IsLeased(path, leaseId).IsLocked()) return (StatusCode.Conflict, "Path is leased");

            StorePathDetail pathDetail = data.ConvertTo(path);

            if (_store.TryGetValue(path, out var readPayload))
            {
                if (readPayload.PathDetail.IsFolder || readPayload.Data is null) return (StatusCode.Conflict, "Path already exists as a folder");

                DataETag dataETag = (readPayload.Data + data).WithHash();
                var newPayload = readPayload with
                {
                    PathDetail = readPayload.PathDetail with { ContentLength = dataETag.Data.Length },
                    Data = dataETag,
                };

                _store[path] = newPayload;

                _logger.LogDebug("Append Path={path}, length={length}", path, data.Data.Length);
                return newPayload.Data.ETag.NotEmpty();
            }

            DirectoryDetail detail = new(pathDetail, data);
            _store[path] = detail;
            return detail.Data.NotNull().ETag.NotEmpty();
        }
    }

    public Option CreateFolder(string path)
    {
        if (!StorePathTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");
        _logger.LogDebug("Creating folder path={path}", path);

        lock (_lock)
        {
            int start = 0;

            while (start < path.Length)
            {
                int slash = path.IndexOf('/', start);
                string currentPath = slash >= 0 ? path[..slash] : path;

                if (IsLeased(currentPath).IsLocked()) return (StatusCode.Conflict, "Path is leased");

                if (_store.TryGetValue(currentPath, out var existing))
                {
                    if (!existing.PathDetail.IsFolder) return (StatusCode.Conflict, "Path already exists as a file");
                }
                else
                {
                    var pathDetail = new StorePathDetail
                    {
                        Path = currentPath,
                        ContentLength = 0,
                        IsFolder = true,
                        CreatedOn = DateTimeOffset.UtcNow,
                        LastModified = DateTimeOffset.UtcNow,
                    };

                    _store[currentPath] = new DirectoryDetail(pathDetail);
                }

                if (slash < 0) break;
                start = slash + 1;
            }

            return StatusCode.OK;
        }
    }

    public Option Delete(string path, string? leaseId)
    {
        if (!StorePathTool.IsPathValid(path, true)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path, leaseId).IsLocked()) return (StatusCode.Locked, "Path is leased");

            Option result = _store.TryRemove(path, out var payload) switch
            {
                true when payload.LeaseRecord != null => _leaseStore.TryRemove(payload.LeaseRecord.LeaseId, out _) ? StatusCode.OK : StatusCode.Conflict,
                true => StatusCode.OK,
                _ => StatusCode.NotFound,
            };

            _logger.LogTrace("Remove Path={path}, leaseId={leaseId}, statusCode={statusCode}, error={error}", path, leaseId ?? "<no leaseId>", result.StatusCode, result.Error);
            return result;
        }
    }

    public Option SearchDelete(string pattern)
    {
        if (!StorePathTool.IsPathValid(pattern, true)) return (StatusCode.BadRequest, "Path is invalid");

        pattern = StorePathTool.AddRecursiveSafe(pattern);
        var matcher = new PathMatching(pattern.NotEmpty());

        var result = _store.Keys
            .Where(x => matcher.IsMatch(x))
            .Select(x => Delete(x, null))
            .ToList();

        return result.Count > 0 ? StatusCode.OK : StatusCode.NotFound;
    }

    public bool Exist(string path) => _store.ContainsKey(path);

    public Option<DataETag> Get(string path)
    {
        if (!StorePathTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        Option<DataETag> result = _store.TryGetValue(path, out var payload) switch
        {
            true when payload.Data is not null => payload.Data,
            true => StatusCode.NoContent,
            false => StatusCode.NotFound,
        };

        return result;
    }

    public Option<StorePathDetail> GetDetail(string path)
    {
        if (!StorePathTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        return _store.TryGetValue(path, out var payload) switch
        {
            true => payload.LeaseRecord switch
            {
                null => payload.PathDetail with { LeaseStatus = LeaseStatus.Unlocked, LeaseDuration = LeaseDuration.Infinite },
                var v => payload.PathDetail with
                {
                    LeaseDuration = v.Infinite ? LeaseDuration.Infinite : LeaseDuration.Fixed,
                    LeaseStatus = LeaseStatus.Locked,
                },
            },

            false => StatusCode.NotFound,
        };
    }

    public Option<string> Set(string path, DataETag data, string? leaseId)
    {
        if (!StorePathTool.IsPathValid(path)) return (StatusCode.BadRequest, "Path is invalid");

        lock (_lock)
        {
            if (IsLeased(path, leaseId).IsLocked()) return (StatusCode.Locked, "Path is leased");

            if (data.ETag.IsNotEmpty())
            {
                if (_store.TryGetValue(path, out var existing))
                {
                    if (existing.PathDetail.IsFolder || existing.Data is null) return (StatusCode.Conflict, "Path already exists as a folder");
                    if (existing.Data.ETag != data.ETag) return (StatusCode.Conflict, "ETag does not match");
                }
            }

            data = data.WithHash();

            var result = _store.AddOrUpdate(path,
                x =>
                {
                    var p = new DirectoryDetail(data.ConvertTo(x), data, null);
                    return p;
                },
                (x, current) =>
                {
                    if (current.PathDetail.IsFolder || current.Data is null) return current;

                    var payload = current with
                    {
                        Data = data,
                        PathDetail = current.PathDetail with
                        {
                            ContentLength = data.Data.Length,
                            LastModified = DateTimeOffset.UtcNow,
                            ETag = data.ETag.NotEmpty(),
                        },
                    };

                    return payload;
                });

            _logger.LogDebug("Set Path={path}, length={length}", path, data.Data.Length);
            if (result.PathDetail.IsFolder || result.Data is null) return (StatusCode.Conflict, "Path already exists as a folder");
            return result.PathDetail.ETag;
        }
    }

    public IReadOnlyList<StorePathDetail> Search(string pattern, bool includeFolder = false, int index = 0, int size = -1)
    {
        if (!StorePathTool.IsPathValid(pattern, true)) return [];

        index.Assert(x => x >= 0, "Index must be greater than or equal to zero");
        size.Assert(x => x == -1 || x > 0, "Size must be greater than zero or -1 for unlimited");

        var query = new PathMatching(pattern, StringComparison.Ordinal);
        int maxSize = size < 1 ? int.MaxValue : size;

        var list = _store.Values
            .Where(x => pattern == "*" || query.IsMatch(x.PathDetail.Path))
            .Where(x => includeFolder || !x.PathDetail.IsFolder)
            .Select(x => x.PathDetail with { Path = x.PathDetail.Path })
            .OrderBy(x => x.Path)
            .Skip(index)
            .Take(maxSize)
            .ToImmutableArray();

        return list;
    }

    public IReadOnlyList<(StorePathDetail Detail, DataETag Data)> SearchData(string pattern)
    {
        var query = new PathMatching(pattern, StringComparison.Ordinal);

        var list = _store.Values
            .Where(x => x.Data is not null)
            .Select(x => (Detail: x.PathDetail, Data: x.Data))
            .Where(x => pattern == "*" || query.IsMatch(x.Detail.Path))
            .Where(x => pattern == "*" || query.IsMatch(x.Detail.Path))
            .Select(x => (x.Detail, Data: x.Data!))
            .ToImmutableArray();

        return list;
    }
}
