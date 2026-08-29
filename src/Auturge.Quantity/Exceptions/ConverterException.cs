namespace Auturge.Quantity.Exceptions;

public class ConverterException(string message, Exception? innerException = null) : Exception(message, innerException);

public class ConverterNotFoundException(Unit source, Unit target, Exception? innerException = null) :
    ConverterException(string.Format(RS.EX_ConverterNotFound, source.DisplayName, target.DisplayName),
        innerException)
{
    public Unit SourceUnit { get; } = source;
    public Unit TargetUnit { get; } = target;
}
