using System.ComponentModel.DataAnnotations.Schema;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public record OrderRecord
{
    [Column("order_id")] public long? OrderId { get; set; }
    [Column("customer_id")] public long? CustomerId { get; set; }
    [Column("email")] public string Email { get; set; } = null!;
    [Column("status")] public string Status { get; set; } = OrderStatus.Pending;
    [Column("currency_code")] public string CurrencyCode { get; set; } = "USD";
    [Column("subtotal_cents")] public int SubtotalCents { get; set; }
    [Column("shipping_cents")] public int ShippingCents { get; set; }
    [Column("tax_cents")] public int TaxCents { get; set; }
    [Column("discount_cents")] public int DiscountCents { get; set; }
    [Column("total_cents")] public int TotalCents { get; set; }
    [Column("ship_name")] public string? ShipName { get; set; }
    [Column("ship_line1")] public string? ShipLine1 { get; set; }
    [Column("ship_line2")] public string? ShipLine2 { get; set; }
    [Column("ship_city")] public string? ShipCity { get; set; }
    [Column("ship_region")] public string? ShipRegion { get; set; }
    [Column("ship_postal_code")] public string? ShipPostalCode { get; set; }
    [Column("ship_country_code")] public string? ShipCountryCode { get; set; }
    [Column("placed_utc")] public DateTime? PlacedUtc { get; set; }
    [Column("created_utc")] public DateTime CreatedUtc { get; set; }
    [Column("modified_utc")] public DateTime ModifiedUtc { get; set; }
    [Column("JsonData")] public string? JsonData { get; set; }

    [NotMapped] public IReadOnlyList<OrderItemRecord> Items { get; set; } = [];

    public static IValidator<OrderRecord> Validator { get; } = new Validator<OrderRecord>()
        .RuleFor(x => x.Email).ValidEmail()
        .RuleFor(x => x.CurrencyCode).NotEmpty()
        .RuleFor(x => x.ShippingCents).Must(x => x >= 0, _ => "must be >= 0")
        .RuleFor(x => x.TaxCents).Must(x => x >= 0, _ => "must be >= 0")
        .RuleFor(x => x.DiscountCents).Must(x => x >= 0, _ => "must be >= 0")
        .Build();
}

public record OrderItemRecord
{
    [Column("order_item_id")] public long OrderItemId { get; set; }
    [Column("order_id")] public long OrderId { get; set; }
    [Column("variant_id")] public long VariantId { get; set; }
    [Column("sku")] public string Sku { get; set; } = null!;
    [Column("product_name")] public string ProductName { get; set; } = null!;
    [Column("size")] public string Size { get; set; } = null!;
    [Column("color")] public string Color { get; set; } = null!;
    [Column("unit_price_cents")] public int UnitPriceCents { get; set; }
    [Column("quantity")] public int Quantity { get; set; }
    [Column("line_total_cents")] public int LineTotalCents { get; set; }
    [Column("shipped_qty")] public int ShippedQty { get; set; }
    [Column("created_utc")] public DateTime CreatedUtc { get; set; }
    [Column("JsonData")] public string? JsonData { get; set; }
}

public static class OrderStatus
{
    public const string Pending = "pending";
    public const string Paid = "paid";
    public const string PartiallyShipped = "partially_shipped";
    public const string Shipped = "shipped";
    public const string Delivered = "delivered";
    public const string Cancelled = "cancelled";
    public const string Refunded = "refunded";
    public static IReadOnlyList<string> All { get; } = [Pending, Paid, PartiallyShipped, Shipped, Delivered, Cancelled, Refunded];
}

public static class OrderRecordExtensions
{
    public static Option Validate(this OrderRecord record) => OrderRecord.Validator.Validate(record).ToOptionStatus();
}
