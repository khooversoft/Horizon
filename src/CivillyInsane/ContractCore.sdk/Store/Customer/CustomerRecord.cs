using System.ComponentModel.DataAnnotations.Schema;
using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public record CustomerRecord
{
    [Column("customer_id")] public long? CustomerId { get; set; }
    [Column("email")] public string Email { get; set; } = null!;
    [Column("first_name")] public string? FirstName { get; set; }
    [Column("last_name")] public string? LastName { get; set; }
    [Column("phone")] public string? Phone { get; set; }
    [Column("created_utc")] public DateTime CreatedUtc { get; set; }
    [Column("modified_utc")] public DateTime ModifiedUtc { get; set; }
    [Column("JsonData")] public string? JsonData { get; set; }

    public static IValidator<CustomerRecord> Validator { get; } = new Validator<CustomerRecord>()
        .RuleFor(x => x.Email).ValidEmail()
        .Build();
}

public static class CustomerRecordExtensions
{
    public static Option Validate(this CustomerRecord record) => CustomerRecord.Validator.Validate(record).ToOptionStatus();
}
