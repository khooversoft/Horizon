using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Toolbox.Tools;

namespace Toolbox.Data;

/// <summary>
/// Handle simple SQL parameters (immutable)
/// </summary>
[DebuggerDisplay("Name={Name}, Value={Value}")]
public class SqlSimpleParameter : ISqlParameter
{
    public SqlSimpleParameter(string name, object? value)
    {
        name.NotEmpty();

        Name = name;
        Value = value;
    }

    /// <summary>
    /// Name of the parameter
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Value of the parameter — null is mapped to DBNull.Value when sent to SQL Server
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Convert to SQL Parameter
    /// </summary>
    /// <returns>SQL parameter</returns>
    public SqlParameter ToSqlParameter()
    {
        return new SqlParameter(Name, Value ?? DBNull.Value);
    }
}
