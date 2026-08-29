using System.Diagnostics;

namespace Auturge.Result;

public interface IResult
{
    /// <summary>
    /// A code representing the final state of the step or process.
    /// </summary>
    ResultCode Code { get; }
}

public interface IResult<out TOut> : IResult
{
    /// <summary>
    /// The resulting data from the step or process.
    /// </summary>
    TOut? Output { get; }
}

public interface IFailure : IResult
{
    /// <summary>
    /// A display-ready explanation of the problem. Required.
    /// </summary>
    string Reason { get; }

    /// <summary>
    /// A list of exceptions causing the failure.
    /// </summary>
    List<Exception> Exceptions { get; }
}

public interface IFailure<out TOut> : IResult<TOut>, IFailure;

public interface ISuccess : IResult;

public interface ISuccess<out TOut> : IResult<TOut>, ISuccess;

public interface ISkipped : IResult
{
    string Reason { get; }
}

public interface ISkipped<out TOut> : IResult<TOut>, ISkipped;
