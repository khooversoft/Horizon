using System.Collections.Immutable;

namespace Toolbox.Types;

public record KeyValues<TKey, TValue> where TKey : notnull where TValue : notnull
{
    public KeyValues(TKey key, IReadOnlyList<TValue> values)
    {
        Key = key;
        Values = values.ToImmutableArray();
    }

    public TKey Key { get; }
    public IReadOnlyList<TValue> Values { get; }
}
