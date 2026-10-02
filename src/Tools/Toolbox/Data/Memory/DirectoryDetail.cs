using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public record DirectoryDetail
{
    public DirectoryDetail(StorePathDetail pathDetail, DataETag? data = null, LeaseRecord? leaseRecord = null)
    {
        PathDetail = pathDetail.NotNull();
        Data = data?.Action(x => x.ETag.NotEmpty());
        LeaseRecord = leaseRecord;
    }

    public StorePathDetail PathDetail { get; init; } = default!;
    public DataETag? Data { get; init; }
    public LeaseRecord? LeaseRecord { get; init; }
}
