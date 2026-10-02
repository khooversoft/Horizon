using System.Text;
using System.Text.RegularExpressions;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class StringToolExtensions
{
    /// <summary>
    /// Matches input string against a wildcard pattern with case-insensitive comparison.
    /// Supports '*' (matches zero or more characters) and '?' (matches exactly one character).
    /// All other characters are matched literally.
    /// </summary>
    /// <param name="input">The string to match against the pattern. Returns false if null or empty.</param>
    /// <param name="pattern">The wildcard pattern to match. Returns false if null or empty.</param>
    /// <param name="useContains">If true and pattern contains no wildcards, uses Contains instead of Equals for matching.</param>
    /// <returns>True if the input matches the pattern; otherwise, false.</returns>
    public static bool Like(this string? input, string? pattern, bool useContains = false)
    {
        if (input.IsEmpty() || pattern.IsEmpty()) return false;

        if (!pattern.Any(x => x == '*' || x == '?')) return useContains switch
        {
            false => StringComparer.OrdinalIgnoreCase.Equals(input, pattern),
            true => input.Contains(pattern, StringComparison.OrdinalIgnoreCase),
        };

        var builder = new StringBuilder("^");

        foreach (char c in pattern)
        {
            switch (c)
            {
                case '*': builder.Append(".*"); break;
                case '?': builder.Append('.'); break;
                default: builder.Append(Regex.Escape(c.ToString())); break;
            }
        }

        builder.Append('$');

        bool result = Regex.IsMatch(input, builder.ToString(), RegexOptions.IgnoreCase);
        return result;
    }

    /// <summary>
    /// Test if string has wildcard characters '*' or '?'
    /// </summary>
    public static bool HasWildCard(this string? value) => value switch
    {
        null => false,
        string v when v.IsEmpty() => false,
        string v when v.IndexOfAny(['*', '?'], 0) >= 0 => true,
        _ => false,
    };

    public static string MergeKeyValuePairs(this string?[] styles, char delimiter = ';', char keyValueDelimiter = ':')
    {
        var merged = new List<KeyValuePair<string, string>>();
        var stylesList = (styles ?? []).NotNull().Where(x => x.IsNotEmpty()).OfType<string>();

        foreach (var style in stylesList)
        {
            foreach (var rule in style.Split(delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var separator = rule.IndexOf(keyValueDelimiter);
                if (separator <= 0) continue;

                var property = rule[..separator].Trim();
                var value = rule[(separator + 1)..].Trim();
                if (property.IsEmpty() || value.IsEmpty()) continue;

                merged.RemoveAll(x => x.Key.Equals(property, StringComparison.OrdinalIgnoreCase));
                merged.Add(new KeyValuePair<string, string>(property, value));
            }
        }

        return merged
            .Select(x => $"{x.Key}: {x.Value}")
            .Join("; ");
    }
}
