using System.Collections;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Ring buffer is a fixed-size data structure that overwrites the oldest data when new data is added
/// beyond its capacity. It is useful for scenarios where you want to maintain a history of the most
/// recent items, such as logging, streaming data, or buffering input/output operations.
/// 
/// Mininum size is 1
/// 
/// </summary>
public class RingBuffer<T> : IEnumerable<T>
{
    private readonly T[] _buffer;
    private int _count;

    public RingBuffer(int size)
    {
        size.Assert(x => x > 0, "Size must be greater than zero.");
        _buffer = new T[size];
    }

    public int Size => _buffer.Length;
    public int Index { get; private set; } = 0;

    public int Count => _count;

    public void Add(T value)
    {
        _buffer[Index] = value;

        Index++;
        if (Index == Size) Index = 0;
        if (_count < Size) _count++;
    }

    public Option<T> Get()
    {
        if (_count == 0) return StatusCode.NotFound;

        int lastIndex = Index - 1;
        if (lastIndex < 0) lastIndex = Size - 1;

        return _buffer[lastIndex];
    }

    public void Clear()
    {
        Array.Clear(_buffer);
        Index = 0;
        _count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (_count == 0) yield break;

        int start = (_count == Size) ? Index : 0;
        for (int i = 0; i < _count; i++)
        {
            int idx = start + i;
            if (idx >= Size) idx -= Size;

            yield return _buffer[idx];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
