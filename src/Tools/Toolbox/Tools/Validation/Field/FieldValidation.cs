using Toolbox.Extensions;

namespace Toolbox.Tools;

public enum FieldValidator
{
    Required,
    Standard,
}

public static class FieldValidation
{
    public static string? StandardFormat(string? value) => value switch
    {
        _ when value.IsEmpty() => "Required.",
        string v => !v.All(c => char.IsLetterOrDigit(c) || c is '-' or '.') ? "Only letters, numbers, '-' or '.' or '@' are allowed." : null
    };

    public static string? RequiredFormat(string? value) => value switch
    {
        _ when value.IsEmpty() => "Required.",
        _ => null,
    };

    public static FieldValidator<string> Create(FieldValidator validator) => validator switch
    {
        FieldValidator.Standard => new(StandardFormat),
        FieldValidator.Required => new(RequiredFormat),
        _ => throw new NotImplementedException(),
    };
}
