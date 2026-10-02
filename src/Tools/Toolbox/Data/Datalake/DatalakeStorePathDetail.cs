using Toolbox.Extensions;

namespace Toolbox.Data;


public record DatalakeStorePathDetail
{
    public string Path { get; init; } = null!;
    public bool IsFolder { get; init; }
    public long ContentLength { get; init; }
    public DateTimeOffset? CreatedOn { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastModified { get; init; } = DateTimeOffset.UtcNow;
    public string ETag { get; init; } = null!;
    public LeaseStatus LeaseStatus { get; init; }
    public LeaseDuration LeaseDuration { get; init; }
    public string? ContentHash { get; init; }

    public string SizeK => $"{(int)(ContentLength / 1024)} KB";

    public string GetFileName()
    {
        if (Path.IsEmpty()) return string.Empty;

        int lastSlashIndex = Path.LastIndexOf('/');
        return lastSlashIndex >= 0 ? Path[(lastSlashIndex + 1)..] : Path;
    }
}
