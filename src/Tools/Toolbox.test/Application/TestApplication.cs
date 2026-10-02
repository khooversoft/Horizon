using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.test.Application;

internal static class TestApplication
{
    public static DatalakeStore GetDatalake(string basePath) => new DatalakeStore(ReadOption(basePath).Datalake, new NullLogger<DatalakeStore>());

    public static TestOption ReadOption(string basePath) => new ConfigurationBuilder()
        .AddJsonFile("TestSettings.json", optional: false, reloadOnChange: true)
        .AddUserSecrets("gfs-web-secrets")
        .Build()
        .Get<TestOption>().NotNull()
        .Func(x => x with { Datalake = x.Datalake with { BasePath = basePath } });
}


public record TestOption
{
    public DatalakeOption Datalake { get; init; } = null!;
    public string AdminConnectionString { get; init; } = null!;

    public static IValidator<TestOption> Validator { get; } = new Validator<TestOption>()
        .RuleFor(x => x.Datalake).Validate(DatalakeOption.Validator)
        .RuleFor(x => x.AdminConnectionString).NotEmpty()
        .Build();
}