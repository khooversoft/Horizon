using Toolbox.Tools;

namespace Toolbox.test.Tools;

public class DraftTests
{
    private record Person
    {
        public string Name { get; init; } = string.Empty;
        public int Age { get; init; }
    }

    [Fact]
    public void NewDraftHasNoStateOrHistory()
    {
        var draft = new Draft<string>();

        draft.Original.BeNull();
        draft.Current.BeNull();
        draft.IsModified.BeFalse();
        draft.CanUndo.BeFalse();
    }

    [Fact]
    public void SetAssignsOriginalAndCurrent()
    {
        var draft = new Draft<string>();

        draft.Set("hello");

        draft.Original.Be("hello");
        draft.Current.Be("hello");
        draft.IsModified.BeFalse();
        draft.CanUndo.BeFalse();
    }

    [Fact]
    public void SetRaisesChangedWithValue()
    {
        var draft = new Draft<string>();
        var events = new List<string>();
        draft.Changed += v => events.Add(v);

        draft.Set("hello");

        events.Count.Be(1);
        events[0].Be("hello");
    }

    [Fact]
    public void SetClearsUndoHistory()
    {
        var draft = new Draft<int>();
        draft.Set(1);
        draft.Apply(x => x + 1);
        draft.CanUndo.BeTrue();

        draft.Set(10);

        draft.CanUndo.BeFalse();
        draft.IsModified.BeFalse();
        draft.Current.Be(10);
        draft.Original.Be(10);
    }

    [Fact]
    public void ApplyChangesCurrentAndMarksModified()
    {
        var draft = new Draft<int>();
        draft.Set(1);

        draft.Apply(x => x + 5);

        draft.Current.Be(6);
        draft.Original.Be(1);
        draft.IsModified.BeTrue();
        draft.CanUndo.BeTrue();
    }

    [Fact]
    public void ApplyRaisesChangedWithNewValue()
    {
        var draft = new Draft<int>();
        draft.Set(1);
        var events = new List<int>();
        draft.Changed += v => events.Add(v);

        draft.Apply(x => x + 1);
        draft.Apply(x => x + 1);

        events.Count.Be(2);
        events[0].Be(2);
        events[1].Be(3);
    }

    [Fact]
    public void ApplyWithNoEffectiveChangeIsIgnored()
    {
        var draft = new Draft<int>();
        draft.Set(1);
        var events = new List<int>();
        draft.Changed += v => events.Add(v);

        draft.Apply(x => x);

        events.Count.Be(0);
        draft.IsModified.BeFalse();
        draft.CanUndo.BeFalse();
        draft.Current.Be(1);
    }

    [Fact]
    public void ApplyEqualByValueRecordIsIgnored()
    {
        var draft = new Draft<Person>();
        draft.Set(new Person { Name = "Sam", Age = 30 });
        var events = new List<Person>();
        draft.Changed += v => events.Add(v);

        draft.Apply(p => p with { Age = 30 });

        events.Count.Be(0);
        draft.IsModified.BeFalse();
        draft.CanUndo.BeFalse();
    }

    [Fact]
    public void ApplyNullMutateThrows()
    {
        var draft = new Draft<int>();
        draft.Set(1);

        Verify.Throws<ArgumentNullException>(() => draft.Apply(null!));
    }

    [Fact]
    public void UndoRevertsToPreviousValue()
    {
        var draft = new Draft<int>();
        draft.Set(1);
        draft.Apply(x => x + 1);
        draft.Apply(x => x + 1);
        draft.Current.Be(3);

        draft.Undo();

        draft.Current.Be(2);
        draft.CanUndo.BeTrue();
        draft.IsModified.BeTrue();
    }

    [Fact]
    public void UndoAllReturnsToOriginalAndClearsModified()
    {
        var draft = new Draft<int>();
        draft.Set(1);
        draft.Apply(x => x + 1);
        draft.Apply(x => x + 1);

        draft.Undo();
        draft.Undo();

        draft.Current.Be(1);
        draft.IsModified.BeFalse();
        draft.CanUndo.BeFalse();
    }

    [Fact]
    public void UndoRaisesChangedWithRestoredValue()
    {
        var draft = new Draft<int>();
        draft.Set(1);
        draft.Apply(x => x + 1);
        var events = new List<int>();
        draft.Changed += v => events.Add(v);

        draft.Undo();

        events.Count.Be(1);
        events[0].Be(1);
    }

    [Fact]
    public void UndoWithEmptyHistoryIsNoOp()
    {
        var draft = new Draft<int>();
        draft.Set(5);
        var events = new List<int>();
        draft.Changed += v => events.Add(v);

        draft.Undo();

        events.Count.Be(0);
        draft.Current.Be(5);
        draft.CanUndo.BeFalse();
    }

    [Fact]
    public void ResetRestoresOriginalAndClearsHistory()
    {
        var draft = new Draft<int>();
        draft.Set(1);
        draft.Apply(x => x + 1);
        draft.Apply(x => x + 1);

        draft.Reset();

        draft.Current.Be(1);
        draft.Original.Be(1);
        draft.IsModified.BeFalse();
        draft.CanUndo.BeFalse();
    }

    [Fact]
    public void ResetRaisesChangedWithOriginalValue()
    {
        var draft = new Draft<int>();
        draft.Set(1);
        draft.Apply(x => x + 9);
        var events = new List<int>();
        draft.Changed += v => events.Add(v);

        draft.Reset();

        events.Count.Be(1);
        events[0].Be(1);
    }

    [Fact]
    public void UndoHistoryIsCappedAtOneHundred()
    {
        var draft = new Draft<int>();
        draft.Set(0);

        for (int i = 0; i < 150; i++)
        {
            draft.Apply(x => x + 1);
        }

        draft.Current.Be(150);

        int undoCount = 0;
        while (draft.CanUndo)
        {
            draft.Undo();
            undoCount++;
        }

        undoCount.Be(100);
        draft.Current.Be(50);
    }

    [Fact]
    public void CustomComparerControlsChangeDetection()
    {
        var draft = new Draft<string>(StringComparer.OrdinalIgnoreCase);
        draft.Set("hello");
        var events = new List<string>();
        draft.Changed += v => events.Add(v);

        draft.Apply(_ => "HELLO");

        events.Count.Be(0);
        draft.IsModified.BeFalse();
        draft.CanUndo.BeFalse();

        draft.Apply(_ => "world");

        events.Count.Be(1);
        draft.IsModified.BeTrue();
    }
}
