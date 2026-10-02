namespace Toolbox.Types;

public readonly struct KeyValue<TValue>
{
    public KeyValue(string key, TValue value)
    {
        this.Key = key;
        this.Value = value;
    }

    public string Key { get; init; }

    public TValue Value { get; init; }

    public override string ToString() => $"{Key}:{Value}";

    public void Deconstruct(out string key, out TValue value)
    {
        key = Key;
        value = Value;
    }

    public KeyValuePair<string, TValue> ToKeyValuePair() => new(Key, Value);

    public static implicit operator KeyValue<TValue>((string key, TValue value) source) => new(source.key, source.value);

    public static implicit operator (string key, TValue value)(KeyValue<TValue> source) => (source.Key, source.Value);
}
