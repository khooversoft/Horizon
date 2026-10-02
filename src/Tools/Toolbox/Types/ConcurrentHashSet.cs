using System.Collections;
using System.Collections.Concurrent;

namespace Toolbox.Types;

public class ConcurrentHashSet<T> : ICollection<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, byte> _dictionary;

    public ConcurrentHashSet(IEqualityComparer<T>? comparer = null) => _dictionary = new ConcurrentDictionary<T, byte>(comparer);

    public ConcurrentHashSet(IEnumerable<T> values, IEqualityComparer<T>? comparer = null) : this(comparer)
    {
        foreach (var value in values)
        {
            _dictionary[value] = 0;
        }

        Comparer = comparer;
    }

    public int Count => _dictionary.Count;
    public bool IsReadOnly => false;

    public IEqualityComparer<T>? Comparer { get; }

    public void Add(T item) => _dictionary[item] = 0;
    public void Clear() => _dictionary.Clear();
    public bool Remove(T item) => _dictionary.TryRemove(item, out _);
    public bool Contains(T item) => _dictionary.ContainsKey(item);
    public bool TryAdd(T item) => _dictionary.TryAdd(item, 0);
    public bool TryRemove(T item) => _dictionary.TryRemove(item, out _);

    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        T[] keys = _dictionary.Keys.ToArray();
        if (array.Length - arrayIndex < keys.Length) throw new ArgumentException("The destination array has insufficient space.", nameof(array));

        Array.Copy(keys, 0, array, arrayIndex, keys.Length);
    }

    public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal object[] ToImuttableArray()
    {
        throw new NotImplementedException();
    }
}
