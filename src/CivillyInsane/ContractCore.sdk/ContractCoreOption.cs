using Toolbox.Tools;
using Toolbox.Types;

namespace ContractCore.sdk;

public record ContractCoreOption
{
    public string ConnectionString { get; init; } = null!;

    public static IValidator<ContractCoreOption> Validator { get; } = new Validator<ContractCoreOption>()
        .RuleFor(x => x.ConnectionString).NotEmpty()
        .Build();
}

public static class ContractCoreOptionExtensions
{
    public static Option Validate(this ContractCoreOption option) => ContractCoreOption.Validator.Validate(option).ToOptionStatus();
}
