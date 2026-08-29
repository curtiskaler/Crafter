// ReSharper disable InconsistentNaming

using System.Diagnostics;

namespace Auturge.Common.Logging;

public interface ILogEntry
{
    public const string CATEGORY_DEFAULT = "Default";

    DateTime TimeStamp { get; }
    LogLevel LogLevel { get; }
    string Category { get; }

    string? Message { get; }
    object? Data { get; }
    List<Exception>? Exceptions { get; }
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct LogEntry : ILogEntry
{
    private string DebuggerDisplay => $"{LogLevel.ToString().ToUpperInvariant()}: {Message}";

    /// <summary>
    /// Initializes an instance of the LogEntry struct for an event in the 'Default' category happening right meow.
    /// </summary>
    /// <param name="message">A user-friendly message to display.</param>
    /// <param name="logLevel">The log level.</param>
    /// <param name="data">(optional) The data for the event.</param>
    /// <param name="exception">(optional) The exception.</param>
    public LogEntry(LogLevel logLevel, string message, object? data = null, Exception? exception = null)
        : this(logLevel, message, data, exception != null ? [exception] : null)
    {
    }

    /// <summary>
    /// Initializes an instance of the LogEntry struct for an event in the 'Default' category happening right meow.
    /// </summary>
    /// <param name="message">A user-friendly message to display.</param>
    /// <param name="logLevel">The log level.</param>
    /// <param name="data">(optional) The data for the event.</param>
    /// <param name="exceptions">(optional) The exceptions.</param>
    public LogEntry(LogLevel logLevel, string message, object? data = null, List<Exception>? exceptions = null)
        : this(ILogEntry.CATEGORY_DEFAULT, logLevel, message, data, exceptions)
    {
    }

    /// <summary>
    /// Initializes an instance of the LogEntry struct.
    /// </summary>
    /// <param name="category">The category name for the log.</param>
    /// <param name="message">A user-friendly message to display.</param>
    /// <param name="logLevel">The log level.</param>
    /// <param name="data">(optional) The data for the event.</param>
    /// <param name="exception">The log exception.</param>
    public LogEntry(string category, LogLevel logLevel, string message, object? data = null,
        Exception? exception = null)
        : this(category, logLevel, message, null, data, exception != null ? [exception] : null)
    {
    }

    public LogEntry(string category, LogLevel logLevel, string message, object? data = null,
        List<Exception>? exceptions = null)
        : this(category, logLevel, message, null, data, exceptions)
    {
    }

    /// <summary>
    /// Initializes an instance of the LogEntry struct.
    /// </summary>
    /// <param name="category">The category name for the log.</param>
    /// <param name="message">A user-friendly message to display.</param>
    /// <param name="logLevel">The log level.</param>
    /// <param name="timeStamp">The date and time at which the event occurred.</param>
    /// <param name="data">(optional) The data for the event.</param>
    /// <param name="exceptions">The log exceptions.</param>
    public LogEntry(string category, LogLevel logLevel, string message,
        DateTime? timeStamp = null, object? data = null, List<Exception>? exceptions = null)
    {
        TimeStamp = timeStamp ?? DateTime.UtcNow;
        Message = message;
        LogLevel = logLevel;
        Category = category;
        Data = data;
        Exceptions = exceptions;
    }

    /// <summary> Gets the log timestamp. </summary>
    public DateTime TimeStamp { get; }

    /// <summary> Gets the log level. </summary>
    public LogLevel LogLevel { get; }

    /// <summary> Gets the log category. </summary>
    public string Category { get; }

    // ============== Content below

    /// <summary> Gets the log message. </summary>
    public string? Message { get; }

    /// <summary> Gets the log data. </summary>
    public object? Data { get; }

    /// <summary> Gets the log exceptions. </summary>
    public List<Exception>? Exceptions { get; }
}
