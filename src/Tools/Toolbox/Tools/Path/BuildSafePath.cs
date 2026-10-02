using System.Buffers;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class BuildSafePath
{
    /// <summary>
    /// Parse path and return components, path, filename, appType, and extension.
    /// Only the file name is required
    /// 
    /// File format is different from app type
    /// ".json" is the file format, and "registry" is the app type. The app type is used to identify the application that created the file.
    /// 
    /// Examples...
    /// [{paths/]{fileName}.[{appType}].{extension}
    /// 
    /// standardProcessor                       path = null, fileName = standardProcessor, appType = null, extension = null
    /// standardProcessor.registry.json         path = null, fileName = standardProcessor, appType = registry, extension = json
    /// user/user@domain.com.profile.json       path = user, fileName = user@domian.com, appType = profile, extension = json
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>

    public static (string? path, string fileName, string? appType, string? extension) Parse(string path)
    {
        path.NotEmpty();

        ReadOnlySpan<char> span = path.AsSpan().Trim('/');
        if (span.IsEmpty) throw new ArgumentException("File name is required.", nameof(path));

        int lastSlash = span.LastIndexOf('/');

        string? rootPath = lastSlash switch
        {
            -1 => null,
            0 => null,
            _ => span[..lastSlash].ToString(),
        };

        ReadOnlySpan<char> fileSpan = lastSlash switch
        {
            -1 => span,
            _ => span[(lastSlash + 1)..],
        };

        if (fileSpan.IsEmpty) throw new ArgumentException("File name is required.", nameof(path));

        int lastDot = fileSpan.LastIndexOf('.');
        if (lastDot <= 0 || lastDot == fileSpan.Length - 1)
        {
            return (rootPath, fileSpan.ToString(), null, null);
        }

        int previousDot = fileSpan[..lastDot].LastIndexOf('.');
        int atIndex = fileSpan.LastIndexOf('@');

        if (atIndex >= 0 && previousDot <= atIndex)
        {
            return (rootPath, fileSpan.ToString(), null, null);
        }

        string extension = fileSpan[(lastDot + 1)..].ToString();

        if (previousDot <= 0)
        {
            return (rootPath, fileSpan[..lastDot].ToString(), null, extension);
        }

        return (
            rootPath,
            fileSpan[..previousDot].ToString(),
            fileSpan[(previousDot + 1)..lastDot].ToString(),
            extension
        );
    }

    public static string Build(string path, string? appType = null, string? extension = null) => Build([path], appType, extension);

    public static string Build(string[] paths, string? appType = null, string? extension = null)
    {
        if (paths is null || paths.Length == 0) throw new ArgumentException("At least one path is required.", nameof(paths));

        var line = paths
            .SelectMany(x => x.NotEmpty().Split('/', StringSplitOptions.RemoveEmptyEntries))
            .Join('/');

        var (path, fileName, currentAppType, currentExtension) = Parse(line);

        string? finalAppType = appType.IsNotEmpty() ? appType : currentAppType;
        string? finalExtension = extension.IsNotEmpty() ? extension : currentExtension;

        if (finalAppType.IsNotEmpty() && finalExtension.IsEmpty()) throw new ArgumentException("If appType is specified, extension must also be specified.", nameof(extension));

        int maxLength = line.Length + (finalAppType?.Length ?? 0) + (finalExtension?.Length ?? 0) + 2;

        char[]? rented = null;
        Span<char> buffer = maxLength <= 512 ? stackalloc char[maxLength] : rented = ArrayPool<char>.Shared.Rent(maxLength);

        try
        {
            int index = 0;

            if (path.IsNotEmpty())
            {
                index = AppendSafePath(path.AsSpan(), buffer, index);
                buffer[index++] = '/';
            }

            index = AppendSafePath(fileName.AsSpan(), buffer, index);

            if (finalAppType.IsNotEmpty())
            {
                buffer[index++] = '.';
                index = AppendSafeSuffix(finalAppType!.AsSpan(), buffer, index);
            }

            if (finalExtension.IsNotEmpty())
            {
                buffer[index++] = '.';
                index = AppendSafeSuffix(finalExtension!.AsSpan(), buffer, index);
            }

            return new string(buffer[..index]);
        }
        finally
        {
            if (rented is not null) ArrayPool<char>.Shared.Return(rented);
        }
    }

    private static int AppendSafePath(ReadOnlySpan<char> source, Span<char> destination, int index)
    {
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            destination[index++] = c switch
            {
                >= 'a' and <= 'z' => c,
                >= 'A' and <= 'Z' => (char)(c + ('a' - 'A')),
                >= '0' and <= '9' => c,
                '-' or '_' or '/' or '@' or '.' => c,
                _ => '_'
            };
        }

        return index;
    }

    private static int AppendSafeSuffix(ReadOnlySpan<char> source, Span<char> destination, int index)
    {
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (c == '.') continue;

            destination[index++] = c switch
            {
                >= 'a' and <= 'z' => c,
                >= 'A' and <= 'Z' => (char)(c + ('a' - 'A')),
                >= '0' and <= '9' => c,
                '-' or '_' or '@' => c,
                _ => '_'
            };
        }

        return index;
    }
}
