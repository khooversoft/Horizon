using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public enum LeaseStatus
{
    Locked,
    Unlocked,
}

public enum LeaseDuration
{
    Infinite,
    Fixed
}

public record StorePathDetail
{
    public string Path { get; init; } = null!;
    public long ContentLength { get; init; }
    public bool IsFolder { get; init; }
    public string ETag { get; init; } = null!;
    public DateTimeOffset? CreatedOn { get; init; }
    public DateTimeOffset? LastModified { get; init; }
    public LeaseStatus LeaseStatus { get; init; }
    public LeaseDuration LeaseDuration { get; init; }

    public string SizeK => $"{(int)(ContentLength / 1024)} KB";

    public string? GetFileName() => StorePathTool.GetFileName(Path);
}


public static class StorePathDetailTool
{
    public static StorePathDetail ConvertTo(this DatalakeStorePathDetail pathDetail) => new()
    {
        Path = pathDetail.Path,
        ContentLength = pathDetail.ContentLength,
        IsFolder = pathDetail.IsFolder,
        ETag = pathDetail.ETag,
        CreatedOn = pathDetail.CreatedOn,
        LastModified = pathDetail.LastModified
    };

    public static StorePathDetail ConvertTo(this DataETag dataETag, string path) => new StorePathDetail
    {
        Path = path,
        ContentLength = dataETag.Data.Length,
        ETag = dataETag.ETag.NotEmpty(),
    };
}