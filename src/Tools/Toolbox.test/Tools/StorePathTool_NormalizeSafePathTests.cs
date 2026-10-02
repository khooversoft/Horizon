using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class StorePathTool_NormalizeSafePathTests
{
    [Theory]
    [InlineData(new[] { "standardProcessor" }, "registry", "json", "standardprocessor.registry.json")]
    [InlineData(new[] { "user", "user@domain.com" }, "profile", "json", "user/user@domain.com.profile.json")]
    [InlineData(new[] { "User", "My File" }, "Profile", ".JSON", "user/my_file.profile.json")]
    [InlineData(new[] { "folder/sub", "File Name" }, "app/type", "data.bin", "folder/sub/file_name.app_type.databin")]
    public void NormalizeSafePath_ValidInput_ReturnsExpected(string[] paths, string appType, string extension, string expected)
    {
        BuildSafePath.Build(paths, appType, extension).Be(expected);
    }

    [Fact]
    public void NormalizeSafePath_UnsafeCharacters_AreNormalized()
    {
        BuildSafePath.Build(["folder!", "file#name"], "reg!stry", "j$son")
            .Be("folder_/file_name.reg_stry.j_son");
    }

    [Fact]
    public void NormalizeSafePath_ExistingAppTypeAndExtension_AreReplaced()
    {
        BuildSafePath.Build(["folder", "file.profile.json"], "registry", "json")
            .Be("folder/file.registry.json");
    }

    [Fact]
    public void NormalizeSafePath_ExistingExtensionWithNewAppType_IsReplaced()
    {
        BuildSafePath.Build(["folder", "file.json"], "registry", "json")
            .Be("folder/file.registry.json");
    }
}
