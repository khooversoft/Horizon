using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools;

public static class VerifyLong
{
    [DebuggerStepThrough]
    public static long Be(this long subject, long value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static long? Be(this long? subject, long? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject != value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static long NotBe(this long subject, long value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should NOT be '{value}'", because));
        return subject;
    }

    [DebuggerStepThrough]
    public static long? NotBe(this long? subject, long? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
    {
        if (subject == value) throw new ArgumentException(Verify.FormatException($"Value is '{subject}' should NOT be '{value}'", because));
        return subject;
    }
}
