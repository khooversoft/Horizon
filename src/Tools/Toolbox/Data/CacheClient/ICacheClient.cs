using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Data;

public interface ICacheClient
{
    bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? value);
    void Upsert<T>(string key, T value);
    void Remove(string key);
}

public interface ICacheClient<T> : ICacheClient
{
    bool TryGetValue(string key, [NotNullWhen(true)] out T? value) => TryGetValue<T>(key, out value);
    void Upsert(string key, T value) => Upsert<T>(key, value);
}
