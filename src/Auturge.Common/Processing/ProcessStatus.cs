// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Diagnostics;
using Auturge.Common.Logging;

namespace Auturge.Common.Processing;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ProcessStatus
{
    private string DebuggerDisplay => $"{CurrentState.ToString()}";

    /// <summary>
    /// The state of the process.
    /// </summary>
    public LifecycleState CurrentState { get; private set; } = LifecycleState.NotStarted;

    /// <summary>
    /// The integer code representing the final state of the process. 
    /// </summary>
    public int StateCode
    {
        get
        {
            return CurrentState switch
            {
                LifecycleState.Completed => (int)StatusCode.Completed,
                LifecycleState.NotStarted or LifecycleState.Failed or LifecycleState.InProgress =>
                    // TODO: figure out from the exception which code to return
                    (int)StatusCode.FuckeryAbounds,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    /// <summary>
    /// The timestamp at which the process started.
    /// </summary>
    public DateTime StartTime { get; private set; }

    /// <summary>
    /// The timestamp at which the process completed.
    /// </summary>
    public DateTime? CompletionTime { get; private set; }

    /// <summary>
    /// A list of messages (e.g., error details, status message) 
    /// </summary>
    public List<ILogEntry> Messages { get; } = [];

    /// <summary>
    /// The messages representing errors.
    /// </summary>
    public List<ILogEntry> Errors => Messages
        .Where(it => it.Exceptions != null || it.LogLevel == LogLevel.Error || it.LogLevel == LogLevel.Fatal)
        .ToList();

    /// <summary>
    /// The messages from the error entries.
    /// </summary>
    public List<string> ErrorMessages => Errors.Select(e => e.Message).ToList()!;

    /// <summary>
    /// An optional property to hold an exception if one occurred
    /// </summary>
    public List<Exception> Exceptions => Errors
        .Where(it => it.Exceptions != null)
        .SelectMany(it => it.Exceptions!)
        .ToList();

    /// <summary>
    /// An optional property to hold how much of the process is complete.
    /// </summary>
    public double ProgressPercentage { get; private set; } // 0.0 to 100.0

    public void Mark(LifecycleState state, string message, List<Exception>? exception = null, LogLevel? level = null)
    {
        CurrentState = state;
        var levelToUse = level ?? (exception != null ? LogLevel.Error : LogLevel.Info);
        AddMessage(state, levelToUse, message, exception);
    }

    private void AddMessage(LifecycleState state, LogLevel level, string message, Exception? exception = null)
    {
        var msg = new LogEntry(level, message, state, exception);
        Messages.Add(msg);
    }

    private void AddMessage(LifecycleState state, LogLevel level, string message, List<Exception>? exceptions = null)
    {
        var msg = new LogEntry(level, message, state, exceptions);
        Messages.Add(msg);
    }

    public void MarkStarted(string fmtString = "Process started at {0}.")
    {
        StartTime = DateTime.UtcNow;
        Mark(LifecycleState.InProgress, $"Process started at {StartTime}.", null, LogLevel.Debug);
    }

    public void MarkCompleted()
    {
        CompletionTime = DateTime.UtcNow;
        ProgressPercentage = 100.0;
        Mark(LifecycleState.Completed, $"Process completed at {CompletionTime}.", null, LogLevel.Debug);
    }

    public void MarkInitializing()
        => Mark(LifecycleState.Initializing, $"Initialization started at {DateTime.UtcNow}.");

    public void MarkInitialized()
        => Mark(LifecycleState.Initialized, $"Initialization completed at {DateTime.UtcNow}.");

    public void MarkFailed(ProcessStatus failure) => MarkFailed(failure.Exceptions);
    public void MarkFailed(List<Exception> exceptions) => MarkFailed(exceptions.First());
    public void MarkFailed(Exception exception) => MarkFailed(exception.Message, [exception]);

    public void MarkFailed(string error, List<Exception>? exceptions = null, LogLevel? level = LogLevel.Error)
    {
        CompletionTime = DateTime.UtcNow;
        Mark(LifecycleState.Failed, error, exceptions, level);
    }
}
