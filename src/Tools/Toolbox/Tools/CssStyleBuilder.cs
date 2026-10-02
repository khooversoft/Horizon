using System.Collections;
using System.Text;
using Toolbox.Extensions;

namespace Toolbox.Tools;

/// <summary>
/// Thread-safe builder for a CSS "style" attribute (a set of "property: value" pairs separated by ';').
/// Properties are appended in the order specified; duplicates are collapsed only when rendered
/// (case-insensitive), with the last value/occurrence winning. A value is optional (a property may be
/// present with no value). Designed for high frequency use; input is processed using <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public sealed class CssStyleBuilder : IEnumerable<KeyValuePair<string, string?>>
{
    private const char Delimiter = ';';
    private const char KeyValueDelimiter = ':';
    private readonly object _lock = new();
    private readonly List<KeyValuePair<string, string?>> _styles = new();

    public CssStyleBuilder() { }

    /// <summary>Create the builder pre-populated with one or more ';' separated style declarations.</summary>
    public CssStyleBuilder(string? value) => Add(value);

    /// <summary>Number of style properties currently aggregated.</summary>
    public int Count
    {
        get { lock (_lock) return _styles.Count; }
    }

    /// <summary>Add one or more ';' separated style declaration(s), ignored if empty.</summary>
    public CssStyleBuilder Add(string? value)
    {
        if (value.IsNotEmpty()) AddInternal(value);
        return this;
    }

    /// <summary>Add style(s) when <paramref name="include"/> is true and value is not empty.</summary>
    public CssStyleBuilder Add(string? value, bool include) => include ? Add(value) : this;

    /// <summary>Add style(s) when <paramref name="isInclude"/> returns true and value is not empty.</summary>
    public CssStyleBuilder Add(string? value, Func<bool> isInclude)
    {
        ArgumentNullException.ThrowIfNull(isInclude);
        return value.IsNotEmpty() && isInclude() ? Add(value) : this;
    }

    /// <summary>Add a single property with an explicit (optional) value.</summary>
    public CssStyleBuilder Add(string property, string? value)
    {
        if (property.IsNotEmpty()) SetInternal(property.Trim(), value.IsEmpty() ? null : value!.Trim());
        return this;
    }

    /// <summary>Add a collection of style declaration values.</summary>
    public CssStyleBuilder Add(IEnumerable<string?>? values)
    {
        if (values is null) return this;
        foreach (var value in values) Add(value);
        return this;
    }

    /// <summary>Remove a style property by name, all occurrences (case-insensitive). Returns true if any were present.</summary>
    public bool RemoveKey(string? key)
    {
        if (key.IsEmpty()) return false;

        lock (_lock)
        {
            int removed = _styles.RemoveAll(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            return removed > 0;
        }
    }

    /// <summary>Remove all aggregated styles.</summary>
    public CssStyleBuilder Clear()
    {
        lock (_lock) _styles.Clear();
        return this;
    }

    /// <summary>Build the resulting style attribute string, unique properties in the order specified (last occurrence wins).</summary>
    public override string ToString()
    {
        lock (_lock)
        {
            if (_styles.Count == 0) return string.Empty;

            var builder = new StringBuilder();

            foreach (var style in DedupeLastWins(_styles))
            {
                if (builder.Length > 0) builder.Append(Delimiter).Append(' ');

                builder.Append(style.Key);
                if (style.Value.IsNotEmpty()) builder.Append(KeyValueDelimiter).Append(' ').Append(style.Value);
            }

            return builder.ToString();
        }
    }

    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
    {
        List<KeyValuePair<string, string?>> snapshot;
        lock (_lock) snapshot = DedupeLastWins(_styles);
        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void AddInternal(ReadOnlySpan<char> input)
    {
        foreach (Range range in input.Split(Delimiter))
        {
            ReadOnlySpan<char> rule = input[range].Trim();
            if (rule.IsEmpty) continue;

            int sep = rule.IndexOf(KeyValueDelimiter);

            string property;
            string? value;

            if (sep < 0)
            {
                property = rule.ToString();
                value = null;
            }
            else
            {
                property = rule[..sep].Trim().ToString();
                ReadOnlySpan<char> valueSpan = rule[(sep + 1)..].Trim();
                value = valueSpan.IsEmpty ? null : valueSpan.ToString();
            }

            if (property.Length == 0) continue;

            SetInternal(property, value);
        }
    }

    private void SetInternal(string property, string? value)
    {
        lock (_lock)
        {
            _styles.Add(new KeyValuePair<string, string?>(property, value));
        }
    }

    /// <summary>Return unique properties preserving order; when a property repeats the last occurrence (and its value) wins its position.</summary>
    private static List<KeyValuePair<string, string?>> DedupeLastWins(List<KeyValuePair<string, string?>> source)
    {
        var result = new List<KeyValuePair<string, string?>>(source.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = source.Count - 1; i >= 0; i--)
        {
            if (seen.Add(source[i].Key)) result.Add(source[i]);
        }

        result.Reverse();
        return result;
    }

    public static CssStyleBuilder operator +(CssStyleBuilder builder, string? value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(value);
    }

    public static CssStyleBuilder operator +(CssStyleBuilder builder, IEnumerable<string?>? values)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(values);
    }

    public static CssStyleBuilder operator -(CssStyleBuilder builder, string? key)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.RemoveKey(key);
        return builder;
    }

    public static bool operator ==(CssStyleBuilder? left, CssStyleBuilder? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;

        return left.Equals(right);
    }

    public static bool operator !=(CssStyleBuilder? left, CssStyleBuilder? right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is not CssStyleBuilder other) return false;

        List<KeyValuePair<string, string?>> a, b;
        lock (_lock) a = DedupeLastWins(_styles);
        lock (other._lock) b = DedupeLastWins(other._styles);

        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (!string.Equals(a[i].Key, b[i].Key, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(a[i].Value, b[i].Value, StringComparison.Ordinal)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        lock (_lock)
        {
            foreach (var style in DedupeLastWins(_styles))
            {
                hash.Add(style.Key, StringComparer.OrdinalIgnoreCase);
                hash.Add(style.Value);
            }
        }

        return hash.ToHashCode();
    }
}
