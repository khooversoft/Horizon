using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class BuildSafePathTests
{
    [Theory]
    [InlineData("file")]
    [InlineData("folder/file.json")]
    [InlineData("Folder/File.JSON")]
    [InlineData("user@domain.com.profile.json")]
    [InlineData("f1/f2/f3.app.json")]
    [InlineData("a-b_c.1/next")]
    public void IsPathValid_ShouldReturnTrue_ForValidPaths(string path)
    {
        StorePathTool.IsPathValid(path).BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("/file")]
    [InlineData("file/")]
    [InlineData("folder/my file.json")]
    [InlineData("folder\\file.json")]
    [InlineData("file#name.json")]
    public void IsPathValid_ShouldReturnFalse_ForInvalidPaths(string path)
    {
        StorePathTool.IsPathValid(path).BeFalse();
    }

    [Theory]
    [InlineData(new[] { "file" }, null, null, "file")]
    [InlineData(new[] { "file.json" }, null, null, "file.json")]
    [InlineData(new[] { "folder", "file.json" }, null, null, "folder/file.json")]
    [InlineData(new[] { "folder", "file.app.json" }, null, null, "folder/file.app.json")]
    [InlineData(new[] { "f1/f2", "f3.json" }, null, null, "f1/f2/f3.json")]
    [InlineData(new[] { "standardProcessor" }, "registry", "json", "standardprocessor.registry.json")]
    [InlineData(new[] { "user@domain.com" }, "profile", "json", "user@domain.com.profile.json")]
    [InlineData(new[] { "user", "user@domain.com" }, "profile", "json", "user/user@domain.com.profile.json")]
    [InlineData(new[] { "User", "My File" }, "Profile", ".JSON", "user/my_file.profile.json")]
    [InlineData(new[] { "folder/sub", "File Name" }, "app/type", "data.bin", "folder/sub/file_name.app_type.databin")]
    [InlineData(new[] { "folder/sub", "file/FileName" }, "app/type", "data.bin", "folder/sub/file/filename.app_type.databin")]
    [InlineData(new[] { "folder", "file.profile.json" }, "registry", "json", "folder/file.registry.json")]
    [InlineData(new[] { "folder", "file.json" }, "registry", "json", "folder/file.registry.json")]
    public void BuildValue(string[] paths, string? appType, string? extension, string expected)
    {
        BuildSafePath.Build(paths, appType, extension).Be(expected);
    }

    [Fact]
    public void Build_ShouldThrow_WhenAppTypeSpecifiedWithoutExtension()
    {
        Verify.Throws<ArgumentException>(() => BuildSafePath.Build(["file"], "registry", null));
    }
}
