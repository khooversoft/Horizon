using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class StorePathTool
{
    /// <summary>
    /// Test if path is valid
    /// 
    /// 1) Path must not be null or empty
    /// 2) Path must not start with a slash
    /// 3) Path must not end with a slash
    /// 4) Path must only container valid characters, a-z, A-Z, 0-9, -, _, /, @, and .
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsPathValid(string path, bool wildCard = false)
    {
        if (string.IsNullOrEmpty(path)) return false;

        ReadOnlySpan<char> span = path.AsSpan();

        if (span[0] == '/' || span[^1] == '/') return false;

        for (int i = 0; i < span.Length; i++)
        {
            char c = span[i];

            bool isValid = c switch
            {
                >= 'a' and <= 'z' => true,
                >= 'A' and <= 'Z' => true,
                >= '0' and <= '9' => true,
                '-' or '_' or '/' or '@' or '.' => true,
                '*' when wildCard => true,
                _ => false,
            };

            if (!isValid) return false;
        }

        return true;
    }

    public static string ToFolderSearch(string? path, bool recursive = false)
    {
        if (path.IsEmpty()) return recursive ? "**" : "*";

        string newPath = recursive switch
        {
            false => GetRootPath(path) + "/*",
            true => GetRootPath(path) + "/**",
        };

        if (newPath.Length > 1 && newPath.StartsWith('/')) newPath = newPath[1..];
        return newPath;
    }

    /// <summary>
    /// If no wildcard is present, appends a recursive wildcard to the path.
    /// If a wildcard pattern is already present, returns the path unchanged.
    /// If the path ends with a redundant recursive file pattern, normalizes it to the recursive folder pattern.
    /// 
    /// "file" => "file/**"
    /// "file/**" => "file/**"
    /// "file.json" => "file.json/**"
    /// "file.*" => "file.*"
    /// "file/*" => "file/*"
    /// "file/*/*.json" => "file/*/*.json"
    /// "file/**/*.json" => "file/**/*.json"
    /// "file/**/*.*" => "file/**"
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string AddRecursiveSafe(string? path)
    {
        if (path.IsEmpty()) return "**";

        return path switch
        {
            "**" => path,
            string value when value.EndsWith("/**/*.*") => value[..^4],
            string value when value.EndsWith("/**/*") => value[..^2],
            string value when value.Contains('*') => value,
            _ => path + "/**",
        };
    }

    /// <summary>
    /// Gets the normalized root path for a store path and appends any additional path segments.
    /// </summary>
    /// <param name="path">
    /// The source path. Wildcard suffixes such as <c>/*</c> and <c>/**</c> are removed before the root path is built.
    /// </param>
    /// <param name="additionalPaths">
    /// Optional additional path values to append. Each value can contain one or more <c>/</c>-delimited segments.
    /// </param>
    /// <returns>
    /// A lower-case path composed from the root portion of <paramref name="path"/> and the appended path segments.
    /// </returns>
    public static string GetRootPath(string path, params string[] additionalPaths)
    {
        path.NotEmpty();
        int idx = path.IndexOf('*');

        var rootPath = idx switch
        {
            -1 => path,
            int v => path[..v].Func(x =>
            {
                int lastSlashIdx = x.LastIndexOf('/');
                return lastSlashIdx switch
                {
                    -1 => string.Empty,
                    var v when v == x.Length - 1 => x[..^1],
                    _ => x[..lastSlashIdx],
                };
            })
        };

        var addParts = additionalPaths.SelectMany(x => x.Split('/', StringSplitOptions.RemoveEmptyEntries));

        var fullPath = rootPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Concat(addParts)
            .Join('/');

        return fullPath.ToLowerInvariant();
    }

    /// <summary>
    /// Gets the file name from a store path, preserving the file extension.
    /// </summary>
    /// <param name="path">The full store path.</param>
    /// <returns>The last path segment, including its extension when present.</returns>
    public static string GetFileName(string path)
    {
        path.NotEmpty();

        var span = path.AsSpan().TrimEnd('/');

        if (span.IsEmpty) return string.Empty;

        int lastSlash = span.LastIndexOf('/');
        return lastSlash switch
        {
            -1 => span.ToString(),
            _ => span[(lastSlash + 1)..].ToString(),
        };
    }

    public static string GetFileExtension(string path)
    {
        path.NotEmpty();

        var span = path.AsSpan().TrimEnd('/');
        if (span.IsEmpty) return string.Empty;

        int lastSlash = span.LastIndexOf('/');
        int lastDot = span.LastIndexOf('.');

        return lastDot switch
        {
            -1 => string.Empty,
            _ when lastDot < lastSlash => string.Empty,
            _ => span[lastDot..].ToString(),
        };
    }

    public static string ToSafePath(string path, bool wildCard = false)
    {
        path.NotEmpty();
        if (path.Contains("//", StringComparison.Ordinal)) throw new ArgumentException("Path cannot contain consecutive '/' characters.");

        Span<char> safeChars = stackalloc char[path.Length];
        ReadOnlySpan<char> readPath = path.AsSpan();
        int toIndex = 0;
        int fromIndex = readPath is ['/', ..] ? 1 : 0;

        // Convert remaining characters to safe chars
        while (fromIndex < readPath.Length)
        {
            char c = readPath[fromIndex];
            safeChars[toIndex] = c switch
            {
                >= 'a' and <= 'z' => c,
                >= 'A' and <= 'Z' => char.ToLowerInvariant(c),
                >= '0' and <= '9' => c,
                '-' or '_' or '/' or '@' or '.' => c,
                '*' when wildCard => c,
                _ => '_'
            };
            toIndex++;
            fromIndex++;
        }

        if (toIndex > 0 && safeChars[toIndex - 1] == '/') toIndex--;

        return new string(safeChars[..toIndex]);
    }
}