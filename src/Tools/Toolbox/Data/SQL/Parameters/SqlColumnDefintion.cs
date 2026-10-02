using System.Data;
using Microsoft.Data.SqlClient.Server;
using Toolbox.Tools;

namespace Toolbox.Data;

public class SqlColumnDefintion<T>
{
    public SqlColumnDefintion(string columnName, SqlDbType sqlDbType, Func<T, object> getValue, int? dataSize = null)
    {
        columnName.NotEmpty();
        getValue.NotNull();

        ColumnName = columnName;
        SqlDbType = sqlDbType;
        GetValue = getValue;
        DataSize = dataSize;
    }

    public string ColumnName { get; }

    public SqlDbType SqlDbType { get; }

    public int? DataSize { get; }

    public Func<T, object> GetValue { get; }

    /// <summary>
    /// Get Sql Metadata for column definition
    /// </summary>
    /// <returns></returns>
    public SqlMetaData GetSqlMetaData()
    {
        switch (this.SqlDbType)
        {
            case SqlDbType.NVarChar:
                return new SqlMetaData(this.ColumnName, this.SqlDbType, this.DataSize ?? SqlMetaData.Max);

            default:
                return new SqlMetaData(this.ColumnName, this.SqlDbType);
        }
    }
}
