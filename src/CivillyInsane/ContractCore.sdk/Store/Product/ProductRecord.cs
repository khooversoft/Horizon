using System.ComponentModel.DataAnnotations.Schema;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public record ProductRecord
{
    [Column("product_id")] public long? ProductId { get; set; }
    [Column("slug")] public string Slug { get; set; } = null!;
    [Column("name")] public string Name { get; set; } = null!;
    [Column("description")] public string? Description { get; set; }
    [Column("base_price_cents")] public int BasePriceCents { get; set; }
    [Column("currency_code")] public string CurrencyCode { get; set; } = "USD";
    [Column("status")] public string Status { get; set; } = ProductStatus.Draft;
    [Column("created_utc")] public DateTime CreatedUtc { get; set; }
    [Column("modified_utc")] public DateTime ModifiedUtc { get; set; }
    [Column("JsonData")] public string? JsonData { get; set; }

    public static IValidator<ProductRecord> Validator { get; } = new Validator<ProductRecord>()
        .RuleFor(x => x.Slug).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.BasePriceCents).Must(x => x >= 0, _ => "must be >= 0")
        .RuleFor(x => x.CurrencyCode).NotEmpty()
        .RuleFor(x => x.Status).Must(x => ProductStatus.All.Contains(x), x => $"invalid status '{x}'")
        .Build();
}

public static class ProductStatus
{
    public const string Draft = "draft";
    public const string Active = "active";
    public const string Archived = "archived";
    public static IReadOnlyList<string> All { get; } = [Draft, Active, Archived];
}

public static class ProductRecordExtensions
{
    public static Option Validate(this ProductRecord record) => ProductRecord.Validator.Validate(record).ToOptionStatus();
}
