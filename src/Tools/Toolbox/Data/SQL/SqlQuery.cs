using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class SqlQuery
{
    private static readonly Random _random = new Random();
    private readonly string _connectionString;
    private readonly ILogger _logger;
    private const int _retryCount = 5;
    private const int _deadLockNumber = 1205;
    private const int _errorNumber = 50000;
    private const string _deadLockMessage = "Deadlock retry failed";

    public SqlQuery(string connectionString, ILogger logger)
    {
        _connectionString = connectionString.NotEmpty();
        _logger = logger.NotNull();
    }

    public string? Command { get; set; }
    public CommandType CommandType { get; set; } = CommandType.StoredProcedure;
    public IList<ISqlParameter> Parameters { get; } = new List<ISqlParameter>();

    public SqlQuery SetCommand(string command, CommandType commandType)
    {
        Command = command.NotEmpty();
        CommandType = commandType;
        return this;
    }

    public SqlQuery AddParameter<T>(string name, T? value)
    {
        name.NotEmpty();

        if (typeof(T).IsEnum)
        {
            Parameters.Add(new SqlSimpleParameter(name, value.NotNull().ToString().NotEmpty()));
            return this;
        }

        Parameters.Add(new SqlSimpleParameter(name, value!));
        return this;
    }

    /// <summary>
    /// Add parameter
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public SqlQuery AddParameter(ISqlParameter parameter)
    {
        parameter.NotNull();
        Parameters.Add(parameter);
        return this;
    }

    public async Task<Option<int>> ExecuteNonQuery()
    {
        var result = await Execute(async () =>
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();

            cmd.CommandText = Command;
            cmd.CommandType = CommandType;
            cmd.Parameters.AddRange(Parameters.Select(x => x.ToSqlParameter()).ToArray());
            conn.Open();

            var result = await cmd.ExecuteNonQueryAsync();
            _logger.LogDebug("ExecuteNonQuery, command={command}, result={result}", Command, result);
            return result;
        });

        return result switch
        {
            int v => v,
            StoredProcedureError err => (StatusCode.BadRequest, err.Error),
            _ => throw new InvalidOperationException("Failed to return value"),
        };
    }

    public async Task<IReadOnlyList<T>> Execute<T>(Func<SqlDataReader, IReadOnlyList<T>>? factory = null) where T : class, new()
    {
        var result = await Execute(async () =>
        {
            var list = new List<T>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();

            cmd.CommandText = Command;
            cmd.CommandType = CommandType;
            cmd.Parameters.AddRange(Parameters.Select(x => x.ToSqlParameter()).ToArray());
            conn.Open();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            var result = factory switch
            {
                null => reader.MapToList<T>(),
                _ => factory(reader),
            };

            _logger.LogDebug("Execute, command={command}, return count={count}", Command, result.Count);
            return result;
        });

        return result switch
        {
            IReadOnlyList<T> v => v,
            _ => throw new InvalidOperationException("Failed to return value"),
        };
    }

    private async Task<object> Execute(Func<Task<object>> sqlCmd)
    {
        SqlException saveEx = null!;

        for (int retry = 0; retry < _retryCount; retry++)
        {
            try
            {
                return await sqlCmd();
            }
            catch (SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case _deadLockNumber:
                        saveEx = sqlEx;
                        _logger.LogError(sqlEx, "SQL error - Victim of Deadlock: {Command}, Parameters: {Parameters}", Command, Parameters);
                        Thread.Sleep(TimeSpan.FromMilliseconds(_random.Next(10, 1000)));
                        continue;

                    case _errorNumber:
                        _logger.LogWarning(sqlEx, "SQL error - Command: {Command}, Parameters: {Parameters}", Command, Parameters);
                        return new StoredProcedureError(Command, sqlEx.Message);
                }

                _logger.LogError(sqlEx, "SQL execution error - Command: {Command}, Parameters: {Parameters}", Command, Parameters);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during SQL execution - Command: {Command}, Parameters: {Parameters}", Command, Parameters);
                throw new InvalidOperationException($"Unexpected error during SQL execution - Command: {Command}, Parameters: {Parameters}", ex);
            }
        }

        throw new InvalidOperationException(_deadLockMessage, saveEx);
    }

    private record StoredProcedureError(string? Command, string Error);
}
