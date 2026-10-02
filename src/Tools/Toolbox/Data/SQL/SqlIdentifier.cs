using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

/// <summary>
/// Immutable structure to represent SQL object types such as table name
/// </summary>
public class SqlIdentifier
{
    private const string _beginToken = "[";
    private const string _endToken = "]";

    public SqlIdentifier(string schema, string name)
    {
        Schame = schema.NotEmpty();
        Name = name.NotEmpty();
    }

    public SqlIdentifier(string name)
    {
        name.NotEmpty();

        var parts = new Stack<string>(name.Split('.').Select(x => RemoveEnclosingString(x)));

        Name = parts.Pop();
        if (parts.Count > 0) Schame = parts.Pop();
    }

    /// <summary>
    /// Schema name
    /// </summary>
    public string? Schame { get; }

    /// <summary>
    /// Name of object
    /// </summary>
    public string Name { get; }

    public override string ToString()
    {
        var list = new List<string>();
        list.Add($"{_beginToken}{Schame}{_endToken}");
        list.Add($"{_beginToken}{Name}{_endToken}");

        return string.Join(".", list);
    }

    /// <summary>
    /// Easy translation to string
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator string(SqlIdentifier value) => value.ToString();

    /// <summary>
    /// Remove enclosing string
    /// </summary>
    /// <param name="value">string</param>
    /// <returns>string if not modified or start and end removed from the string</returns>
    private string RemoveEnclosingString(string value)
    {
        if (value.IsEmpty() == true)
        {
            return value;
        }

        if (value.Length <= 3 || value[0] != _beginToken[0] || value[value.Length - 1] != _endToken[1])
        {
            return value;
        }

        return value.Substring(_beginToken.Length, value.Length - _endToken.Length - 1);
    }
}
