using System.Collections;
using Toolbox.Extensions;

namespace Toolbox.Tools;

/// <summary>
/// Thread-safe builder for a CSS "class" attribute (a set of class names separated by spaces).
/// Class names are appended in the order specified; duplicates are collapsed only when rendered
/// (case-insensitive), with the last occurrence winning its position.
/// Designed for high frequency use; input is processed using <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public sealed class CssBuilder : IEnumerable<string>
{
    private const char Delimiter = ' ';
    private readonly object _lock = new();
    private readonly List<string> _classes = new();

    public CssBuilder() { }

    /// <summary>Create the builder pre-populated with one or more space separated class names.</summary>
    public CssBuilder(string? value) => Add(value);

    /// <summary>Number of class names currently aggregated.</summary>
    public int Count
    {
        get { lock (_lock) return _classes.Count; }
    }

    /// <summary>Add one or more space separated class names, ignored if empty.</summary>
    public CssBuilder Add(string? value)
    {
        if (value.IsNotEmpty()) AddInternal(value);
        return this;
    }

    /// <summary>Add class name(s) when <paramref name="include"/> is true and value is not empty.</summary>
    public CssBuilder Add(string? value, bool include) => include ? Add(value) : this;

    /// <summary>Add class name(s) when <paramref name="isInclude"/> returns true and value is not empty.</summary>
    public CssBuilder Add(string? value, Func<bool> isInclude)
    {
        ArgumentNullException.ThrowIfNull(isInclude);
        return value.IsNotEmpty() && isInclude() ? Add(value) : this;
    }

    /// <summary>Add a collection of class name values.</summary>
    public CssBuilder Add(IEnumerable<string?>? values)
    {
        if (values is null) return this;
        foreach (var value in values) Add(value);
        return this;
    }

    /// <summary>Remove a class name, all occurrences (case-insensitive). Returns true if any were present.</summary>
    public bool RemoveKey(string? key)
    {
        if (key.IsEmpty()) return false;

        lock (_lock)
        {
            int removed = _classes.RemoveAll(x => string.Equals(x, key, StringComparison.OrdinalIgnoreCase));
            return removed > 0;
        }
    }

    /// <summary>Remove all aggregated class names.</summary>
    public CssBuilder Clear()
    {
        lock (_lock) _classes.Clear();
        return this;
    }

    /// <summary>Build the resulting class attribute string, unique class names in the order specified (last occurrence wins).</summary>
    public override string ToString()
    {
        lock (_lock) return string.Join(Delimiter, DedupeLastWins(_classes));
    }

    public IEnumerator<string> GetEnumerator()
    {
        List<string> snapshot;
        lock (_lock) snapshot = DedupeLastWins(_classes);
        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void AddInternal(ReadOnlySpan<char> input)
    {
        lock (_lock)
        {
            foreach (Range range in input.Split(Delimiter))
            {
                ReadOnlySpan<char> token = input[range].Trim();
                if (token.IsEmpty) continue;

                _classes.Add(token.ToString());
            }
        }
    }

    /// <summary>Return unique class names preserving order; when a name repeats the last occurrence keeps its position.</summary>
    private static List<string> DedupeLastWins(List<string> source)
    {
        var result = new List<string>(source.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = source.Count - 1; i >= 0; i--)
        {
            if (seen.Add(source[i])) result.Add(source[i]);
        }

        result.Reverse();
        return result;
    }

    public static CssBuilder operator +(CssBuilder builder, string? value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(value);
    }

    public static CssBuilder operator +(CssBuilder builder, IEnumerable<string?>? values)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(values);
    }

    public static CssBuilder operator -(CssBuilder builder, string? key)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.RemoveKey(key);
        return builder;
    }

    public static bool operator ==(CssBuilder? left, CssBuilder? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;

        return left.Equals(right);
    }

    public static bool operator !=(CssBuilder? left, CssBuilder? right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is not CssBuilder other) return false;

        List<string> a, b;
        lock (_lock) a = DedupeLastWins(_classes);
        lock (other._lock) b = DedupeLastWins(other._classes);

        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (!string.Equals(a[i], b[i], StringComparison.OrdinalIgnoreCase)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        lock (_lock)
        {
            foreach (var name in DedupeLastWins(_classes)) hash.Add(name, StringComparer.OrdinalIgnoreCase);
        }

        return hash.ToHashCode();
    }
}
