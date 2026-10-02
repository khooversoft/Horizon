using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.test.Types;

public class RingBufferTests
{
    [Fact]
    public void EmptyTest()
    {
        var r = new RingBuffer<int>(1);
        r.Count.Be(0);
        r.Index.Be(0);
        r.Size.Be(1);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSizeIsZero()
    {
        Verify.Throws<ArgumentException>(() => new RingBuffer<int>(0));
    }

    [Fact]
    public void Add_ShouldIncreaseCount_UntilCapacity()
    {
        var r = new RingBuffer<int>(3);

        r.Add(10);
        r.Count.Be(1);
        r.Index.Be(1);
        r.Get().Return().Be(10);

        r.Add(20);
        r.Count.Be(2);
        r.Index.Be(2);
        r.Get().Return().Be(20);

        r.Add(30);
        r.Count.Be(3);
        r.Index.Be(0);
        r.Get().Return().Be(30);

        r.Add(40);
        r.Count.Be(3);
        r.Index.Be(1);
        r.Get().Return().Be(40);

        r.ToArray().Be([20, 30, 40]);
    }

    [Fact]
    public void Get_ShouldReturnItemsInInsertionOrder_WhenNotFull()
    {
        var r = new RingBuffer<int>(5);
        r.Add(1);
        r.Add(2);
        r.Add(3);

        r.ToArray().Be(new[] { 1, 2, 3 });
        r.Get().Return().Be(3);
    }

    [Fact]
    public void Get_ShouldReturnItemsInRingOrder_WhenWrapped()
    {
        var r = new RingBuffer<int>(3);
        r.Add(1);
        r.Add(2);
        r.Add(3);
        r.Add(4);
        r.Add(5);

        r.ToArray().Be(new[] { 3, 4, 5 });
        r.Get().Return().Be(5);
    }

    [Fact]
    public void Enumeration_ShouldMatchGetOrder()
    {
        var r = new RingBuffer<int>(3);
        r.Add(10);
        r.Add(20);
        r.Add(30);
        r.Add(40);

        r.ToArray().Be(new[] { 20, 30, 40 });
    }

    [Fact]
    public void Get_ShouldReturnEmpty_WhenBufferIsEmpty()
    {
        var r = new RingBuffer<int>(3);

        r.Get().IsNotFound().BeTrue();
        r.ToArray().Length.Be(0);
    }

    [Fact]
    public void Clear_ShouldResetState()
    {
        var r = new RingBuffer<int>(3);
        r.Add(1);
        r.Add(2);
        r.Add(3);

        r.Clear();

        r.Count.Be(0);
        r.Index.Be(0);
        r.Get().IsNotFound().BeTrue();
    }

    [Fact]
    public void Clear_ShouldAllowReuse()
    {
        var r = new RingBuffer<int>(2);
        r.Add(1);
        r.Add(2);
        r.Clear();

        r.Add(9);
        r.Add(10);

        r.ToArray().Be(new[] { 9, 10 });
        r.Get().Return().Be(10);
        r.Count.Be(2);
        r.Index.Be(0);
    }

    [Fact]
    public void SingleItemBuffer_ShouldAlwaysKeepLatestValue()
    {
        var r = new RingBuffer<int>(1);
        r.Add(7);
        r.ToArray().Be(new[] { 7 });
        r.Get().Return().Be(7);

        r.Add(8);
        r.Count.Be(1);
        r.Index.Be(0);
        r.ToArray().Be(new[] { 8 });
        r.Get().Return().Be(8);
    }
}
