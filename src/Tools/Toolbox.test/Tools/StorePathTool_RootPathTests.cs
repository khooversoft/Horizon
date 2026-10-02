using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class StorePathTool_RootPathTests
{

    [Fact]
    public void GetRootPath_PathWithoutWildcard_ReturnsLowercasePath()
    {
        var result = StorePathTool.GetRootPath("Folder/SubFolder");
        result.Be("folder/subfolder");
    }

    [Fact]
    public void GetRootPath_PathWithWildcardAtEnd_ReturnsFolderPath()
    {
        var result = StorePathTool.GetRootPath("folder/subfolder/*.json");
        result.Be("folder/subfolder");
    }

    [Fact]
    public void GetRootPath_PathWithDoubleStarAtEnd_ReturnsFolderPath()
    {
        var result = StorePathTool.GetRootPath("folder/subfolder/**");
        result.Be("folder/subfolder");
    }

    [Fact]
    public void GetRootPath_PathWithWildcardInMiddle_ReturnsPathBeforeWildcard()
    {
        var result = StorePathTool.GetRootPath("folder/*/file.json");
        result.Be("folder");
    }

    [Fact]
    public void GetRootPath_OnlyWildcard_ReturnsEmpty()
    {
        var result = StorePathTool.GetRootPath("**");
        result.Be("");
    }

    [Fact]
    public void GetRootPath_WildcardAtStart_ReturnsEmpty()
    {
        var result = StorePathTool.GetRootPath("*.json");
        result.Be("");
    }

    [Fact]
    public void GetRootPath_WithAdditionalPaths_CombinesPaths()
    {
        var result = StorePathTool.GetRootPath("folder", "sub1", "sub2");
        result.Be("folder/sub1/sub2");
    }

    [Fact]
    public void GetRootPath_WithAdditionalPathsContainingSlashes_SplitsAndCombines()
    {
        var result = StorePathTool.GetRootPath("folder", "sub1/sub2", "sub3");
        result.Be("folder/sub1/sub2/sub3");
    }

    [Fact]
    public void GetRootPath_PathWithTrailingSlashBeforeWildcard_HandlesCorrectly()
    {
        var result = StorePathTool.GetRootPath("folder/subfolder/*");
        result.Be("folder/subfolder");
    }

}
