
namespace Auturge.Validation.Exceptions;

public class TypeMismatchException : Exception
{
    public TypeMismatchException() : this(RS.EX_TypeMismatch)
    {
    }

    public TypeMismatchException(string message) : base(message)
    {
    }

    public TypeMismatchException(string message, Exception inner) : base(message, inner)
    {
    }
}
