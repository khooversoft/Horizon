using System.ComponentModel.DataAnnotations.Schema;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public record VariantRecord
{
    [Column("variant_id")] public long? VariantId { get; set; }
    [Column("product_id")] public long ProductId { get; set; }
    [Column("product_name")] public string? ProductName { get; set; }
    [Column("sku")] public string Sku { get; set; } = null!;
    [Column("size")] public string Size { get; set; } = null!;
    [Column("color")] public string Color { get; set; } = null!;
    [Column("price_cents")] public int? PriceCents { get; set; }
    [Column("effective_price_cents")] public int EffectivePriceCents { get; set; }
    [Column("currency_code")] public string? CurrencyCode { get; set; }
    [Column("stock_qty")] public int StockQty { get; set; }
    [Column("status")] public string Status { get; set; } = VariantStatus.Active;
    [Column("created_utc")] public DateTime CreatedUtc { get; set; }
    [Column("modified_utc")] public DateTime ModifiedUtc { get; set; }
    [Column("JsonData")] public string? JsonData { get; set; }

    public static IValidator<VariantRecord> Validator { get; } = new Validator<VariantRecord>()
        .RuleFor(x => x.ProductId).Must(x => x > 0, _ => "is required")
        .RuleFor(x => x.Sku).NotEmpty()
        .RuleFor(x => x.Size).NotEmpty()
        .RuleFor(x => x.Color).NotEmpty()
        .RuleFor(x => x.PriceCents).Must(x => x is null || x >= 0, _ => "must be >= 0")
        .RuleFor(x => x.StockQty).Must(x => x >= 0, _ => "must be >= 0")
        .RuleFor(x => x.Status).Must(x => VariantStatus.All.Contains(x), x => $"invalid status '{x}'")
        .Build();
}

public static class VariantStatus
{
    public const string Active = "active";
    public const string Inactive = "inactive";
    public static IReadOnlyList<string> All { get; } = [Active, Inactive];
}

public static class VariantRecordExtensions
{
    public static Option Validate(this VariantRecord record) => VariantRecord.Validator.Validate(record).ToOptionStatus();
}
