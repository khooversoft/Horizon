namespace Toolbox.Tools;

/// <summary>
/// Thread-safe editing "draft" over an immutable value. Holds an original snapshot and a working
/// current value, supports functional mutation via <see cref="Apply"/>, and a bounded undo history.
/// </summary>
public sealed class Draft<T>
{
    private const int MaxUndo = 100;

    private readonly Lock _lock = new();
    private readonly LinkedList<T> _undo = new();
    private readonly IEqualityComparer<T> _comparer;
    private T _original = default!;
    private T _current = default!;

    public Draft(IEqualityComparer<T>? comparer = null) => _comparer = comparer ?? EqualityComparer<T>.Default;

    /// <summary>Raised (outside the lock) with the new current value whenever it changes.</summary>
    public event Action<T>? Changed;

    public T Original
    {
        get { lock (_lock) return _original; }
    }

    public T Current
    {
        get { lock (_lock) return _current; }
    }

    public bool IsModified
    {
        get { lock (_lock) return !_comparer.Equals(_current, _original); }
    }

    public bool CanUndo
    {
        get { lock (_lock) return _undo.Count > 0; }
    }

    /// <summary>Loads a new original value as the current value and clears the undo history.</summary>
    public void Set(T value)
    {
        lock (_lock)
        {
            _original = value;
            _current = value;
            _undo.Clear();
        }

        Changed?.Invoke(value);
    }

    /// <summary>Restores the current value to the original and clears the undo history.</summary>
    public void Reset()
    {
        T value;
        lock (_lock)
        {
            _current = _original;
            _undo.Clear();
            value = _current;
        }

        Changed?.Invoke(value);
    }

    /// <summary>Applies a pure transformation, pushing the previous value onto the bounded undo history.</summary>
    public void Apply(Func<T, T> mutate)
    {
        ArgumentNullException.ThrowIfNull(mutate);

        T value;
        bool changed;
        lock (_lock)
        {
            T next = mutate(_current);
            changed = !_comparer.Equals(next, _current);
            if (changed)
            {
                _undo.AddLast(_current);
                if (_undo.Count > MaxUndo) _undo.RemoveFirst();
                _current = next;
            }

            value = _current;
        }

        if (changed) Changed?.Invoke(value);
    }

    /// <summary>Reverts the current value to the most recent undo snapshot, if any.</summary>
    public void Undo()
    {
        T value;
        lock (_lock)
        {
            if (_undo.Count == 0) return;

            _current = _undo.Last!.Value;
            _undo.RemoveLast();
            value = _current;
        }

        Changed?.Invoke(value);
    }
}
