using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Tools;

namespace Toolbox.test.Data;

public class CacheClientTests
{
    private ITestOutputHelper _outputHelper;

    public CacheClientTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private record PersonRecord(string Name, int Age);

    private IHost CreateHost()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddCacheClient(x => x.ToLowerInvariant(), TimeSpan.FromMilliseconds(500));
            })
            .Build();

        return host;
    }

    private IHost CreateGenericHost<T>()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddCacheClient<T>(x => x.ToLowerInvariant(), TimeSpan.FromMilliseconds(500));
            })
            .Build();

        return host;
    }

    [Fact]
    public void SingleRoundTrip()
    {
        using var host = CreateHost();
        const string key = "TestKey";
        const string value = "TestValue";

        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        cacheClient.TryGetValue<string>(key, out var cachedValue).BeFalse();
        cacheClient.Upsert(key, value);

        cacheClient.TryGetValue<string>(key.ToLowerInvariant(), out cachedValue).BeTrue();
        cachedValue.Be(value);

        cacheClient.Remove(key);
        cacheClient.TryGetValue<string>(key, out _).BeFalse();
    }

    [Fact]
    public void Update()
    {
        using var host = CreateHost();
        const string key = "TestKey";
        const string value1 = "TestValue";
        const string value2 = "TestValue2";

        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        cacheClient.TryGetValue<string>(key, out var cachedValue).BeFalse();
        cacheClient.Upsert(key, value1);

        cacheClient.TryGetValue<string>(key.ToLowerInvariant(), out cachedValue).BeTrue();
        cachedValue.Be(value1);

        cacheClient.Upsert(key, value2);
        cacheClient.TryGetValue<string>(key, out cachedValue).BeTrue();
        cachedValue.Be(value2);

        cacheClient.Remove(key);
        cacheClient.TryGetValue<string>(key, out _).BeFalse();
    }

    [Fact]
    public async Task TimeoutWithUpdate()
    {
        using var host = CreateHost();
        const string key = "TestKey";
        const string value1 = "TestValue";
        const string value2 = "TestValue2";

        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        cacheClient.TryGetValue<string>(key, out var cachedValue).BeFalse();
        cacheClient.Upsert(key, value1);

        cacheClient.TryGetValue<string>(key.ToLowerInvariant(), out cachedValue).BeTrue();
        cachedValue.Be(value1);

        await Task.Delay(TimeSpan.FromMilliseconds(600));
        cacheClient.TryGetValue<string>(key, out _).BeFalse();

        cacheClient.Upsert(key, value2);
        cacheClient.TryGetValue<string>(key, out cachedValue).BeTrue();
        cachedValue.Be(value2);

        cacheClient.Remove(key);
        cacheClient.TryGetValue<string>(key, out _).BeFalse();
    }

    [Fact]
    public void GenericCacheClient_CanResolveFromDI()
    {
        using var host = CreateGenericHost<string>();

        ICacheClient<string> cacheClient = host.Services.GetRequiredService<ICacheClient<string>>();
        Verify.NotNull(cacheClient);

        const string key = "GenericKey";
        const string value = "GenericValue";

        cacheClient.TryGetValue<string>(key, out _).BeFalse();
        cacheClient.Upsert(key, value);
        cacheClient.TryGetValue<string>(key, out var cachedValue).BeTrue();
        cachedValue.Be(value);
    }

    [Fact]
    public void MultipleKeys_AreIndependent()
    {
        using var host = CreateHost();
        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        cacheClient.Upsert("key1", "value1");
        cacheClient.Upsert("key2", "value2");
        cacheClient.Upsert("key3", "value3");

        cacheClient.TryGetValue<string>("key1", out var v1).BeTrue();
        cacheClient.TryGetValue<string>("key2", out var v2).BeTrue();
        cacheClient.TryGetValue<string>("key3", out var v3).BeTrue();

        v1.Be("value1");
        v2.Be("value2");
        v3.Be("value3");

        cacheClient.Remove("key2");

        cacheClient.TryGetValue<string>("key1", out _).BeTrue();
        cacheClient.TryGetValue<string>("key2", out _).BeFalse();
        cacheClient.TryGetValue<string>("key3", out _).BeTrue();
    }

    [Fact]
    public void ComplexObjectRoundTrip()
    {
        using var host = CreateHost();
        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        const string key = "PersonKey";
        var person = new PersonRecord("Alice", 30);

        cacheClient.TryGetValue<PersonRecord>(key, out _).BeFalse();
        cacheClient.Upsert(key, person);

        cacheClient.TryGetValue<PersonRecord>(key, out var cached).BeTrue();
        cached.Be(person);
    }

    [Fact]
    public async Task SlidingExpiration_ResetsOnAccess()
    {
        using var host = CreateHost();
        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        const string key = "SlidingKey";
        cacheClient.Upsert(key, "value");

        // Access the item twice before expiry to reset the sliding window each time
        await Task.Delay(TimeSpan.FromMilliseconds(300));
        cacheClient.TryGetValue<string>(key, out _).BeTrue();

        await Task.Delay(TimeSpan.FromMilliseconds(300));
        cacheClient.TryGetValue<string>(key, out _).BeTrue();

        // Now let it actually expire
        await Task.Delay(TimeSpan.FromMilliseconds(600));
        cacheClient.TryGetValue<string>(key, out _).BeFalse();
    }

    [Fact]
    public void EmptyKey_ThrowsOnUpsert()
    {
        using var host = CreateHost();
        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        Verify.Throws<ArgumentNullException>(() => cacheClient.Upsert(string.Empty, "value"));
    }

    [Fact]
    public void EmptyKey_ThrowsOnTryGetValue()
    {
        using var host = CreateHost();
        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        Verify.Throws<ArgumentNullException>(() => cacheClient.TryGetValue<string>(string.Empty, out _));
    }

    [Fact]
    public void EmptyKey_ThrowsOnRemove()
    {
        using var host = CreateHost();
        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        Verify.Throws<ArgumentNullException>(() => cacheClient.Remove(string.Empty));
    }

    [Fact]
    public void NullValue_ThrowsOnUpsert()
    {
        using var host = CreateHost();
        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        Verify.Throws<ArgumentNullException>(() => cacheClient.Upsert<string>("key", null!));
    }

    [Fact]
    public void TypeMismatch_TryGetValue_ReturnsFalse()
    {
        using var host = CreateHost();
        ICacheClient cacheClient = host.Services.GetRequiredService<ICacheClient>();

        cacheClient.Upsert("TypeKey", "stringValue");

        // Retrieving as a different type should not succeed
        cacheClient.TryGetValue<int>("TypeKey", out _).BeFalse();
    }
}
