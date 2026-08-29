namespace Auturge.Validation;

/// <summary>
/// An individual component within a rule.
/// <para></para>
/// In a rule definition such as <c>AddRuleFor(x => x.Name).NotNull().NotEqual("Foo")</c>
/// both <c>NotNull</c> and <c>NotEqual</c> are rule components.
/// </summary>
public interface IRuleComponent
{
    /// <summary> Whether this validator has a condition associated with it. </summary>
    bool HasCondition { get; }

    /// <summary> The validator associated with this component. </summary>
    IPropertyValidator Validator { get; }

    /// <summary> Retrieves the error code. </summary>
    string? ErrorCode { get; }
}

/// <inheritdoc/>
public interface IRuleComponent<TObj, out TProperty> : IRuleComponent
{
    /// <summary>
    /// Adds a condition for this validator. If there's already a condition, they're combined with an AND.
    /// </summary>
    /// <param name="condition"></param>
    void ApplyCondition(Func<ValidationContext<TObj>, bool> condition);

    /// <summary>
    /// Sets the overridden error message template for this validator.
    /// </summary>
    /// <param name="errorFactory">A function for retrieving the error message template.</param>
    void SetErrorMessage(Func<ValidationContext<TObj>, TProperty?, string> errorFactory);

    /// <summary>
    /// Sets the overridden error message template for this validator.
    /// </summary>
    /// <param name="errorMessage">The error message to set</param>
    void SetErrorMessage(string errorMessage);
    
    /// <summary>
    /// Function used to retrieve the severity for the validator
    /// </summary>
    Func<ValidationContext<TObj>, TProperty?, Severity>? SeverityProvider { set; }
}

/// <inheritdoc/>
public class RuleComponent<TObj, TProperty> : IRuleComponent<TObj, TProperty>
{
    private string? _errorMessage;
    private Func<ValidationContext<TObj>, TProperty?, string>? _errorMessageFactory;
    private Func<ValidationContext<TObj>, bool>? _condition;
    private readonly IPropertyValidator<TObj, TProperty?> _propertyValidator;

    internal RuleComponent(IPropertyValidator<TObj, TProperty?> propertyValidator)
    {
        _propertyValidator = propertyValidator;
    }

    /// <inheritdoc/>
    public string? ErrorCode { get; internal set; }

    /// <inheritdoc />
    public bool HasCondition => _condition != null;

    /// <inheritdoc/>
    public Func<ValidationContext<TObj>, TProperty?, Severity>? SeverityProvider { get; set; }
    
    /// <inheritdoc />
    public virtual IPropertyValidator Validator => _propertyValidator;

    /// <inheritdoc />
    public void ApplyCondition(Func<ValidationContext<TObj>, bool> condition)
    {
        if (_condition == null)
        {
            _condition = condition;
        }
        else
        {
            Func<ValidationContext<TObj>, bool> original = _condition;
            _condition = ctx => condition(ctx) && original(ctx);
        }
    }

    /// <summary>
    /// Gets the error message. If a context is supplied, it will be used to format the message if it has placeholders.
    /// If no context is supplied, the raw unformatted message will be returned, containing placeholders.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="value">The current property value.</param>
    /// <returns>Either the formatted or unformatted error message.</returns>
    public string GetErrorMessage(ValidationContext<TObj> context, TProperty? value)
    {
        // Use a custom message if one has been specified.
        // If no custom message has been supplied, use the default.
        string rawTemplate = (_errorMessageFactory?.Invoke(context, value) ?? _errorMessage)
                              ?? Validator.GetDefaultMessageTemplate(ErrorCode);

        return context.MessageFormatter.BuildMessage(rawTemplate);
    }

    /// <summary>
    /// Sets the overridden error message template for this validator.
    /// </summary>
    /// <param name="errorFactory">A function for retrieving the error message template.</param>
    public void SetErrorMessage(Func<ValidationContext<TObj>, TProperty?, string> errorFactory)
    {
        _errorMessageFactory = errorFactory;
        _errorMessage = null;
    }

    /// <summary>
    /// Sets the overridden error message template for this validator.
    /// </summary>
    /// <param name="errorMessage">The error message to set</param>
    public void SetErrorMessage(string errorMessage)
    {
        _errorMessage = errorMessage;
        _errorMessageFactory = null;
    }

    internal bool Validate(ValidationContext<TObj> context, TProperty? value) => InvokePropertyValidator(context, value);

    internal bool InvokeCondition(ValidationContext<TObj> context)
        => _condition == null || _condition(context);

    private protected virtual bool InvokePropertyValidator(ValidationContext<TObj> context, TProperty? value)
        => _propertyValidator.IsValid(context, value);
}
