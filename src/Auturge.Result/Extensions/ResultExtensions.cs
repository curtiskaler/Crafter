namespace Auturge.Result;

public static class ResultExtensions
{
    public static bool IsFailure(this IResult result) =>
        result == null ? throw new ArgumentNullException(nameof(result)) : result.Code.Equals(ResultCode.FAILURE);

    public static bool IsSuccess(this IResult result)
        => result == null ? throw new ArgumentNullException(nameof(result)) : result.Code.Equals(ResultCode.SUCCESS);

    public static bool IsSkipped(this IResult result)
        => result == null ? throw new ArgumentNullException(nameof(result)) : result.Code.Equals(ResultCode.SKIP);

    public static List<Failure> GetFailures(this IEnumerable<IResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results
            .Where(it => it.Code.Equals(ResultCode.FAILURE))
            .Select(it => (it as Failure)!)
            .ToList();
    }

    public static Exception? AggregateExceptions(this IEnumerable<IResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        List<Failure> failures = results.GetFailures();
        var exceptions = failures.SelectMany(it => it.Exceptions).ToList();
        return exceptions.Count switch
        {
            0 => null,
            1 => exceptions.First(),
            _ => new AggregateException(exceptions)
        };
    }

    public static Exception? AggregateExceptions(this IResult result)
        => new List<IResult> { result }.AggregateExceptions();
}
