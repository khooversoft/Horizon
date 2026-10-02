namespace Toolbox.Tools;

public class FieldValidator<T>
{
    private List<Func<T?, string?>> _validators = new();
    public FieldValidator() { }
    public FieldValidator(Func<T?, string?> validation) => this.Add(validation);

    public bool IsError => ErrorText is not null;
    public string? ErrorText { get; private set; }

    public void Add(Func<T?, string?> validation) => _validators.Add(validation.NotNull());


    public void Validate(T? value) => ErrorText = _validators
        .Select(x => x(value))
        .FirstOrDefault(x => x is not null);
}