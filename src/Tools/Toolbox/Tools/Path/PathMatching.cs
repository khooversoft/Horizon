using System.Text;

namespace Toolbox.Tools;

/// <summary>
/// Pattern Meaning
/// 
/// *       Matches zero or more characters in a file or folder name(but not / or \).
/// ?       Matches exactly one character(except / or \).
/// **      Matches zero or more directory levels (recursive).
/// {a,b}	Matches either a or b (brace expansion).
/// [abc]   Matches any one of the characters a, b, or c.
/// [!abc]  Matches any character except a, b, or c.
/// 
/// Matching is case-sensitive by default (StringComparison.Ordinal); pass StringComparison.OrdinalIgnoreCase to ignore case.
/// Backslashes are normalized to '/', repeated slashes are collapsed, and a leading '/' in the pattern anchors to the root.
/// Brace expansion {} works for multiple extensions or names (literal alternatives only).
/// 
/// Examples:
/// 
/// one.txt             Exact match
/// dir/two.txt         Exact match
/// *.txt               Match all .txt files
/// ?.txt               Match any single-character .txt file name
/// file[1-3].txt       Match file1.txt, file2.txt, file3.txt
/// [!tT]emp*           Match all files except those starting with "temp"
/// *.{jpg,png}         Match either .jpg or .png files
/// *.txt	            All files with .txt file extension.
/// *.*                 All files with an extension.
/// *	                All files in top-level (root) directory only.
/// /*	                Same as '*': all files in the root directory only.
/// .*	                File names beginning with '.'.
/// *word*              All files with 'word' in the filename.
/// readme.*            All files named 'readme' with any file extension.
/// styles/*.css	    All files with extension '.css' in the directory 'styles/'.
/// scripts/*/*         All files exactly one subdirectory level under 'scripts/' (not files directly in 'scripts/').
/// images*/*	        All files in a folder with name that is or begins with 'images'.
/// **/*	            All files in any subdirectory.
/// dir/**/*            All files in any subdirectory under 'dir/' recursively.
/// dir/*	            Direct children of 'dir/' only (not items in subfolders).
/// d1/d2/*	            Direct children of 'd1/d2/' only (not items in subfolders).
/// 
/// </summary>
public class PathMatching
{
    private readonly string _pattern;
    private readonly StringComparison _comparison;

    public PathMatching(string pattern, StringComparison comparison = StringComparison.Ordinal)
    {
        pattern.NotEmpty();

        string normalized = NormalizePath(pattern);
        if (normalized.Length > 1 && normalized[0] == '/') normalized = normalized[1..];

        // Extract the static base path prefix (before any glob characters)
        int firstGlob = normalized.IndexOfAny(['*', '?', '[', '{']);
        if (firstGlob >= 0)
        {
            int lastSlash = normalized.LastIndexOf('/', firstGlob);
            BasePath = lastSlash >= 0 ? normalized[..lastSlash] : string.Empty;
        }
        else
        {
            BasePath = normalized;
        }

        IsRecursive = CalculateIsRecursive(normalized);

        _pattern = normalized;
        _comparison = comparison;
    }

    public string BasePath { get; }
    public bool IsRecursive { get; }

    public bool IsMatch(string path)
    {
        string normalizedFileName = NormalizePath(path);
        return GlobMatch(_pattern, normalizedFileName, _comparison);
    }

    private static bool CalculateIsRecursive(string normalizedPattern)
    {
        ReadOnlySpan<char> span = normalizedPattern;

        // "**" anywhere means recursive
        if (span.IndexOf("**".AsSpan()) >= 0)
            return true;

        // Two or more '*' within the directory portion also means recursive
        int lastSlashIndex = span.LastIndexOf('/');
        ReadOnlySpan<char> directoryPart = lastSlashIndex >= 0 ? span[..lastSlashIndex] : ReadOnlySpan<char>.Empty;

        int first = directoryPart.IndexOf('*');
        return first >= 0 && directoryPart[(first + 1)..].IndexOf('*') >= 0;
    }

    /// <summary>
    /// Comparison-aware backtracking glob matcher operating directly over spans (no regex allocation).
    /// Supports: * (non-slash run), ? (single non-slash), ** (recursive), [abc]/[!abc]/[a-z] classes,
    /// and {a,b} brace expansion of literal alternatives.
    /// </summary>
    private static bool GlobMatch(ReadOnlySpan<char> pattern, ReadOnlySpan<char> path, StringComparison comparison)
    {
        while (!pattern.IsEmpty)
        {
            char c = pattern[0];

            switch (c)
            {
                case '*':
                    if (pattern.Length >= 2 && pattern[1] == '*')
                    {
                        if (pattern.Length >= 3 && pattern[2] == '/')
                        {
                            // **/ -> optional recursive directory prefix: match nothing, or any run ending in '/'
                            ReadOnlySpan<char> afterSlash = pattern[3..];
                            if (GlobMatch(afterSlash, path, comparison)) return true;

                            for (int k = 0; k < path.Length; k++)
                            {
                                if (path[k] == '/' && GlobMatch(afterSlash, path[(k + 1)..], comparison)) return true;
                            }

                            return false;
                        }

                        // ** -> match everything remaining, including slashes
                        ReadOnlySpan<char> afterStars = pattern[2..];
                        for (int k = 0; k <= path.Length; k++)
                        {
                            if (GlobMatch(afterStars, path[k..], comparison)) return true;
                        }

                        return false;
                    }
                    else
                    {
                        // * -> zero or more non-slash characters
                        ReadOnlySpan<char> rest = pattern[1..];
                        for (int k = 0; k <= path.Length; k++)
                        {
                            if (GlobMatch(rest, path[k..], comparison)) return true;
                            if (k < path.Length && path[k] == '/') break;
                        }

                        return false;
                    }

                case '?':
                    if (path.IsEmpty || path[0] == '/') return false;
                    pattern = pattern[1..];
                    path = path[1..];
                    break;

                case '[':
                    if (path.IsEmpty || path[0] == '/') return false;
                    if (!MatchCharClass(ref pattern, path[0], comparison)) return false;
                    path = path[1..];
                    break;

                case '{':
                    return MatchBrace(pattern, path, comparison);

                default:
                    if (path.IsEmpty || !CharEqual(path[0], c, comparison)) return false;
                    pattern = pattern[1..];
                    path = path[1..];
                    break;
            }
        }

        return path.IsEmpty;
    }

    private static bool MatchBrace(ReadOnlySpan<char> pattern, ReadOnlySpan<char> path, StringComparison comparison)
    {
        int close = pattern.IndexOf('}');
        if (close < 0)
        {
            // No closing brace -> treat '{' as a literal
            if (path.IsEmpty || !CharEqual(path[0], '{', comparison)) return false;
            return GlobMatch(pattern[1..], path[1..], comparison);
        }

        ReadOnlySpan<char> inner = pattern[1..close];
        ReadOnlySpan<char> rest = pattern[(close + 1)..];

        while (true)
        {
            int comma = inner.IndexOf(',');
            ReadOnlySpan<char> part = comma < 0 ? inner : inner[..comma];

            if (path.Length >= part.Length && StartsWith(path, part, comparison) && GlobMatch(rest, path[part.Length..], comparison))
                return true;

            if (comma < 0) break;
            inner = inner[(comma + 1)..];
        }

        return false;
    }

    private static bool MatchCharClass(ref ReadOnlySpan<char> pattern, char ch, StringComparison comparison)
    {
        // pattern[0] == '['
        int i = 1;
        bool negate = false;
        if (i < pattern.Length && pattern[i] == '!')
        {
            negate = true;
            i++;
        }

        bool matched = false;
        while (i < pattern.Length && pattern[i] != ']')
        {
            if (i + 2 < pattern.Length && pattern[i + 1] == '-' && pattern[i + 2] != ']')
            {
                if (CharInRange(ch, pattern[i], pattern[i + 2], comparison)) matched = true;
                i += 3;
            }
            else
            {
                if (CharEqual(ch, pattern[i], comparison)) matched = true;
                i++;
            }
        }

        if (i < pattern.Length) i++; // skip closing ']'
        pattern = pattern[i..];

        return matched ^ negate;
    }

    private static bool StartsWith(ReadOnlySpan<char> value, ReadOnlySpan<char> prefix, StringComparison comparison)
    {
        for (int i = 0; i < prefix.Length; i++)
        {
            if (!CharEqual(value[i], prefix[i], comparison)) return false;
        }

        return true;
    }

    private static bool CharEqual(char a, char b, StringComparison comparison) => comparison switch
    {
        StringComparison.Ordinal => a == b,
        _ => a == b || char.ToUpperInvariant(a) == char.ToUpperInvariant(b),
    };

    private static bool CharInRange(char ch, char lo, char hi, StringComparison comparison)
    {
        if (ch >= lo && ch <= hi) return true;
        if (comparison == StringComparison.Ordinal) return false;

        char upper = char.ToUpperInvariant(ch);
        char lower = char.ToLowerInvariant(ch);
        return (upper >= lo && upper <= hi) || (lower >= lo && lower <= hi);
    }

    private static string NormalizePath(string path)
    {
        path.NotEmpty();
        ReadOnlySpan<char> src = path;

        // Fast path: no backslashes and no repeated slashes -> return as-is
        bool needsNormalize = false;
        char prev = '\0';
        foreach (char ch in src)
        {
            char n = ch == '\\' ? '/' : ch;
            if (ch == '\\' || (n == '/' && prev == '/'))
            {
                needsNormalize = true;
                break;
            }

            prev = n;
        }

        if (!needsNormalize) return path;

        var sb = new StringBuilder(path.Length);
        prev = '\0';
        foreach (char ch in src)
        {
            char n = ch == '\\' ? '/' : ch;
            if (n == '/' && prev == '/') continue;
            sb.Append(n);
            prev = n;
        }

        return sb.ToString();
    }
}

