using Microsoft.Data.SqlClient;

namespace Toolbox.Data;

/// <summary>
/// Interface for simple SQL parameters
/// </summary>
public interface ISqlParameter
{
    SqlParameter ToSqlParameter();
}
