namespace Auturge.Validation;

/// <summary>
/// Defines a validation failure.
/// </summary>
public class ValidationFailure(string propertyName, string errorMessage)
{
    /// <summary>
    /// The error message
    /// </summary>
    public string ErrorMessage { get; set; } = errorMessage;

    /// <summary>
    /// The name of the property
    /// </summary>
    public string PropertyName { get; set; } = propertyName;

    /// <summary>
    /// Custom severity level associated with the failure.
    /// </summary>
    public Severity Severity { get; set; } = Severity.Error;
    
    public string? ErrorCode { get; set; } = null;

    public ValidationFailure() : this(string.Empty, string.Empty)
    {
    }

    public override string ToString() => ErrorMessage;
}

/// <summary>
/// Defines a validation failure that includes the invalid value.
/// </summary>
public class ValidationFailure<T>(string propertyName, string errorMessage, T? badValue)
    : ValidationFailure(propertyName, errorMessage)
{
    /// <summary>
    /// The property value that caused the failure.
    /// </summary>
    public T? BadValue { get; set; } = badValue;
}
