using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Data;

public interface ISqlClient
{
    string ConnectionString { get; }
    SqlQuery Query();
    bool TestConnection();
}

public static class SqlClientTool
{
    public static T CreateSqlStore<T>(string connectionString, IServiceProvider serviceProvider)
    {
        var sqlOption = new SqlOption { ConnectionString = connectionString.NotEmpty() };
        ISqlClient<T> sqlClient = ActivatorUtilities.CreateInstance<SqlClient<T>>(serviceProvider, sqlOption);
        sqlClient.TestConnection().BeTrue($"Failed to connect to SQL database connectionString={connectionString}");

        var store = ActivatorUtilities.CreateInstance<T>(serviceProvider, sqlClient);
        return store;
    }
}

public interface ISqlClient<T> : ISqlClient { }

public class SqlClient<T> : SqlClient, ISqlClient<T>
{
    public SqlClient(SqlOption sqlOption, ILogger<SqlClient> logger)
        : base(sqlOption, logger)
    {
    }
}

public class SqlClient
{
    private readonly ILogger<SqlClient> _logger;

    public SqlClient(SqlOption sqlOption, ILogger<SqlClient> logger)
    {
        sqlOption.NotNull();
        _logger = logger.NotNull();

        ConnectionString = sqlOption.ConnectionString.NotEmpty();
    }

    public SqlClient(string connectionString, ILogger<SqlClient> logger)
    {
        ConnectionString = connectionString.NotNull();
        _logger = logger;
    }

    public string ConnectionString { get; }

    public SqlQuery Query() => new SqlQuery(ConnectionString, _logger);

    public bool TestConnection()
    {
        try
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "select 1";
            cmd.CommandType = CommandType.Text;

            conn.Open();
            cmd.ExecuteNonQuery();

            _logger.LogDebug("Database connect test passed - connection string: {ConnectionString}", ConnectionString);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to database - connection string: {ConnectionString}", ConnectionString);
            return false;
        }
    }
}
