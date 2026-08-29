namespace Auturge.Result;

public static class ExceptionExtensions
{
    public static List<Exception> ToList(this Exception ex) => [ex];
}
