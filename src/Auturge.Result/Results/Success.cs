using System.Diagnostics;

// ReSharper disable MemberCanBePrivate.Global

namespace Auturge.Result;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Success(string objective, string message) : ISuccess
{
    public ResultCode Code => ResultCode.SUCCESS;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay =>
        string.IsNullOrWhiteSpace(Message) ? Code.ToString() : $"{Code.ToString()}: {Message}";

    public string Objective { get; } = objective ?? throw new ArgumentNullException(nameof(objective));
    public string Message { get; } = message;

    public Success(string objective) : this(objective, string.Empty)
    {
    }

    public static implicit operator bool(Success result)
        => result is null ? throw new ArgumentNullException(nameof(result)) : true;
}

public class Success<TOut>(string objective, TOut? output, string message) : Success(objective, message), ISuccess<TOut>
{
    public TOut? Output { get; } = output;

    public Success(string objective, TOut? output) : this(objective, output, string.Empty)
    {
    }

    public static implicit operator bool(Success<TOut> result)
        => result is null ? throw new ArgumentNullException(nameof(result)) : true;
}
