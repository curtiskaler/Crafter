namespace Auturge.Common.Logging;

/// <summary>
/// Defines logging severity levels.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Logs that contain the most detailed messages, for debugging details.
    /// These messages may contain sensitive application data.
    /// These messages are disabled by default and should never be enabled in a production environment.
    /// </summary>
    Trace = 0,
    
    /// <summary>
    /// Logs that are used for interactive investigation during development.
    /// These logs should primarily contain information useful for debugging and have no long-term value.
    /// </summary>
    Debug = 1,
    
    /// <summary>
    /// General application flow events.
    /// Logs that track the general flow of the application. These logs should have long-term value.
    /// </summary>
    Info, 
    
    /// <summary>
    /// Logs that highlight unexpected or abnormal events in the application flow,
    /// that do not cause the application execution to stop.
    /// </summary>
    Warn, 
    
    /// <summary>
    /// Failures or exceptions that prevent specific operations, not application-wide failures..
    /// </summary>
    Error,
    
    /// <summary>
    /// Unrecoverable application or system crashes requiring immediate attention.
    /// </summary>
    Fatal,
    
    /// <summary>
    /// Not used for writing log messages.
    /// Disables logging (possible for a given logging category).
    /// </summary>
    None,
}
