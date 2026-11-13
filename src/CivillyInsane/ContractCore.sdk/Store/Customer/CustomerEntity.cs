using System.Data;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public class CustomerEntity
{
    private readonly ISqlClient _client;
    private readonly ILogger<CustomerEntity> _logger;
    private readonly IAuthAccess? _authAccess;
    private readonly IStoreNotify? _storeNotify;

    public CustomerEntity(ISqlClient client, ILogger<CustomerEntity> logger, IAuthAccess? authAccess = null, IStoreNotify? storeNotify = null)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        _authAccess = authAccess;
        _storeNotify = storeNotify;
    }

    public async Task<Option<CustomerRecord>> Save(CustomerRecord record)
    {
        record.NotNull().Validate().ThrowOnError();

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[customer-save]", CommandType.StoredProcedure)
                .AddParameter("@customer_id", record.CustomerId)
                .AddParameter("@email", record.Email)
                .AddParameter("@first_name", record.FirstName)
                .AddParameter("@last_name", record.LastName)
                .AddParameter("@phone", record.Phone)
                .AddParameter("@JsonData", record.JsonData)
                .Execute<CustomerRecord>();

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Saved customer {record.Email}", $"Failed to save customer {record.Email}");
        return result;
    }

    public Task<Option<CustomerRecord>> Get(long customerId) => Get(customerId, null);

    public Task<Option<CustomerRecord>> Get(string email) => Get(null, email.NotEmpty());

    private Task<Option<CustomerRecord>> Get(long? customerId, string? email) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[customer-get]", CommandType.StoredProcedure)
            .AddParameter("@customer_id", customerId)
            .AddParameter("@email", email)
            .Execute<CustomerRecord>();

        return list.ToSingle();
    });

    public Task<Option<IReadOnlyList<CustomerRecord>>> List(int offset = 0, int pageSize = 100) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[customer-list]", CommandType.StoredProcedure)
            .AddParameter("@Offset", offset)
            .AddParameter("@PageSize", pageSize)
            .Execute<CustomerRecord>();

        return list.ToListOption();
    });
}
