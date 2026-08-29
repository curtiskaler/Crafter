namespace Crafter.Model.Exceptions;

public class DeveloperException : Exception
{
    public DeveloperException(string message) : base(message)
    {
    }

    public DeveloperException(string message, Exception inner) : base(message, inner)
    {
    }
}
