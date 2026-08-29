namespace Auturge.Validation;

/// <summary>
/// A custom property validator.
/// This interface should not be implemented directly in your code as it is subject to change.
/// </summary>
public interface IPropertyValidator
{
    /// <summary>
    /// The name of the validator. This is usually the type name without any generic parameters.
    /// This is used as the default Error Code for the validator.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Returns the default error message template for this validator, when not overridden.
    /// </summary>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    string GetDefaultMessageTemplate(string? errorCode);
}

/// <inheritdoc/>
public interface IPropertyValidator<TObj, in TProperty> : IPropertyValidator
{
    /// <summary>
    /// Validates a specific property value.
    /// </summary>
    /// <param name="context">The validation context. The parent object can be obtained from here.</param>
    /// <param name="value">The current property value to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    bool IsValid(ValidationContext<TObj> context, TProperty? value);
}
