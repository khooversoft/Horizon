using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class StorePathTool_SearchTests
{
    [Fact]
    public void ToFolderSearch_NonRecursive_AppendsSingleStar()
    {
        var result = StorePathTool.ToFolderSearch("folder/subfolder", recursive: false);
        result.Be("folder/subfolder/*");
    }

    [Fact]
    public void ToFolderSearch_Recursive_AppendsDoubleStar()
    {
        var result = StorePathTool.ToFolderSearch("folder/subfolder", recursive: true);
        result.Be("folder/subfolder/**");
    }

    [Fact]
    public void ToFolderSearch_PathWithWildcard_UsesRootPath()
    {
        var result = StorePathTool.ToFolderSearch("folder/*.json", recursive: false);
        result.Be("folder/*");
    }

    [Fact]
    public void ToFolderSearch_PathWithWildcard_Recursive_UsesRootPath()
    {
        var result = StorePathTool.ToFolderSearch("folder/**/*.json", recursive: true);
        result.Be("folder/**");
    }

    [Fact]
    public void AddRecursiveSafe_DoubleStar_ReturnsUnchanged()
    {
        var result = StorePathTool.AddRecursiveSafe("**");
        result.Be("**");
    }

    [Fact]
    public void AddRecursiveSafe_RootSingleStar_ReturnsUnchanged()
    {
        var result = StorePathTool.AddRecursiveSafe("/*");
        result.Be("/*");
    }

    [Fact]
    public void AddRecursiveSafe_PathEndsWithDoubleStar_ReturnsUnchanged()
    {
        var result = StorePathTool.AddRecursiveSafe("folder/**");
        result.Be("folder/**");
    }

    [Fact]
    public void AddRecursiveSafe_PathEndsWithSingleStar_ReturnsUnchanged()
    {
        var result = StorePathTool.AddRecursiveSafe("folder/*");
        result.Be("folder/*");
    }

    [Fact]
    public void AddRecursiveSafe_RegularPath_AppendsDoubleStar()
    {
        var result = StorePathTool.AddRecursiveSafe("folder/subfolder");
        result.Be("folder/subfolder/**");
    }

    [Fact]
    public void AddRecursiveSafe_SingleFolder_AppendsDoubleStar()
    {
        var result = StorePathTool.AddRecursiveSafe("folder");
        result.Be("folder/**");
    }

    [Fact]
    public void AddRecursiveSafe_FileWildcard_ReturnsUnchanged()
    {
        var result = StorePathTool.AddRecursiveSafe("file.*");
        result.Be("file.*");
    }

    [Fact]
    public void AddRecursiveSafe_NestedSingleStarPattern_ReturnsUnchanged()
    {
        var result = StorePathTool.AddRecursiveSafe("file/*/*.json");
        result.Be("file/*/*.json");
    }

    [Fact]
    public void AddRecursiveSafe_RecursiveFilePattern_ReturnsUnchanged()
    {
        var result = StorePathTool.AddRecursiveSafe("file/**/*.json");
        result.Be("file/**/*.json");
    }

    [Fact]
    public void AddRecursiveSafe_RecursiveAllFilesPattern_NormalizesToRecursiveFolder()
    {
        var result = StorePathTool.AddRecursiveSafe("file/**/*.*");
        result.Be("file/**");
    }
}
