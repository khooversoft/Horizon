using System.ComponentModel.DataAnnotations.Schema;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public record ShipmentRecord
{
    [Column("shipment_id")] public long? ShipmentId { get; set; }
    [Column("order_id")] public long OrderId { get; set; }
    [Column("carrier")] public string? Carrier { get; set; }
    [Column("service_level")] public string? ServiceLevel { get; set; }
    [Column("tracking_number")] public string? TrackingNumber { get; set; }
    [Column("status")] public string Status { get; set; } = ShipmentStatus.Pending;
    [Column("shipped_utc")] public DateTime? ShippedUtc { get; set; }
    [Column("delivered_utc")] public DateTime? DeliveredUtc { get; set; }
    [Column("created_utc")] public DateTime CreatedUtc { get; set; }
    [Column("modified_utc")] public DateTime ModifiedUtc { get; set; }
    [Column("JsonData")] public string? JsonData { get; set; }

    [NotMapped] public IReadOnlyList<ShipmentItemRecord> Items { get; set; } = [];
    [NotMapped] public IReadOnlyList<TrackingEventRecord> TrackingEvents { get; set; } = [];

    public static IValidator<ShipmentRecord> Validator { get; } = new Validator<ShipmentRecord>()
        .RuleFor(x => x.OrderId).Must(x => x > 0, _ => "is required")
        .RuleFor(x => x.Status).Must(x => ShipmentStatus.All.Contains(x), x => $"invalid status '{x}'")
        .Build();
}

public record ShipmentItemRecord
{
    [Column("shipment_item_id")] public long ShipmentItemId { get; set; }
    [Column("shipment_id")] public long ShipmentId { get; set; }
    [Column("order_item_id")] public long OrderItemId { get; set; }
    [Column("sku")] public string Sku { get; set; } = null!;
    [Column("product_name")] public string ProductName { get; set; } = null!;
    [Column("size")] public string Size { get; set; } = null!;
    [Column("color")] public string Color { get; set; } = null!;
    [Column("quantity")] public int Quantity { get; set; }
    [Column("created_utc")] public DateTime CreatedUtc { get; set; }
    [Column("JsonData")] public string? JsonData { get; set; }
}

public record TrackingEventRecord
{
    [Column("tracking_event_id")] public long? TrackingEventId { get; set; }
    [Column("shipment_id")] public long ShipmentId { get; set; }
    [Column("status_code")] public string StatusCode { get; set; } = null!;
    [Column("description")] public string? Description { get; set; }
    [Column("location")] public string? Location { get; set; }
    [Column("event_utc")] public DateTime? EventUtc { get; set; }
    [Column("created_utc")] public DateTime CreatedUtc { get; set; }
    [Column("JsonData")] public string? JsonData { get; set; }

    public static IValidator<TrackingEventRecord> Validator { get; } = new Validator<TrackingEventRecord>()
        .RuleFor(x => x.ShipmentId).Must(x => x > 0, _ => "is required")
        .RuleFor(x => x.StatusCode).NotEmpty()
        .Build();
}

public static class ShipmentStatus
{
    public const string Pending = "pending";
    public const string Shipped = "shipped";
    public const string InTransit = "in_transit";
    public const string Delivered = "delivered";
    public const string Returned = "returned";
    public const string Exception = "exception";
    public static IReadOnlyList<string> All { get; } = [Pending, Shipped, InTransit, Delivered, Returned, Exception];
}

public static class ShipmentRecordExtensions
{
    public static Option Validate(this ShipmentRecord record) => ShipmentRecord.Validator.Validate(record).ToOptionStatus();
    public static Option Validate(this TrackingEventRecord record) => TrackingEventRecord.Validator.Validate(record).ToOptionStatus();
}
