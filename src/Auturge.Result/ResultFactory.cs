using System.Runtime.InteropServices;
// using Auturge.Processing;

// ReSharper disable InconsistentNaming

namespace Auturge.Result;

public static class ResultFactory
{
    public static Success SUCCESS(string objective, [Optional] string message)
        => new(objective, message);

    public static Success<T> SUCCESS<T>(string objective, T result, [Optional] string message)
        => new(objective, result, message);

    public static Failure FAILURE(string objective, string reason, Exception exception) 
        => new(objective, reason, exception);

    public static Failure FAILURE(string objective, string reason, IEnumerable<Exception> exceptions) 
        => new(objective, reason, exceptions);

    public static Failure<T> FAILURE<T>(string objective, string reason, Exception exception) 
        => new(objective, reason, exception);

    public static Failure<T> FAILURE<T>(string objective, string reason, IEnumerable<Exception> exceptions)
        => new(objective, reason, exceptions);

    
    public static Skipped SKIP(string objective, string reason) 
        => new(objective, reason, new List<Exception>());

    public static Skipped<T> SKIP<T>(string objective, string reason) 
        => new(objective, reason, new List<Exception>());
    
    public static Skipped SKIP(string objective, string reason, Exception exception) 
        => new(objective, reason, exception);

    public static Failure SKIP(string objective, string reason, IEnumerable<Exception> exceptions) 
        => new(objective, reason, exceptions);

    public static Skipped<T> SKIP<T>(string objective, string reason, Exception exception) 
        => new(objective, reason, exception);

    public static Skipped<T> SKIP<T>(string objective, string reason, IEnumerable<Exception> exceptions)
        => new(objective, reason, exceptions);
}
