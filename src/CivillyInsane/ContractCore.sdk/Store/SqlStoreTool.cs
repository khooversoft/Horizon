using Microsoft.Data.SqlClient;
using Toolbox.Types;

namespace ContractCore.sdk;

/// <summary>
/// Maps database API errors (THROW 51000-51002) to Option status codes.
/// </summary>
internal static class SqlStoreTool
{
    private const int _invalidArgument = 51000;
    private const int _notFound = 51001;
    private const int _conflict = 51002;

    public static async Task<Option<T>> Run<T>(Func<Task<Option<T>>> func)
    {
        try
        {
            return await func();
        }
        catch (SqlException ex) when (ex.Number is _invalidArgument or _notFound or _conflict)
        {
            StatusCode statusCode = ex.Number switch
            {
                _invalidArgument => StatusCode.BadRequest,
                _notFound => StatusCode.NotFound,
                _ => StatusCode.Conflict,
            };

            return (statusCode, ex.Message);
        }
    }

    public static Option<T> ToSingle<T>(this IReadOnlyList<T> result) => result switch
    {
        { Count: 0 } => StatusCode.NotFound,
        { Count: 1 } => result[0],
        _ => StatusCode.Conflict,
    };

    public static Option<IReadOnlyList<T>> ToListOption<T>(this IReadOnlyList<T> result) => new Option<IReadOnlyList<T>>(result);
}
