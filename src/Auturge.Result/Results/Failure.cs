using System.Diagnostics;
using Auturge.Results;

namespace Auturge.Result;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Failure : IFailure
{
    // Evaluated only when inspected during a debugging session
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay =>
        string.IsNullOrWhiteSpace(Objective)
            ? $"{Code.ToString()}: {Reason}"
            : $"{Code.ToString()}: [{Objective}]: {Reason}";

    public string Objective { get; }
    public ResultCode Code => ResultCode.FAILURE;
    public string Reason { get; }
    public List<Exception> Exceptions { get; }

    public Failure(string objective, Exception ex) : this(objective, null, ex.ToList())
    {
    }

    public Failure(string objective, string? reason, Exception ex) : this(objective, reason, ex.ToList())
    {
    }

    public Failure(string objective, string? reason, IEnumerable<Exception> exceptions)
    {
        ArgumentNullException.ThrowIfNull(objective);

        var exceptionsList = exceptions.ToList();
        if (reason == null && exceptionsList.Count == 0)
        {
            throw new ArgumentException(ResultStrings.ERROR_CannotCreateFailureWithoutReasonOrException);
        }

        Objective = objective ?? throw new ArgumentNullException(nameof(objective));
        Exceptions = exceptionsList;
        Reason = reason ?? exceptionsList.First().Message;
    }

    public static implicit operator bool(Failure f) => false;
}

public class Failure<TOut> : Failure, IFailure<TOut>
{
    public TOut? Output { get; }

    public Failure(string objective, string reason, Exception ex) : this(objective, default, reason, ex.ToList())
    {
    }

    public Failure(string objective, string reason, IEnumerable<Exception> exeptions) : this(objective, default, reason,
        exeptions)
    {
    }

    public Failure(string objective, TOut? output, string reason, Exception ex) : this(objective, output, reason,
        ex.ToList())
    {
    }

    public Failure(string objective, TOut? output, string? reason, IEnumerable<Exception> exceptions) : base(objective,
        reason, exceptions)
    {
        Output = output ?? default;
    }

    public static implicit operator bool(Failure<TOut> result) 
        => result == null ? throw new ArgumentNullException(nameof(result)) : false;
}
