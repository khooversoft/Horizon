using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Toolbox.Tools;

namespace Toolbox.Types;

public sealed record DataETag : IEquatable<DataETag>
{
    public DataETag(byte[] data) => Data = ImmutableArray.Create<byte>(data.NotNull());
    public DataETag(byte[] data, string? etag) => (Data, ETag) = (ImmutableArray.Create<byte>(data.NotNull()), etag);

    [JsonConstructor]
    public DataETag(ImmutableArray<byte> data, string? eTag) => (Data, ETag) = (data, eTag);

    public ImmutableArray<byte> Data { get; init; }
    public string? ETag { get; init; }

    public DataETag Append(DataETag append) => Data.Concat(append.Data).ToDataETag();

    public bool Equals(DataETag? other) => other is not null && !other.Data.IsDefault && Data.SequenceEqual(other.Data);
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in Data)
        {
            hash.Add(b);
        }
        return hash.ToHashCode();
    }

    public static implicit operator DataETag(byte[] data) => new(data);
    public static DataETag operator +(DataETag left, DataETag right) => left.Append(right);

    public static IValidator<DataETag> Validator { get; } = new Validator<DataETag>()
        .RuleFor(x => x.Data).NotNull()
        .RuleFor(x => x.Data).Must(x => x.Length > 0, x => $"Data {x.Length} is invalid")
        .Build();
}
