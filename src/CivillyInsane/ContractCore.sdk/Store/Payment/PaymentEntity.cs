using System.Data;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public class PaymentEntity
{
    private readonly ISqlClient _client;
    private readonly ILogger<PaymentEntity> _logger;
    private readonly IAuthAccess? _authAccess;
    private readonly IStoreNotify? _storeNotify;

    public PaymentEntity(ISqlClient client, ILogger<PaymentEntity> logger, IAuthAccess? authAccess = null, IStoreNotify? storeNotify = null)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        _authAccess = authAccess;
        _storeNotify = storeNotify;
    }

    public async Task<Option<PaymentRecord>> Save(PaymentRecord record)
    {
        record.NotNull().Validate().ThrowOnError();

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[payment-save]", CommandType.StoredProcedure)
                .AddParameter("@payment_id", record.PaymentId)
                .AddParameter("@order_id", record.OrderId)
                .AddParameter("@provider", record.Provider)
                .AddParameter("@provider_ref", record.ProviderRef)
                .AddParameter("@status", record.Status)
                .AddParameter("@amount_cents", record.AmountCents)
                .AddParameter("@currency_code", record.CurrencyCode)
                .AddParameter("@JsonData", record.JsonData)
                .Execute<PaymentRecord>();

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Saved payment for order {record.OrderId}", $"Failed to save payment for order {record.OrderId}");
        return result;
    }

    public Task<Option<PaymentRecord>> Get(long paymentId) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[payment-get]", CommandType.StoredProcedure)
            .AddParameter("@payment_id", paymentId)
            .Execute<PaymentRecord>();

        return list.ToSingle();
    });

    public Task<Option<IReadOnlyList<PaymentRecord>>> List(long orderId) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[payment-list]", CommandType.StoredProcedure)
            .AddParameter("@order_id", orderId)
            .Execute<PaymentRecord>();

        return list.ToListOption();
    });
}
