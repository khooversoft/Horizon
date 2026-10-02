using System.Collections;
using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class CssBuilderTests
{
    [Fact]
    public void NoValuesReturnBlank()
    {
        var builder = new CssBuilder();

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void DefaultConstructorHasNoClasses()
    {
        var builder = new CssBuilder();

        builder.Count.Be(0);
        builder.ToString().Be(string.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValueConstructorWithEmptyValueIsBlank(string? value)
    {
        var builder = new CssBuilder(value);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void ValueConstructorWithSingleClass()
    {
        var builder = new CssBuilder("btn");

        builder.Count.Be(1);
        builder.ToString().Be("btn");
    }

    [Fact]
    public void ValueConstructorWithMultipleSpaceSeparatedClasses()
    {
        var builder = new CssBuilder("btn btn-primary active");

        builder.Count.Be(3);
        builder.ToString().Be("btn btn-primary active");
    }

    [Fact]
    public void ValueConstructorTrimsAndCollapsesExtraWhitespace()
    {
        var builder = new CssBuilder("  btn    btn-primary   ");

        builder.Count.Be(2);
        builder.ToString().Be("btn btn-primary");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddEmptyValueIsIgnored(string? value)
    {
        var builder = new CssBuilder();

        builder.Add(value);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void AddSingleClass()
    {
        var builder = new CssBuilder();

        builder.Add("btn");

        builder.Count.Be(1);
        builder.ToString().Be("btn");
    }

    [Fact]
    public void AddIsChainable()
    {
        var builder = new CssBuilder();

        var result = builder.Add("btn").Add("active");

        ReferenceEquals(builder, result).BeTrue();
        builder.ToString().Be("btn active");
    }

    [Fact]
    public void AddSpaceSeparatedClassesSplitsThem()
    {
        var builder = new CssBuilder();

        builder.Add("btn btn-primary");

        builder.Count.Be(2);
        builder.ToString().Be("btn btn-primary");
    }

    [Fact]
    public void AddWithIncludeTrueAddsValue()
    {
        var builder = new CssBuilder();

        builder.Add("btn", true);

        builder.ToString().Be("btn");
    }

    [Fact]
    public void AddWithIncludeFalseSkipsValue()
    {
        var builder = new CssBuilder();

        builder.Add("btn", false);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void AddWithPredicateTrueAddsValue()
    {
        var builder = new CssBuilder();

        builder.Add("btn", () => true);

        builder.ToString().Be("btn");
    }

    [Fact]
    public void AddWithPredicateFalseSkipsValue()
    {
        var builder = new CssBuilder();

        builder.Add("btn", () => false);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void AddWithNullPredicateThrows()
    {
        var builder = new CssBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Add("btn", (Func<bool>)null!));
    }

    [Fact]
    public void AddWithEmptyValueAndPredicateDoesNotInvokePredicate()
    {
        var builder = new CssBuilder();
        bool invoked = false;

        builder.Add("", () => { invoked = true; return true; });

        invoked.BeFalse();
        builder.Count.Be(0);
    }

    [Fact]
    public void AddEnumerableNullIsIgnored()
    {
        var builder = new CssBuilder();

        builder.Add((IEnumerable<string?>)null!);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void AddEnumerableAddsAllValues()
    {
        var builder = new CssBuilder();

        builder.Add(new[] { "btn", "active" });

        builder.Count.Be(2);
        builder.ToString().Be("btn active");
    }

    [Fact]
    public void AddEnumerableSkipsNullAndEmptyValues()
    {
        var builder = new CssBuilder();

        builder.Add(new[] { "btn", null, "", "   ", "active" });

        builder.Count.Be(2);
        builder.ToString().Be("btn active");
    }

    [Fact]
    public void CountReflectsRawAddedTokensNotDeduped()
    {
        var builder = new CssBuilder();

        builder.Add("btn btn active");

        builder.Count.Be(3);
        builder.ToString().Be("btn active");
    }

    [Fact]
    public void RemoveKeyWithNullReturnsFalse()
    {
        var builder = new CssBuilder("btn");

        builder.RemoveKey(null).BeFalse();
        builder.Count.Be(1);
    }

    [Fact]
    public void RemoveKeyWithEmptyReturnsFalse()
    {
        var builder = new CssBuilder("btn");

        builder.RemoveKey("").BeFalse();
        builder.Count.Be(1);
    }

    [Fact]
    public void RemoveKeyNotPresentReturnsFalse()
    {
        var builder = new CssBuilder("btn");

        builder.RemoveKey("missing").BeFalse();
        builder.ToString().Be("btn");
    }

    [Fact]
    public void RemoveKeyPresentReturnsTrueAndRemoves()
    {
        var builder = new CssBuilder("btn active");

        builder.RemoveKey("btn").BeTrue();
        builder.ToString().Be("active");
    }

    [Fact]
    public void RemoveKeyIsCaseInsensitive()
    {
        var builder = new CssBuilder("Btn active");

        builder.RemoveKey("BTN").BeTrue();
        builder.ToString().Be("active");
    }

    [Fact]
    public void RemoveKeyRemovesAllOccurrences()
    {
        var builder = new CssBuilder();
        builder.Add("btn").Add("active").Add("btn");

        builder.RemoveKey("btn").BeTrue();
        builder.Count.Be(1);
        builder.ToString().Be("active");
    }

    [Fact]
    public void ClearRemovesAllValues()
    {
        var builder = new CssBuilder("btn active");

        var result = builder.Clear();

        ReferenceEquals(builder, result).BeTrue();
        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void ToStringDedupesCollapsingCaseInsensitive()
    {
        var builder = new CssBuilder("btn BTN");

        builder.ToString().Be("BTN");
    }

    [Fact]
    public void ToStringLastOccurrenceWinsPosition()
    {
        var builder = new CssBuilder("a b a");

        builder.ToString().Be("b a");
    }

    [Fact]
    public void ToStringPreservesOriginalCasingOfLastOccurrence()
    {
        var builder = new CssBuilder("Btn active btn");

        builder.ToString().Be("active btn");
    }

    [Fact]
    public void EnumeratorReturnsDedupedClasses()
    {
        var builder = new CssBuilder("a b a");

        var items = builder.ToList();

        items.Count.Be(2);
        items[0].Be("b");
        items[1].Be("a");
    }

    [Fact]
    public void NonGenericEnumeratorReturnsDedupedClasses()
    {
        var builder = new CssBuilder("btn btn active");

        var items = new List<string>();
        foreach (var item in (IEnumerable)builder) items.Add((string)item);

        items.Count.Be(2);
        items[0].Be("btn");
        items[1].Be("active");
    }

    [Fact]
    public void PlusOperatorWithStringAddsValue()
    {
        var builder = new CssBuilder("btn");

        builder += "active";

        builder.ToString().Be("btn active");
    }

    [Fact]
    public void PlusOperatorWithNullBuilderThrows()
    {
        Assert.Throws<ArgumentNullException>(() => (CssBuilder)null! + "btn");
    }

    [Fact]
    public void PlusOperatorWithEnumerableAddsValues()
    {
        var builder = new CssBuilder("btn");

        builder += new[] { "active", "large" };

        builder.ToString().Be("btn active large");
    }

    [Fact]
    public void PlusOperatorWithNullBuilderAndEnumerableThrows()
    {
        Assert.Throws<ArgumentNullException>(() => (CssBuilder)null! + new[] { "btn" });
    }

    [Fact]
    public void MinusOperatorRemovesKey()
    {
        var builder = new CssBuilder("btn active");

        builder -= "btn";

        builder.ToString().Be("active");
    }

    [Fact]
    public void MinusOperatorWithNullBuilderThrows()
    {
        Assert.Throws<ArgumentNullException>(() => (CssBuilder)null! - "btn");
    }

    [Fact]
    public void EqualityWhenSameReference()
    {
        var builder = new CssBuilder("btn");

#pragma warning disable CS1718 // Comparison made to same variable
        (builder == builder).BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
    }

    [Fact]
    public void EqualityWhenSameClassesDifferentCase()
    {
        var left = new CssBuilder("btn active");
        var right = new CssBuilder("BTN ACTIVE");

        (left == right).BeTrue();
        left.Equals(right).BeTrue();
    }

    [Fact]
    public void InequalityWhenDifferentClasses()
    {
        var left = new CssBuilder("btn");
        var right = new CssBuilder("active");

        (left != right).BeTrue();
    }

    [Fact]
    public void InequalityWhenDifferentCount()
    {
        var left = new CssBuilder("btn active");
        var right = new CssBuilder("btn");

        (left == right).BeFalse();
    }

    [Fact]
    public void EqualityWithNullOperands()
    {
        var builder = new CssBuilder("btn");

        (builder == null).BeFalse();
        (null == builder).BeFalse();
        ((CssBuilder?)null == (CssBuilder?)null).BeTrue();
    }

    [Fact]
    public void EqualsWithNonCssBuilderReturnsFalse()
    {
        var builder = new CssBuilder("btn");

        builder.Equals("btn").BeFalse();
        builder.Equals(null).BeFalse();
    }

    [Fact]
    public void GetHashCodeIsEqualForEqualBuilders()
    {
        var left = new CssBuilder("btn active");
        var right = new CssBuilder("BTN ACTIVE");

        left.GetHashCode().Be(right.GetHashCode());
    }

    [Fact]
    public void GetHashCodeIsStableAcrossCalls()
    {
        var builder = new CssBuilder("btn active");

        builder.GetHashCode().Be(builder.GetHashCode());
    }
}
