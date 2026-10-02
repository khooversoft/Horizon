using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.test.Types;

public class ConcurrentHashSetTests
{
    [Fact]
    public void AddListAndRemove()
    {
        ConcurrentHashSet<string> set = new ConcurrentHashSet<string>();
        set.Add("A");
        set.Add("B");
        set.Add("C");
        set.Count.Be(3);
        set.Contains("A").BeTrue();
        set.Contains("B").BeTrue();
        set.Contains("C").BeTrue();
        set.Remove("B");
        set.Count.Be(2);
        set.Contains("A").BeTrue();
        set.Contains("B").BeFalse();
        set.Contains("C").BeTrue();
    }

    [Fact]
    public void AddDuplicate_ShouldNotIncreaseCount()
    {
        var set = new ConcurrentHashSet<string>();

        set.Add("A");
        set.Add("A");

        set.Count.Be(1);
        set.Contains("A").BeTrue();
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenItemAlreadyExists()
    {
        var set = new ConcurrentHashSet<string>();

        set.TryAdd("A").BeTrue();
        set.TryAdd("A").BeFalse();
        set.Count.Be(1);
    }

    [Fact]
    public void TryRemove_ShouldReturnTrueOnlyWhenItemExists()
    {
        var set = new ConcurrentHashSet<string>();
        set.Add("A");

        set.TryRemove("A").BeTrue();
        set.TryRemove("A").BeFalse();
        set.Count.Be(0);
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        var set = new ConcurrentHashSet<string>();

        set.Remove("A").BeFalse();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var set = new ConcurrentHashSet<string>();
        set.Add("A");
        set.Add("B");

        set.Clear();

        set.Count.Be(0);
        set.Contains("A").BeFalse();
        set.Contains("B").BeFalse();
        set.IsReadOnly.BeFalse();
    }

    [Fact]
    public void Constructor_ShouldUseComparer()
    {
        var set = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);

        set.Add("A");

        set.Contains("a").BeTrue();
        set.TryAdd("a").BeFalse();
        set.Count.Be(1);
    }

    [Fact]
    public void GetEnumerator_ShouldReturnAllItems()
    {
        var set = new ConcurrentHashSet<string>();
        set.Add("A");
        set.Add("B");
        set.Add("C");

        set.ToArray().BeEquivalent(["A", "B", "C"]);
    }

    [Fact]
    public void CopyTo_ShouldCopyItemsAtIndex()
    {
        var set = new ConcurrentHashSet<string>();
        set.Add("A");
        set.Add("B");
        set.Add("C");

        string[] array = ["X", "X", "X", "X", "X"];

        set.CopyTo(array, 1);

        array[0].Be("X");
        array[4].Be("X");
        array.Skip(1).Take(3).BeEquivalent(["A", "B", "C"]);
    }

    [Fact]
    public void CopyTo_ShouldThrow_WhenArrayIsNull()
    {
        var set = new ConcurrentHashSet<string>();

        Verify.Throws<ArgumentNullException>(() => set.CopyTo(null!, 0));
    }

    [Fact]
    public void CopyTo_ShouldThrow_WhenArrayIndexIsNegative()
    {
        var set = new ConcurrentHashSet<string>();

        Verify.Throws<ArgumentOutOfRangeException>(() => set.CopyTo([], -1));
    }

    [Fact]
    public void CopyTo_ShouldThrow_WhenArrayDoesNotHaveEnoughSpace()
    {
        var set = new ConcurrentHashSet<string>();
        set.Add("A");
        set.Add("B");

        Verify.Throws<ArgumentException>(() => set.CopyTo(new string[2], 1));
    }
}
