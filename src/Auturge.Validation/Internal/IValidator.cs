namespace Auturge.Validation;

public interface IValidator
{
    /// <summary>
    /// Check to see if the validator can validate objects of the specified type.
    /// </summary>
    bool CanHandle(Type type);
    
    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="context">A ValidationContext</param>
    /// <returns>A ValidationResult object contains any validation failures.</returns>
    ValidationResult Validate(IValidationContext context);
}

public interface IValidator<in T> : IValidator
{
    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate</param>
    /// <returns>A ValidationResult object containing any validation failures.</returns>
    ValidationResult Validate(T instance);
    
    // /// <summary>
    // /// Validate the specified instance asynchronously
    // /// </summary>
    // /// <param name="instance">The instance to validate</param>
    // /// <param name="cancellation"></param>
    // /// <returns>A ValidationResult object containing any validation failures.</returns>
    // Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default);
}
