using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toolbox.Types;

/// <summary>
/// Tags can be just a "tag" or "tag:value" or "tag:value1,value2,value3"
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[JsonConverter(typeof(KeyedCollectionJsonConverterFactory))]
public class KeyedCollection<TKey, TValue> : IEnumerable<KeyValues<TKey, TValue>>, IEquatable<KeyedCollection<TKey, TValue>> where TKey : notnull where TValue : notnull
{
    private readonly ConcurrentDictionary<TKey, ConcurrentHashSet<TValue>> _data;
    private readonly IEqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;
    private readonly IEqualityComparer<TValue>? _valueComparer = EqualityComparer<TValue>.Default;

    public KeyedCollection(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
    {
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
        _data = new ConcurrentDictionary<TKey, ConcurrentHashSet<TValue>>(keyComparer);
        _valueComparer = valueComparer;
    }

    [JsonConstructor]
    public KeyedCollection(IReadOnlyList<KeyValues<TKey, TValue>> items, IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
    {
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
        _valueComparer = valueComparer;
        _data = new ConcurrentDictionary<TKey, ConcurrentHashSet<TValue>>(_keyComparer);

        foreach (var item in items)
        {
            var set = new ConcurrentHashSet<TValue>(_valueComparer);
            foreach (var value in item.Values)
            {
                set.Add(value);
            }

            _data[item.Key] = set;
        }
    }

    public void Add(KeyValues<TKey, TValue> item) => _data.AddOrUpdate(
        item.Key,
        _ => new ConcurrentHashSet<TValue>(item.Values, _valueComparer),
        (k, x) =>
        {
            foreach (var value in item.Values)
            {
                x.Add(value);
            }

            return x;
        });

    public void Add(TKey key, params TValue[] values) => _data.AddOrUpdate(
        key,
        _ => new ConcurrentHashSet<TValue>(values, _valueComparer),
        (k, x) =>
        {
            foreach (var value in values)
            {
                x.Add(value);
            }

            return x;
        });

    public int Count => _data.Count;

    public bool ContainKey(TKey key) => _data.ContainsKey(key);
    public bool ContainsKey(TKey key) => ContainKey(key);

    public bool ContainKeyValue(TKey key, TValue value)
    {
        if (_data.TryGetValue(key, out var set))
        {
            return set.Contains(value);
        }

        return false;
    }

    public bool ContainsKeyValue(TKey key, TValue value) => ContainKeyValue(key, value);

    public IReadOnlyList<TValue> Get(TKey key) => _data.TryGetValue(key, out var set) ? set.ToImmutableArray() : Array.Empty<TValue>();

    public bool IsKeyFlag(TKey key)
    {
        if (_data.TryGetValue(key, out var set))
        {
            return set.Count == 0;
        }

        return false;
    }

    public bool RemoveKey(TKey key) => _data.TryRemove(key, out _);

    public bool RemoveKeyValue(TKey key, TValue value)
    {
        if (_data.TryGetValue(key, out var set))
        {
            return set.TryRemove(value);
        }

        return false;
    }

    public bool TryGetValue(TKey key, out IReadOnlyList<TValue> values)
    {
        if (_data.TryGetValue(key, out var set))
        {
            values = set.ToArray();
            return true;
        }

        values = Array.Empty<TValue>();
        return false;
    }

    public KeyedCollection<TKey, TValue> DeepClone(Func<TKey, TKey>? keyClone = null, Func<TValue, TValue>? valueClone = null)
    {
        keyClone ??= static x => x;
        valueClone ??= static x => x;

        var clone = new KeyedCollection<TKey, TValue>(_keyComparer, _valueComparer);
        foreach (var item in _data)
        {
            clone._data[keyClone(item.Key)] = new ConcurrentHashSet<TValue>(item.Value.Select(valueClone), _valueComparer);
        }

        return clone;
    }

    public bool Equals(KeyedCollection<TKey, TValue>? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        if (Count != other.Count) return false;

        foreach (var item in _data)
        {
            if (!other._data.TryGetValue(item.Key, out var otherSet)) return false;
            if (item.Value.Count != otherSet.Count) return false;

            foreach (var value in item.Value)
            {
                if (!otherSet.Contains(value)) return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is KeyedCollection<TKey, TValue> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = 0;

        foreach (var item in _data)
        {
            var setHash = 0;
            foreach (var value in item.Value)
            {
                setHash ^= _valueComparer?.GetHashCode(value) ?? value.GetHashCode();
            }

            hash ^= HashCode.Combine(_keyComparer.GetHashCode(item.Key), setHash);
        }

        return hash;
    }

    public static bool operator ==(KeyedCollection<TKey, TValue>? left, KeyedCollection<TKey, TValue>? right) =>
        EqualityComparer<KeyedCollection<TKey, TValue>>.Default.Equals(left, right);

    public static bool operator !=(KeyedCollection<TKey, TValue>? left, KeyedCollection<TKey, TValue>? right) => !(left == right);

    public IEnumerator<KeyValues<TKey, TValue>> GetEnumerator() => _data
        .Select(x => new KeyValues<TKey, TValue>(x.Key, x.Value.ToArray()))
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

//public static class KeyedCollectionTool
//{
//    public static KeyedCollection<TKey, TValue> ToKeyedCollection<TKey, TValue>(
//        string json,
//        IEqualityComparer<TKey>? keyComparer = null,
//        IEqualityComparer<TValue>? valueComparer = null
//        ) where TKey : notnull where TValue : notnull
//    {
//        IReadOnlyList<KeyValues<TKey, TValue>> items = JsonSerializer.Deserialize<IReadOnlyList<KeyValues<TKey, TValue>>>(json)
//            ?? Array.Empty<KeyValues<TKey, TValue>>();

//        return new KeyedCollection<TKey, TValue>(items.ToArray(), keyComparer, valueComparer);
//    }
//}


internal sealed class KeyedCollectionJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(KeyedCollection<,>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type[] typeArguments = typeToConvert.GetGenericArguments();
        Type converterType = typeof(KeyedCollectionJsonConverter<,>).MakeGenericType(typeArguments);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class KeyedCollectionJsonConverter<TKey, TValue> : JsonConverter<KeyedCollection<TKey, TValue>>
        where TKey : notnull where TValue : notnull
    {
        public override KeyedCollection<TKey, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            IReadOnlyList<KeyValues<TKey, TValue>>? items = JsonSerializer.Deserialize<IReadOnlyList<KeyValues<TKey, TValue>>>(ref reader, options);
            return items is null ? null : new KeyedCollection<TKey, TValue>(items);
        }

        public override void Write(Utf8JsonWriter writer, KeyedCollection<TKey, TValue> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToArray(), options);
        }
    }
}