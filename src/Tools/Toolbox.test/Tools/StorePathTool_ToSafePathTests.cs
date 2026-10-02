using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class StorePathTool_ToSafePathTests
{
    [Fact]
    public void ToSafePath_PathWithLeadingSlash_RemovesOneLeadingSlash()
    {
        var result = StorePathTool.ToSafePath("/folder/file.json");
        result.Be("folder/file.json");
    }

    [Fact]
    public void ToSafePath_PathWithoutLeadingSlash_ReturnsUnchanged()
    {
        var result = StorePathTool.ToSafePath("folder/file.json");
        result.Be("folder/file.json");
    }

    [Fact]
    public void ToSafePath_SingleSlash_ReturnsEmpty()
    {
        var result = StorePathTool.ToSafePath("/");
        result.Be("");
    }

    [Fact]
    public void ToSafePath_PathWithTrailingSlash_RemovesOneTrailingSlash()
    {
        var result = StorePathTool.ToSafePath("folder/file.json/");
        result.Be("folder/file.json");
    }

    [Fact]
    public void ToSafePath_PathWithLeadingAndTrailingSlash_RemovesOneSlashFromEachSide()
    {
        var result = StorePathTool.ToSafePath("/folder/file.json/");
        result.Be("folder/file.json");
    }

    [Fact]
    public void ToSafePath_PathWithDoubleSlashInMiddle_ThrowsArgumentException()
    {
        Verify.Throws<ArgumentException>(() => StorePathTool.ToSafePath("folder//file.json"));
    }

    [Fact]
    public void ToSafePath_PathWithDoubleLeadingSlash_ThrowsArgumentException()
    {
        Verify.Throws<ArgumentException>(() => StorePathTool.ToSafePath("//folder/file.json"));
    }

    [Fact]
    public void ToSafePath_PathWithDoubleTrailingSlash_ThrowsArgumentException()
    {
        Verify.Throws<ArgumentException>(() => StorePathTool.ToSafePath("folder/file.json//"));
    }
}
