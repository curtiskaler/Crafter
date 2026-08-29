namespace Auturge.Validation;

public interface IValidationContext
{
    /// <summary>
    /// Object being validated
    /// </summary>
    object? InstanceToValidate { get; }

    /// <summary>
    /// Parent validation context.
    /// </summary>
    IValidationContext? ParentContext { get; }

    /// <summary>
    /// A chain of nested properties.
    /// </summary>
    PropertyChain PropertyChain { get; }

    /// <summary>
    /// Determines whether a rule should execute.
    /// </summary>
    IValidatorSelector Selector { get; }

    /// <summary>
    /// Whether the validator should throw an exception if validation fails.
    /// The default is false.
    /// </summary>
    bool ThrowOnFailures { get; }
}

internal interface IMayHaveFailures
{
    List<ValidationFailure> Failures { get; }
}

public class ValidationContext<T> : IValidationContext, IMayHaveFailures
{
    private Func<ValidationContext<T>, string?>? _displayNameFunc;
    private IValidationContext? _parentContext;
    private readonly Stack<(bool IsChildContext, bool IsChildCollectionContext, IValidationContext ParentContext, PropertyChain
        Chain, Dictionary<string, Dictionary<T, bool>> SharedConditionCache)> _state = new();
    
    internal string? RawPropertyName { get; private set; }
    internal List<ValidationFailure> Failures { get; }

    /// <summary> Gets the display name for the current property being validated. </summary>
    public string DisplayName => _displayNameFunc?.Invoke(this) ?? string.Empty;

    /// <inheritdoc/>
    List<ValidationFailure> IMayHaveFailures.Failures => Failures;

    /// <summary> The object to validate. </summary>
    public T? InstanceToValidate { get; }

    /// <summary>
    /// Object being validated
    /// </summary>
    object? IValidationContext.InstanceToValidate => InstanceToValidate;


    /// <summary>
    /// Whether this is a child context
    /// </summary>
    public virtual bool IsChildContext { get; internal set; }

    /// <summary>
    /// Whether this is a child collection context.
    /// </summary>
    public virtual bool IsChildCollectionContext { get; internal set; }

    /// <summary>
    /// The message formatter used to construct error messages.
    /// </summary>
    public MessageFormatter MessageFormatter { get; }

    // This is the root context so it doesn't have a parent.
    // Explicit implementation so it's not exposed necessarily.
    IValidationContext? IValidationContext.ParentContext => _parentContext;

    /// <summary>
    /// Property chain
    /// </summary>
    public PropertyChain PropertyChain { get; private set; }

    /// <summary>
    /// The full path of the current property being validated.
    /// If accessed inside a child validator, this will include the parent's path too.
    /// </summary>
    public string? PropertyPath { get; private set; }

    /// <summary>
    /// Selector
    /// </summary>
    public IValidatorSelector Selector { get; }

    /// <summary>
    /// Shared condition results cache.
    /// The key of the outer dictionary is the ID of the condition, and its value is the cache for that condition.
    /// The key of the inner dictionary is the instance being validated, and the value is the condition result.
    /// </summary>
    private Dictionary<string, Dictionary<T, bool>> SharedConditionCache { get; set; } = new();

    /// <summary>
    /// Whether the root validator should throw an exception when validation fails.
    /// Defaults to false.
    /// </summary>
    public bool ThrowOnFailures { get; internal set; }

    public ValidationContext(T? instanceToValidate)
        : this(instanceToValidate, null, new DefaultValidatorSelector())
    {
    }

    public ValidationContext(T? instanceToValidate, PropertyChain? propertyChain, IValidatorSelector validatorSelector)
        : this(instanceToValidate, propertyChain, validatorSelector, [], new MessageFormatter())
    {
    }

    internal ValidationContext(T? instanceToValidate, PropertyChain? propertyChain,
        IValidatorSelector validatorSelector,
        List<ValidationFailure> failures, MessageFormatter messageFormatter)
    {
        PropertyChain = new PropertyChain(propertyChain);
        InstanceToValidate = instanceToValidate;
        Selector = validatorSelector;
        Failures = failures;
        MessageFormatter = messageFormatter;
    }

    /// <summary>
    /// Adds a new validation failure.
    /// </summary>
    /// <param name="failure">The failure to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddFailure(ValidationFailure failure)
    {
        if (failure == null)
            throw new ArgumentNullException(nameof(failure), RS.EX_A_failure_must_be_specified_when_calling_AddFailure);
        Failures.Add(failure);
    }

    /// <summary>
    /// Adds a new validation failure for the specified message.
    /// The failure will be associated with the current property being validated.
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    public void AddFailure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        ArgumentException.ThrowIfNullOrEmpty(PropertyPath);
        errorMessage = MessageFormatter.BuildMessage(errorMessage);
        AddFailure(new ValidationFailure(PropertyPath, errorMessage));
    }

    /// <summary>
    /// Adds a new validation failure for the specified property.
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <param name="errorMessage">The error message</param>
    public void AddFailure(string? propertyName, string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        errorMessage = MessageFormatter.BuildMessage(errorMessage);
        AddFailure(new ValidationFailure(PropertyChain.BuildPropertyPath(propertyName ?? string.Empty), errorMessage));
    }

    /// <summary>
    /// Gets or creates generic validation context from non-generic validation context.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static ValidationContext<T> GetFromNonGenericContext(IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Already of the correct type.
        if (context is ValidationContext<T> c)
        {
            return c;
        }

        // Parameters match
        if (context.InstanceToValidate is T instanceToValidate)
        {
            List<ValidationFailure> failures = (context is IMayHaveFailures f) ? f.Failures : [];

            return new ValidationContext<T>(instanceToValidate, context.PropertyChain, context.Selector, failures,
                Defaults.MessageFormatter)
            {
                // IsChildContext = context.IsChildContext,
                // RootContextData = context.RootContextData,
                ThrowOnFailures = context.ThrowOnFailures,
                _parentContext = context.ParentContext,
            };
        }

        if (context.InstanceToValidate == null)
        {
            var failures = (context is IMayHaveFailures f) ? f.Failures : new List<ValidationFailure>();

            return new ValidationContext<T>(default, context.PropertyChain, context.Selector, failures,
                Defaults.MessageFormatter)
            {
                // IsChildContext = context.IsChildContext,
                // RootContextData = context.RootContextData,
                ThrowOnFailures = context.ThrowOnFailures,
                _parentContext = context.ParentContext,
            };
        }

        throw new InvalidOperationException(
            $"Cannot validate instances of type '{context.InstanceToValidate.GetType().Name}'. This validator can only validate instances of type '{typeof(T).Name}'.");
    }


    internal void InitializeForPropertyValidator(string propertyPath,
        Func<ValidationContext<T>, string?>? displayNameFunc, string? propertyName)
    {
        PropertyPath = propertyPath;
        _displayNameFunc = displayNameFunc;
        RawPropertyName = propertyName;
    }

    internal void PrepareForChildCollectionValidator()
    {
        _state.Push((IsChildContext, IsChildCollectionContext, _parentContext!, PropertyChain, SharedConditionCache));
        IsChildContext = true;
        IsChildCollectionContext = true;
        PropertyChain = new PropertyChain();
    }

    internal void RestoreState()
    {
        var state = _state.Pop();
        IsChildContext = state.IsChildContext;
        IsChildCollectionContext = state.IsChildCollectionContext;
        _parentContext = state.ParentContext;
        PropertyChain = state.Chain;
        SharedConditionCache = state.SharedConditionCache;
    }
}
