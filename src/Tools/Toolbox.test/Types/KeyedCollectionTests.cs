using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.test.Types;

public class KeyedCollectionTests
{
    [Fact]
    public void AddAndLookupHelpers()
    {
        var subject = new KeyedCollection<string, string>();

        subject.Add("flag");
        subject.Add("tag", "A");
        subject.Add("tag", "B");

        subject.ContainKey("flag").BeTrue();
        subject.ContainsKey("tag").BeTrue();
        subject.ContainKeyValue("tag", "A").BeTrue();
        subject.ContainsKeyValue("tag", "B").BeTrue();
        subject.ContainKeyValue("tag", "C").BeFalse();
        subject.IsKeyFlag("flag").BeTrue();
        subject.IsKeyFlag("tag").BeFalse();
        subject.IsKeyFlag("missing").BeFalse();

        subject.TryGetValue("tag", out var values).BeTrue();
        values.BeEquivalent(["A", "B"]);
        subject.Count.Be(2);
    }

    //[Fact]
    //public void ConstructorFromItems_ShouldInitializeFlagsAndValues()
    //{
    //    IReadOnlyList<KeyValuePair<string, string[]?>> items =
    //    [
    //        new("flag", null),
    //        new("tag", ["A", "B"]),
    //    ];

    //    var subject = new KeyedCollection<string, string>(items);

    //    subject.Count.Be(2);
    //    subject.IsKeyFlag("flag").BeTrue();
    //    subject.ContainKeyValue("tag", "A").BeTrue();
    //    subject.ContainKeyValue("tag", "B").BeTrue();
    //}

    [Fact]
    public void TryGetValue_ShouldReturnFalseAndEmpty_WhenMissing()
    {
        var subject = new KeyedCollection<string, string>();

        subject.TryGetValue("missing", out var values).BeFalse();
        values.BeEquivalent([]);
    }

    [Fact]
    public void RemoveKey_ShouldReturnExpectedResult()
    {
        var subject = new KeyedCollection<string, string>();
        subject.Add("tag", "A");

        subject.RemoveKey("tag").BeTrue();
        subject.RemoveKey("tag").BeFalse();
        subject.ContainKey("tag").BeFalse();
        subject.Count.Be(0);
    }

    [Fact]
    public void RemoveKeyValue_ShouldReturnExpectedResult()
    {
        var subject = new KeyedCollection<string, string>();
        subject.Add("tag", "A");

        subject.RemoveKeyValue("tag", "A").BeTrue();
        subject.RemoveKeyValue("tag", "A").BeFalse();
        subject.RemoveKeyValue("missing", "A").BeFalse();
        subject.IsKeyFlag("tag").BeTrue();
    }

    [Fact]
    public void Equals_ShouldCompareStructure()
    {
        var subject1 = new KeyedCollection<string, string>();
        subject1.Add("flag");
        subject1.Add("tag", "A", "B");

        var subject2 = new KeyedCollection<string, string>();
        subject2.Add("tag", "B");
        subject2.Add("flag");
        subject2.Add("tag", "A");

        subject1.Equals(subject2).BeTrue();
        subject2.Equals(subject1).BeTrue();
        subject1.GetHashCode().Be(subject2.GetHashCode());
        ((object)subject1).Equals(subject2).BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentStructures()
    {
        var subject = new KeyedCollection<string, string>();
        subject.Add("tag", "A");

        var differentKey = new KeyedCollection<string, string>();
        differentKey.Add("other", "A");

        var differentValue = new KeyedCollection<string, string>();
        differentValue.Add("tag", "B");

        subject.NotNull();
        subject.Equals(differentKey).BeFalse();
        subject.Equals(differentValue).BeFalse();
        ((object)subject).Equals("tag").BeFalse();
    }

    [Fact]
    public void DeepClone_ShouldCloneValues()
    {
        var subject = new KeyedCollection<string, Token>(valueComparer: TokenComparer.Instance);
        subject.Add("tag", new Token("A"));

        var clone = subject.DeepClone(valueClone: x => new Token(x.Value));

        clone.Equals(subject).BeTrue();

        subject.TryGetValue("tag", out var originalValues).BeTrue();
        clone.TryGetValue("tag", out var cloneValues).BeTrue();

        ReferenceEquals(originalValues[0], cloneValues[0]).BeFalse();
        cloneValues[0].Value.Be("A");

        clone.Add("tag", new Token("B"));
        subject.ContainKeyValue("tag", new Token("B")).BeFalse();
        clone.ContainKeyValue("tag", new Token("B")).BeTrue();
    }

    [Fact]
    public void DeepClone_ShouldCreateIndependentCopy_ByDefault()
    {
        var subject = new KeyedCollection<string, string>();
        subject.Add("flag");
        subject.Add("tag", "A");

        var clone = subject.DeepClone();

        clone.Equals(subject).BeTrue();

        clone.Add("tag", "B");
        subject.ContainKeyValue("tag", "B").BeFalse();
        clone.ContainKeyValue("tag", "B").BeTrue();
    }

    [Fact]
    public void ContainsAndEquality_ShouldHonorKeyComparer()
    {
        var subject = new KeyedCollection<string, string>(keyComparer: StringComparer.OrdinalIgnoreCase);
        subject.Add("Tag", "A");

        subject.ContainKey("tag").BeTrue();
        subject.ContainKeyValue("tag", "A").BeTrue();

        var other = new KeyedCollection<string, string>(keyComparer: StringComparer.OrdinalIgnoreCase);
        other.Add("tag", "A");

        subject.Equals(other).BeTrue();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnKeyReadonlyLists()
    {
        var subject = new KeyedCollection<string, string>();
        subject.Add("flag");
        subject.Add("tag", "A", "B");

        using IEnumerator<KeyValues<string, string>> enumerator = subject.GetEnumerator();
        var items = subject.ToDictionary(x => x.Key, x => x.Values);

        items.Count.Be(2);
        items["flag"].Count.Be(0);
        items["tag"].BeEquivalent(["A", "B"]);
        enumerator.MoveNext().BeTrue();
    }

    [Fact]
    public void NonGenericEnumerator_ShouldEnumerateKeyValuePairs()
    {
        var subject = new KeyedCollection<string, string>();
        subject.Add("flag");

        var items = new List<KeyValues<string, string>>();
        foreach (object? item in (IEnumerable)subject)
        {
            item.NotNull();
            items.Add((KeyValues<string, string>)item);
        }

        items.Count.Be(1);
        items[0].Key.Be("flag");
        items[0].Values.Count.Be(0);
    }

    [Fact]
    public void ToJson_ShouldRoundTrip()
    {
        var subject = new KeyedCollection<string, string>();
        subject.Add("flag");
        subject.Add("tag", "A", "B");

        string json = subject.ToJson();
        json.NotEmpty();

        var roundtrip = json.ToObject<KeyedCollection<string, string>>().NotNull();
        roundtrip.Equals(subject).BeTrue();
        (subject == roundtrip).BeTrue();
        (subject != roundtrip).BeFalse();
    }

    [Fact]
    public void ToJson_ShouldUseComparerCompatibleRoundTrip()
    {
        var subject = new KeyedCollection<string, string>(keyComparer: StringComparer.OrdinalIgnoreCase);
        subject.Add("Tag", "A");

        string json = subject.ToJson();
        var items = json.ToObject<List<KeyValues<string, string>>>().NotNull();
        var roundtrip = new KeyedCollection<string, string>(items, keyComparer: StringComparer.OrdinalIgnoreCase);

        roundtrip.Equals(subject).BeTrue();
        roundtrip.ContainKey("tag").BeTrue();
    }

    private sealed class Token(string value)
    {
        public string Value { get; set; } = value;
    }

    private sealed class TokenComparer : IEqualityComparer<Token>
    {
        public static TokenComparer Instance { get; } = new TokenComparer();

        public bool Equals(Token? x, Token? y) => string.Equals(x?.Value, y?.Value, StringComparison.Ordinal);

        public int GetHashCode(Token obj) => obj.Value.GetHashCode(StringComparison.Ordinal);
    }
}
