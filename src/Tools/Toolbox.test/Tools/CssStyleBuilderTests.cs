using System.Collections;
using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class CssStyleBuilderTests
{
    [Fact]
    public void DefaultConstructorHasNoStyles()
    {
        var builder = new CssStyleBuilder();

        builder.Count.Be(0);
        builder.ToString().Be(string.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValueConstructorWithEmptyValueIsBlank(string? value)
    {
        var builder = new CssStyleBuilder(value);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void ValueConstructorWithSingleDeclaration()
    {
        var builder = new CssStyleBuilder("color: red");

        builder.Count.Be(1);
        builder.ToString().Be("color: red");
    }

    [Fact]
    public void ValueConstructorWithMultipleDeclarations()
    {
        var builder = new CssStyleBuilder("color: red; margin: 0");

        builder.Count.Be(2);
        builder.ToString().Be("color: red; margin: 0");
    }

    [Fact]
    public void ValueConstructorTrimsPropertyAndValue()
    {
        var builder = new CssStyleBuilder("  color :  red  ;  margin : 0 ");

        builder.Count.Be(2);
        builder.ToString().Be("color: red; margin: 0");
    }

    [Fact]
    public void ValueConstructorIgnoresEmptyDeclarations()
    {
        var builder = new CssStyleBuilder("color: red;; ; margin: 0;");

        builder.Count.Be(2);
        builder.ToString().Be("color: red; margin: 0");
    }

    [Fact]
    public void PropertyWithNoValueRendersKeyOnly()
    {
        var builder = new CssStyleBuilder("color");

        builder.Count.Be(1);
        builder.ToString().Be("color");
    }

    [Fact]
    public void PropertyWithColonButEmptyValueRendersKeyOnly()
    {
        var builder = new CssStyleBuilder("color:");

        builder.Count.Be(1);
        builder.ToString().Be("color");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddEmptyValueIsIgnored(string? value)
    {
        var builder = new CssStyleBuilder();

        builder.Add(value);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void AddSingleDeclaration()
    {
        var builder = new CssStyleBuilder();

        builder.Add("color:   red");

        builder.Count.Be(1);
        builder.ToString().Be("color: red");
    }

    [Fact]
    public void AddIsChainable()
    {
        var builder = new CssStyleBuilder();

        var result = builder.Add("color: red").Add("margin: 0");

        ReferenceEquals(builder, result).BeTrue();
        builder.ToString().Be("color: red; margin: 0");
    }

    [Fact]
    public void AddSemicolonSeparatedDeclarationsSplitsThem()
    {
        var builder = new CssStyleBuilder();

        builder.Add("color: red; margin: 0");

        builder.Count.Be(2);
        builder.ToString().Be("color: red; margin: 0");
    }

    [Fact]
    public void AddWithIncludeTrueAddsValue()
    {
        var builder = new CssStyleBuilder();

        builder.Add("color: red", true);

        builder.ToString().Be("color: red");
    }

    [Fact]
    public void AddWithIncludeFalseSkipsValue()
    {
        var builder = new CssStyleBuilder();

        builder.Add("color: red", false);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void AddWithPredicateTrueAddsValue()
    {
        var builder = new CssStyleBuilder();

        builder.Add("color: red", () => true);

        builder.ToString().Be("color: red");
    }

    [Fact]
    public void AddWithPredicateFalseSkipsValue()
    {
        var builder = new CssStyleBuilder();

        builder.Add("color: red", () => false);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void AddWithNullPredicateThrows()
    {
        var builder = new CssStyleBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Add("color: red", (Func<bool>)null!));
    }

    [Fact]
    public void AddWithEmptyValueAndPredicateDoesNotInvokePredicate()
    {
        var builder = new CssStyleBuilder();
        bool invoked = false;

        builder.Add("", () => { invoked = true; return true; });

        invoked.BeFalse();
        builder.Count.Be(0);
    }

    [Fact]
    public void AddPropertyValuePair()
    {
        var builder = new CssStyleBuilder();

        builder.Add("color", "red");

        builder.Count.Be(1);
        builder.ToString().Be("color: red");
    }

    [Fact]
    public void AddPropertyValuePairTrimsBoth()
    {
        var builder = new CssStyleBuilder();

        builder.Add("  color  ", "  red  ");

        builder.ToString().Be("color: red");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddPropertyWithEmptyValueRendersKeyOnly(string? value)
    {
        var builder = new CssStyleBuilder();

        builder.Add("color", value);

        builder.Count.Be(1);
        builder.ToString().Be("color");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddPropertyValuePairWithEmptyPropertyIsIgnored(string? property)
    {
        var builder = new CssStyleBuilder();

        builder.Add(property!, "red");

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void AddEnumerableNullIsIgnored()
    {
        var builder = new CssStyleBuilder();

        builder.Add((IEnumerable<string?>)null!);

        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void AddEnumerableAddsAllValues()
    {
        var builder = new CssStyleBuilder();

        builder.Add(new[] { "color: red", "margin: 0" });

        builder.Count.Be(2);
        builder.ToString().Be("color: red; margin: 0");
    }

    [Fact]
    public void AddEnumerableSkipsNullAndEmptyValues()
    {
        var builder = new CssStyleBuilder();

        builder.Add(new[] { "color: red", null, "", "   ", "margin: 0" });

        builder.Count.Be(2);
        builder.ToString().Be("color: red; margin: 0");
    }

    [Fact]
    public void CountReflectsRawAddedDeclarationsNotDeduped()
    {
        var builder = new CssStyleBuilder();

        builder.Add("color: red; color: blue");

        builder.Count.Be(2);
        builder.ToString().Be("color: blue");
    }

    [Fact]
    public void RemoveKeyWithNullReturnsFalse()
    {
        var builder = new CssStyleBuilder("color: red");

        builder.RemoveKey(null).BeFalse();
        builder.Count.Be(1);
    }

    [Fact]
    public void RemoveKeyWithEmptyReturnsFalse()
    {
        var builder = new CssStyleBuilder("color: red");

        builder.RemoveKey("").BeFalse();
        builder.Count.Be(1);
    }

    [Fact]
    public void RemoveKeyNotPresentReturnsFalse()
    {
        var builder = new CssStyleBuilder("color: red");

        builder.RemoveKey("margin").BeFalse();
        builder.ToString().Be("color: red");
    }

    [Fact]
    public void RemoveKeyPresentReturnsTrueAndRemoves()
    {
        var builder = new CssStyleBuilder("color: red; margin: 0");

        builder.RemoveKey("color").BeTrue();
        builder.ToString().Be("margin: 0");
    }

    [Fact]
    public void RemoveKeyIsCaseInsensitive()
    {
        var builder = new CssStyleBuilder("Color: red; margin: 0");

        builder.RemoveKey("COLOR").BeTrue();
        builder.ToString().Be("margin: 0");
    }

    [Fact]
    public void RemoveKeyRemovesAllOccurrences()
    {
        var builder = new CssStyleBuilder();
        builder.Add("color", "red").Add("margin", "0").Add("color", "blue");

        builder.RemoveKey("color").BeTrue();
        builder.Count.Be(1);
        builder.ToString().Be("margin: 0");
    }

    [Fact]
    public void ClearRemovesAllValues()
    {
        var builder = new CssStyleBuilder("color: red; margin: 0");

        var result = builder.Clear();

        ReferenceEquals(builder, result).BeTrue();
        builder.Count.Be(0);
        builder.ToString().BeEmpty();
    }

    [Fact]
    public void ToStringEmptyWhenNoStyles()
    {
        var builder = new CssStyleBuilder();

        builder.ToString().Be(string.Empty);
    }

    [Fact]
    public void ToStringDedupeLastValueWins()
    {
        var builder = new CssStyleBuilder("color: red; color: blue");

        builder.ToString().Be("color: blue");
    }

    [Fact]
    public void ToStringLastOccurrenceWinsPosition()
    {
        var builder = new CssStyleBuilder("color: red; margin: 0; color: blue");

        builder.ToString().Be("margin: 0; color: blue");
    }

    [Fact]
    public void ToStringDedupePropertyIsCaseInsensitive()
    {
        var builder = new CssStyleBuilder("color: red; COLOR: blue");

        builder.ToString().Be("COLOR: blue");
    }

    [Fact]
    public void EnumeratorReturnsDedupedStyles()
    {
        var builder = new CssStyleBuilder("color: red; margin: 0; color: blue");

        var items = builder.ToList();

        items.Count.Be(2);
        items[0].Key.Be("margin");
        items[0].Value.Be("0");
        items[1].Key.Be("color");
        items[1].Value.Be("blue");
    }

    [Fact]
    public void EnumeratorYieldsNullValueForValuelessProperty()
    {
        var builder = new CssStyleBuilder("color");

        var item = builder.Single();

        item.Key.Be("color");
        (item.Value is null).BeTrue();
    }

    [Fact]
    public void NonGenericEnumeratorReturnsDedupedStyles()
    {
        var builder = new CssStyleBuilder("color: red; color: blue");

        var items = new List<KeyValuePair<string, string?>>();
        foreach (var item in (IEnumerable)builder) items.Add((KeyValuePair<string, string?>)item);

        items.Count.Be(1);
        items[0].Key.Be("color");
        items[0].Value.Be("blue");
    }

    [Fact]
    public void PlusOperatorWithStringAddsValue()
    {
        var builder = new CssStyleBuilder("color: red");

        builder += "margin: 0";

        builder.ToString().Be("color: red; margin: 0");
    }

    [Fact]
    public void PlusOperatorWithNullBuilderThrows()
    {
        Assert.Throws<ArgumentNullException>(() => (CssStyleBuilder)null! + "color: red");
    }

    [Fact]
    public void PlusOperatorWithEnumerableAddsValues()
    {
        var builder = new CssStyleBuilder("color: red");

        builder += new[] { "margin: 0", "padding: 1px" };

        builder.ToString().Be("color: red; margin: 0; padding: 1px");
    }

    [Fact]
    public void PlusOperatorWithNullBuilderAndEnumerableThrows()
    {
        Assert.Throws<ArgumentNullException>(() => (CssStyleBuilder)null! + new[] { "color: red" });
    }

    [Fact]
    public void MinusOperatorRemovesKey()
    {
        var builder = new CssStyleBuilder("color: red; margin: 0");

        builder -= "color";

        builder.ToString().Be("margin: 0");
    }

    [Fact]
    public void MinusOperatorWithNullBuilderThrows()
    {
        Assert.Throws<ArgumentNullException>(() => (CssStyleBuilder)null! - "color");
    }

    [Fact]
    public void EqualityWhenSameReference()
    {
        var builder = new CssStyleBuilder("color: red");

#pragma warning disable CS1718 // Comparison made to same variable
        (builder == builder).BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
    }

    [Fact]
    public void EqualityWhenSameStylesDifferentPropertyCase()
    {
        var left = new CssStyleBuilder("color: red; margin: 0");
        var right = new CssStyleBuilder("COLOR: red; MARGIN: 0");

        (left == right).BeTrue();
        left.Equals(right).BeTrue();
    }

    [Fact]
    public void InequalityWhenValueCaseDiffers()
    {
        var left = new CssStyleBuilder("color: red");
        var right = new CssStyleBuilder("color: RED");

        (left != right).BeTrue();
    }

    [Fact]
    public void InequalityWhenDifferentStyles()
    {
        var left = new CssStyleBuilder("color: red");
        var right = new CssStyleBuilder("margin: 0");

        (left != right).BeTrue();
    }

    [Fact]
    public void InequalityWhenDifferentCount()
    {
        var left = new CssStyleBuilder("color: red; margin: 0");
        var right = new CssStyleBuilder("color: red");

        (left == right).BeFalse();
    }

    [Fact]
    public void EqualityWithNullOperands()
    {
        var builder = new CssStyleBuilder("color: red");

        (builder == null).BeFalse();
        (null == builder).BeFalse();
        ((CssStyleBuilder?)null == (CssStyleBuilder?)null).BeTrue();
    }

    [Fact]
    public void EqualsWithNonCssStyleBuilderReturnsFalse()
    {
        var builder = new CssStyleBuilder("color: red");

        builder.Equals("color: red").BeFalse();
        builder.Equals(null).BeFalse();
    }

    [Fact]
    public void GetHashCodeIsEqualForEqualBuilders()
    {
        var left = new CssStyleBuilder("color: red; margin: 0");
        var right = new CssStyleBuilder("COLOR: red; MARGIN: 0");

        left.GetHashCode().Be(right.GetHashCode());
    }

    [Fact]
    public void GetHashCodeIsStableAcrossCalls()
    {
        var builder = new CssStyleBuilder("color: red; margin: 0");

        builder.GetHashCode().Be(builder.GetHashCode());
    }
}
