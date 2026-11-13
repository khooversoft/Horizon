using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public class OrderEntity
{
    private readonly ISqlClient _client;
    private readonly ILogger<OrderEntity> _logger;
    private readonly IAuthAccess? _authAccess;
    private readonly IStoreNotify? _storeNotify;

    public OrderEntity(ISqlClient client, ILogger<OrderEntity> logger, IAuthAccess? authAccess = null, IStoreNotify? storeNotify = null)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        _authAccess = authAccess;
        _storeNotify = storeNotify;
    }

    /// <summary>
    /// Create or update a pending order
    /// </summary>
    public async Task<Option<OrderRecord>> Save(OrderRecord record)
    {
        record.NotNull().Validate().ThrowOnError();

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[order-save]", CommandType.StoredProcedure)
                .AddParameter("@order_id", record.OrderId)
                .AddParameter("@customer_id", record.CustomerId)
                .AddParameter("@email", record.Email)
                .AddParameter("@currency_code", record.CurrencyCode)
                .AddParameter("@shipping_cents", record.ShippingCents)
                .AddParameter("@tax_cents", record.TaxCents)
                .AddParameter("@discount_cents", record.DiscountCents)
                .AddParameter("@ship_name", record.ShipName)
                .AddParameter("@ship_line1", record.ShipLine1)
                .AddParameter("@ship_line2", record.ShipLine2)
                .AddParameter("@ship_city", record.ShipCity)
                .AddParameter("@ship_region", record.ShipRegion)
                .AddParameter("@ship_postal_code", record.ShipPostalCode)
                .AddParameter("@ship_country_code", record.ShipCountryCode)
                .AddParameter("@JsonData", record.JsonData)
                .Execute(ReadOrder);

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Saved order for {record.Email}", $"Failed to save order for {record.Email}");
        return result;
    }

    public async Task<Option<OrderRecord>> AddItem(long orderId, long variantId, int quantity, string? jsonData = null)
    {
        quantity.Assert(x => x > 0, "quantity must be > 0");

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[order-item-add]", CommandType.StoredProcedure)
                .AddParameter("@order_id", orderId)
                .AddParameter("@variant_id", variantId)
                .AddParameter("@quantity", quantity)
                .AddParameter("@JsonData", jsonData)
                .Execute(ReadOrder);

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Added item to order {orderId}", $"Failed to add item to order {orderId}");
        return result;
    }

    public async Task<Option<OrderRecord>> SetStatus(long orderId, string status)
    {
        status.NotEmpty().Assert(x => OrderStatus.All.Contains(x), x => $"invalid status '{x}'");

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[order-status-set]", CommandType.StoredProcedure)
                .AddParameter("@order_id", orderId)
                .AddParameter("@status", status)
                .Execute(ReadOrder);

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Set order {orderId} status to {status}", $"Failed to set order {orderId} status to {status}");
        return result;
    }

    public Task<Option<OrderRecord>> Get(long orderId) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[order-get]", CommandType.StoredProcedure)
            .AddParameter("@order_id", orderId)
            .Execute(ReadOrder);

        return list.ToSingle();
    });

    public Task<Option<IReadOnlyList<OrderRecord>>> List(long? customerId = null, string? email = null, string? status = null, int offset = 0, int pageSize = 100) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[order-list]", CommandType.StoredProcedure)
            .AddParameter("@customer_id", customerId)
            .AddParameter("@email", email)
            .AddParameter("@status", status)
            .AddParameter("@Offset", offset)
            .AddParameter("@PageSize", pageSize)
            .Execute<OrderRecord>();

        return list.ToListOption();
    });

    // [App].[order-get] returns the order header followed by its items
    private static IReadOnlyList<OrderRecord> ReadOrder(SqlDataReader reader)
    {
        var orders = reader.MapToList<OrderRecord>();
        if (orders.Count != 1 || !reader.NextResult()) return orders;

        var items = reader.MapToList<OrderItemRecord>();
        return [orders[0] with { Items = items }];
    }
}
