using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class ParsePathTests
{
    [Theory]
    [InlineData("file", null, "file", null, null)]
    [InlineData("file.json", null, "file", null, "json")]
    [InlineData("f1/f2'f3", "f1", "f2'f3", null, null)]
    [InlineData("f1/f2/f3.json", "f1/f2", "f3", null, "json")]
    [InlineData("f1/f2/f3.app.json", "f1/f2", "f3", "app", "json")]
    [InlineData("folder/file.json", "folder", "file", null, "json")]
    [InlineData("standardProcessor.registry.json", null, "standardProcessor", "registry", "json")]
    [InlineData("user/user@domain.com.profile.json", "user", "user@domain.com", "profile", "json")]
    [InlineData("/folder/file.json/", "folder", "file", null, "json")]
    [InlineData("file.", null, "file.", null, null)]
    [InlineData("user@domain.com", null, "user@domain.com", null, null)]
    public void ParseValue(string fullPath, string? path, string fileName, string? appType, string? extension)
    {
        (string? path, string fileName, string? appType, string? extension) result = BuildSafePath.Parse(fullPath);

        result.path.Be(path);
        result.fileName.Be(fileName);
        result.appType.Be(appType);
        result.extension.Be(extension);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("///")]
    public void ParseValue_ShouldThrow_WhenFileNameIsMissing(string fullPath)
    {
        Verify.Throws<ArgumentException>(() => BuildSafePath.Parse(fullPath));
    }
}
