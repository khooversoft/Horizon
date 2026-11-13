using System.ComponentModel.DataAnnotations.Schema;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public record PaymentRecord
{
    [Column("payment_id")] public long? PaymentId { get; set; }
    [Column("order_id")] public long OrderId { get; set; }
    [Column("provider")] public string Provider { get; set; } = null!;
    [Column("provider_ref")] public string? ProviderRef { get; set; }
    [Column("status")] public string Status { get; set; } = PaymentStatus.Pending;
    [Column("amount_cents")] public int AmountCents { get; set; }
    [Column("currency_code")] public string CurrencyCode { get; set; } = "USD";
    [Column("created_utc")] public DateTime CreatedUtc { get; set; }
    [Column("modified_utc")] public DateTime ModifiedUtc { get; set; }
    [Column("JsonData")] public string? JsonData { get; set; }

    public static IValidator<PaymentRecord> Validator { get; } = new Validator<PaymentRecord>()
        .RuleFor(x => x.OrderId).Must(x => x > 0, _ => "is required")
        .RuleFor(x => x.Provider).NotEmpty()
        .RuleFor(x => x.Status).Must(x => PaymentStatus.All.Contains(x), x => $"invalid status '{x}'")
        .RuleFor(x => x.AmountCents).Must(x => x >= 0, _ => "must be >= 0")
        .RuleFor(x => x.CurrencyCode).NotEmpty()
        .Build();
}

public static class PaymentStatus
{
    public const string Pending = "pending";
    public const string Authorized = "authorized";
    public const string Captured = "captured";
    public const string Failed = "failed";
    public const string Refunded = "refunded";
    public static IReadOnlyList<string> All { get; } = [Pending, Authorized, Captured, Failed, Refunded];
}

public static class PaymentRecordExtensions
{
    public static Option Validate(this PaymentRecord record) => PaymentRecord.Validator.Validate(record).ToOptionStatus();
}
