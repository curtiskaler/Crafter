using System.Diagnostics;
using Auturge.Results;

namespace Auturge.Result;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Skipped : IResult
{
    // Evaluated only when inspected during a debugging session
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{Code.ToString()}: [{Objective}]: {Reason}";

    public ResultCode Code => ResultCode.SKIP;
    public string Objective { get; }
    public string Reason { get; }
    public List<Exception> Exceptions { get; }
    
    public Skipped(string objective, Exception ex) : this(objective, null, ex.ToList())
    {
    }

    public Skipped(string objective, IEnumerable<Exception> exceptions) : this(objective, null, exceptions)
    {
    }

    public Skipped(string objective, string reason, Exception ex) : this(objective, reason, ex.ToList())
    {
    }
    
    public Skipped(string objective, string? reason, IEnumerable<Exception> exceptions)
    {
        ArgumentNullException.ThrowIfNull(objective);

        var exceptionsList = exceptions.ToList();
        if (reason == null && exceptionsList.Count == 0)
        {
            throw new ArgumentException(ResultStrings.ERROR_CannotCreateSkippedWithoutReasonOrException);
        }

        Objective = objective ?? throw new ArgumentNullException(nameof(objective));
        Exceptions = exceptionsList;
        Reason = reason ?? exceptionsList.First().Message;
    }

    public static implicit operator bool(Skipped s) => true;
}

public class Skipped<TOut> : Skipped, ISkipped<TOut>
{
    public TOut? Output { get; }

    public Skipped(string objective, string reason) : this(objective, default, reason, new List<Exception>())
    {
    }

    public Skipped(string objective, string reason, Exception ex) : this(objective, default, reason, ex.ToList())
    {
    }

    public Skipped(string objective, string reason, IEnumerable<Exception> exeptions) : this(objective, default, reason,
        exeptions)
    {
    }

    public Skipped(string objective, TOut? output, string reason) : this(objective, output, reason,
        new List<Exception>())
    {
    }

    public Skipped(string objective, TOut? output, string reason, Exception ex) : this(objective, output, reason,
        ex.ToList())
    {
    }

    public Skipped(string objective, TOut? output, string? reason, IEnumerable<Exception> exceptions) : base(objective,
        reason, exceptions)
    {
        Output = output ?? default;
    }

    public static implicit operator bool(Skipped<TOut> result)
        => result == null ? throw new ArgumentNullException(nameof(result)) : true;
}
