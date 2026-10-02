using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class StorePathTool_SafePathTests
{
    [Theory]
    // Allowed characters pass through
    [InlineData("folder/file", "folder/file")]
    [InlineData("folder1/file2", "folder1/file2")]
    [InlineData("folder-a/file_b.txt", "folder-a/file_b.txt")]
    [InlineData("user@domain/file", "user@domain/file")]
    [InlineData("folder/file.txt", "folder/file.txt")]

    // Uppercase letters are lowercased
    [InlineData("Folder/File", "folder/file")]
    [InlineData("FOLDER/FILE", "folder/file")]

    // Unsafe characters are replaced with _
    [InlineData("my folder/my file", "my_folder/my_file")]
    [InlineData("folder!/file#txt", "folder_/file_txt")]
    [InlineData("!!!", "___")]
    [InlineData("My Folder/my file 2024.txt", "my_folder/my_file_2024.txt")]
    public void ToSafePath_NoExtension(string path, string expected)
    {
        BuildSafePath.Build(path).Be(expected);
    }

    [Theory]
    // Extension normalization
    [InlineData("folder/file", ".json", "folder/file.json")]
    [InlineData("folder/file", "json", "folder/file.json")]
    [InlineData("folder/file", "JSON", "folder/file.json")]
    [InlineData("folder/file", ".JSON", "folder/file.json")]

    // Existing extension is stripped and replaced
    [InlineData("folder/file.txt", ".json", "folder/file.json")]
    [InlineData("folder/file.txt", "json", "folder/file.json")]
    [InlineData("folder/file.old.txt", ".json", "folder/file.old.json")]
    [InlineData("folder/document", "md", "folder/document.md")]
    [InlineData("folder.dir/file", ".json", "folder.dir/file.json")]

    // Combined: path normalization + extension replacement
    [InlineData("Folder/File.TXT", ".json", "folder/file.json")]
    [InlineData("MY FOLDER/FILE.TXT", ".json", "my_folder/file.json")]
    [InlineData("My Folder/My File.csv", "json", "my_folder/my_file.json")]
    public void ToSafePath_WithExtension(string path, string extension, string expected)
    {
        BuildSafePath.Build(path, null, extension).Be(expected);
    }
}
