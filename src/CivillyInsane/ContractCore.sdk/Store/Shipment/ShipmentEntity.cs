using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Security;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public class ShipmentEntity
{
    private readonly ISqlClient _client;
    private readonly ILogger<ShipmentEntity> _logger;
    private readonly IAuthAccess? _authAccess;
    private readonly IStoreNotify? _storeNotify;

    public ShipmentEntity(ISqlClient client, ILogger<ShipmentEntity> logger, IAuthAccess? authAccess = null, IStoreNotify? storeNotify = null)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();

        _authAccess = authAccess;
        _storeNotify = storeNotify;
    }

    public async Task<Option<ShipmentRecord>> Save(ShipmentRecord record)
    {
        record.NotNull().Validate().ThrowOnError();

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[shipment-save]", CommandType.StoredProcedure)
                .AddParameter("@shipment_id", record.ShipmentId)
                .AddParameter("@order_id", record.OrderId)
                .AddParameter("@carrier", record.Carrier)
                .AddParameter("@service_level", record.ServiceLevel)
                .AddParameter("@tracking_number", record.TrackingNumber)
                .AddParameter("@status", record.Status)
                .AddParameter("@JsonData", record.JsonData)
                .Execute(ReadShipment);

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Saved shipment for order {record.OrderId}", $"Failed to save shipment for order {record.OrderId}");
        return result;
    }

    public async Task<Option<ShipmentRecord>> AddItem(long shipmentId, long orderItemId, int quantity, string? jsonData = null)
    {
        quantity.Assert(x => x > 0, "quantity must be > 0");

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[shipment-item-add]", CommandType.StoredProcedure)
                .AddParameter("@shipment_id", shipmentId)
                .AddParameter("@order_item_id", orderItemId)
                .AddParameter("@quantity", quantity)
                .AddParameter("@JsonData", jsonData)
                .Execute(ReadShipment);

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Added item to shipment {shipmentId}", $"Failed to add item to shipment {shipmentId}");
        return result;
    }

    public async Task<Option<TrackingEventRecord>> AddTrackingEvent(TrackingEventRecord record)
    {
        record.NotNull().Validate().ThrowOnError();

        var result = await SqlStoreTool.Run(async () =>
        {
            var list = await _client.Query()
                .SetCommand("[App].[tracking-event-add]", CommandType.StoredProcedure)
                .AddParameter("@shipment_id", record.ShipmentId)
                .AddParameter("@status_code", record.StatusCode)
                .AddParameter("@description", record.Description)
                .AddParameter("@location", record.Location)
                .AddParameter("@event_utc", record.EventUtc)
                .AddParameter("@JsonData", record.JsonData)
                .Execute<TrackingEventRecord>();

            return list.ToSingle();
        });

        _storeNotify?.Notify(result, $"Added tracking event to shipment {record.ShipmentId}", $"Failed to add tracking event to shipment {record.ShipmentId}");
        return result;
    }

    public Task<Option<ShipmentRecord>> Get(long shipmentId) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[shipment-get]", CommandType.StoredProcedure)
            .AddParameter("@shipment_id", shipmentId)
            .Execute(ReadShipment);

        return list.ToSingle();
    });

    public Task<Option<IReadOnlyList<ShipmentRecord>>> List(long orderId) => SqlStoreTool.Run(async () =>
    {
        var list = await _client.Query()
            .SetCommand("[App].[shipment-list]", CommandType.StoredProcedure)
            .AddParameter("@order_id", orderId)
            .Execute<ShipmentRecord>();

        return list.ToListOption();
    });

    // [App].[shipment-get] returns the shipment header, its items, then its tracking events
    private static IReadOnlyList<ShipmentRecord> ReadShipment(SqlDataReader reader)
    {
        var shipments = reader.MapToList<ShipmentRecord>();
        if (shipments.Count != 1) return shipments;

        IReadOnlyList<ShipmentItemRecord> items = reader.NextResult() ? reader.MapToList<ShipmentItemRecord>() : [];
        IReadOnlyList<TrackingEventRecord> events = reader.NextResult() ? reader.MapToList<TrackingEventRecord>() : [];

        return [shipments[0] with { Items = items, TrackingEvents = events }];
    }
}
