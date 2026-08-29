namespace Auturge.Validation.Exceptions;

public class ValidationException : Exception
{
    /// <summary>
    /// Validation errors
    /// </summary>
    public IEnumerable<ValidationFailure> Errors { get; private set; }

    /// <summary>
    /// Creates a new ValidationException
    /// </summary>
    /// <param name="message"></param>
    public ValidationException(string message) : this(message, [])
    {
    }

    /// <summary>
    /// Creates a new ValidationException
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errors"></param>
    public ValidationException(string message, IEnumerable<ValidationFailure> errors) : base(message)
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a new ValidationException
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errors"></param>
    /// <param name="appendDefaultMessage">appends default validation error message to message</param>
    public ValidationException(string message, IEnumerable<ValidationFailure> errors, bool appendDefaultMessage)
        : base(appendDefaultMessage ? $"{message} {BuildErrorMessage(errors)}" : message)
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a new ValidationException
    /// </summary>
    /// <param name="errors"></param>
    public ValidationException(IEnumerable<ValidationFailure> errors) : base(BuildErrorMessage(errors))
    {
        Errors = errors;
    }

    private static string BuildErrorMessage(IEnumerable<ValidationFailure> errors)
    {
        IEnumerable<string> arr = errors.Select(x =>
            $"{Environment.NewLine} -- {x.PropertyName}: {x.ErrorMessage}");
        return "Validation failed: " + string.Join(string.Empty, arr);
    }
}
