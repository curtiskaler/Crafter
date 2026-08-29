using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Auturge.Validation;

internal abstract class RuleBase<TObj, TProperty, TValue> : IValidationRule<TObj, TValue> where TObj : notnull
{
    private string? _displayName;
    private string? _propertyDisplayName;
    private string? _propertyName;
    private Func<FailureMode> _failureModeFn;
    private Func<ValidationContext<TObj>, string>? _displayNameFactory;
    protected readonly Func<ValidationContext<TObj>, string?>? _displayNameFunc;

    /// <summary> The components in this rule. </summary>
    public List<RuleComponent<TObj, TValue>> Components { get; } = [];

    /// <inheritdoc/>
    List<IRuleComponent> IValidationRule.Components => [.. Components.Select(IRuleComponent (x) => x)];

    /// <summary>
    /// Condition for all validators in this rule.
    /// </summary>
    internal Func<ValidationContext<TObj>, bool>? Condition { get; }

    /// <inheritdoc/>
    public IRuleComponent<TObj, TValue>? Current => Components.LastOrDefault();

    /// <summary> Dependent rules. </summary>
    internal List<IValidationRuleInternal<TObj>> DependentRules { get; private protected set; } = [];

    /// <inheritdoc/>
    List<IValidationRule> IValidationRule.DependentRules => [.. DependentRules.Select(IValidationRule (x) => x)];

    /// <inheritdoc/>
    public LambdaExpression Expression { get; }

    /// <inheritdoc/>
    public FailureMode FailureMode
    {
        get => _failureModeFn();
        set => _failureModeFn = () => value;
    }

    /// <inheritdoc />
    public bool HasCondition => Condition != null;

    /// <inheritdoc/>
    public MemberInfo Member { get; }

    /// <summary>
    /// Allows custom creation of an error message
    /// </summary>
    public Func<IMessageBuilderContext<TObj, TValue>, string>? MessageBuilder { get; set; }

    /// <summary>
    /// Function that can be invoked to retrieve the value of the property.
    /// </summary>
    public Func<TObj?, TProperty?> PropertyFunc { get; }

    // /// <summary>
    // /// Returns the property name for the property being validated.
    // /// Returns null if it is not a property being validated (eg a method call)
    // /// </summary>
    /// <inheritdoc/>
    public string? PropertyName
    {
        get { return _propertyName; }
        set
        {
            _propertyName = value;
            _propertyDisplayName = _propertyName?.SplitPascalCase();
        }
    }

    /// <inheritdoc/>
    public Type TypeToValidate { get; } //= typeof(TObj);


    public RuleBase(MemberInfo member, Func<TObj?, TProperty?> propertyFunc, LambdaExpression expression,
        Func<FailureMode> failureModeFunc, Type typeToValidate)
    {
        Member = member;
        PropertyFunc = propertyFunc;
        _failureModeFn = failureModeFunc;
        Expression = expression;
        TypeToValidate = typeToValidate;

        Type containerType = typeof(TObj);
        _propertyName = ValidatorOptions.PropertyNameResolver(containerType, member, expression);
        _displayNameFactory = _ => ValidatorOptions.DisplayNameResolver(containerType, member, expression);
        _displayNameFunc = GetDisplayName;
    }

    /// <inheritdoc/>
    public void AddValidator(IPropertyValidator<TObj, TValue?> validator)
    {
        var component = new RuleComponent<TObj, TValue>(validator);
        Components.Add(component);
    }

    /// <inheritdoc/>
    public void ApplyCondition(Func<ValidationContext<TObj>, bool> predicate,
        ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators)
    {
        if (applyConditionTo == ApplyConditionTo.AllValidators)
        {
            foreach (RuleComponent<TObj, TValue> validator in Components)
            {
                validator.ApplyCondition(predicate);
            }

            foreach (IValidationRuleInternal<TObj> dependentRule in DependentRules)
            {
                dependentRule.ApplyCondition(predicate, applyConditionTo);
            }
        }
        else
        {
            Current?.ApplyCondition(predicate);
        }
    }

    /// <inheritdoc/>
    public void Clear() => Components.Clear();

    /// <inheritdoc/>
    string? IValidationRule.GetDisplayName(IValidationContext context) =>
        GetDisplayName(ValidationContext<TObj>.GetFromNonGenericContext(context));

    /// <summary>
    /// Display name for the property.
    /// </summary>
    protected string? GetDisplayName(ValidationContext<TObj> context)
        => _displayNameFactory?.Invoke(context) ?? _displayName ?? _propertyDisplayName;


    /// <summary> Sets the display name for the property. </summary>
    /// <param name="name">The property's display name</param>
    public void SetDisplayName(string name)
    {
        _displayName = name;
        _displayNameFactory = null;
    }

    /// <summary> Sets the display name for the property using a function. </summary>
    /// <param name="factory">The function for building the display name</param>
    public void SetDisplayName(Func<ValidationContext<TObj>, string> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _displayNameFactory = factory;
        _displayName = null;
    }

    /// <inheritdoc/>
    object? IValidationRule<TObj>.GetPropertyValue(TObj instance) => PropertyFunc(instance);

    /// <inheritdoc/>
    bool IValidationRule<TObj>.TryGetPropertyValue<TProp>(TObj instance, [MaybeNullWhen(false)] out TProp value)
    {
        value = default;

        TProperty? result = PropertyFunc(instance);
        if (result is not TProp propertyValue) return false;

        value = propertyValue;
        return true;
    }

    /// <summary> Creates an error validation result for this validator. </summary>
    /// <param name="context">The validator context</param>
    /// <param name="value">The property value</param>
    /// <param name="component">The current rule component.</param>
    /// <returns>Returns an error validation result.</returns>
    protected ValidationFailure CreateValidationError(ValidationContext<TObj> context, TValue? value,
        RuleComponent<TObj, TValue> component)
    {
        string error = MessageBuilder != null
            ? MessageBuilder(new MessageBuilderContext<TObj, TValue>(context, value, component))
            : component.GetErrorMessage(context, value);

        ValidationFailure failure = new ValidationFailure<TValue>(context.PropertyPath!, error, value);
        failure.ErrorCode = component.ErrorCode ?? ValidatorOptions.ErrorCodeResolver(component.Validator);
        failure.Severity = component.SeverityProvider?.Invoke(context, value) ?? ValidatorOptions.Severity;

        if (ValidatorOptions.OnFailureCreated != null)
        {
            failure = ValidatorOptions.OnFailureCreated(failure, context, value, this, component);
        }

        return failure;
    }

    /// <summary>
    /// Prepares the <see cref="MessageFormatter"/> of <paramref name="context"/> for an upcoming <see cref="ValidationFailure"/>.
    /// </summary>
    /// <param name="context">The validator context</param>
    /// <param name="value">Property value.</param>
    protected void PrepareMessageFormatterForValidationError(ValidationContext<TObj> context, TValue? value)
    {
        context.MessageFormatter.AppendPropertyName(context.DisplayName);
        context.MessageFormatter.AppendPropertyValue(value);
        context.MessageFormatter.AppendArgument("PropertyPath", context.PropertyPath);

        // TODO: DELETE ME
        // // If there's a collection index cached in the root context data then add it
        // // to the message formatter. This happens when a child validator is executed
        // // as part of a call to RuleForEach. Usually parameters are not flowed through to
        // // child validators, but we make an exception for collection indices.
        // if (context.RootContextData.TryGetValue("__FV_CollectionIndex", out var index)) {
        //     // If our property validator has explicitly added a placeholder for the collection index
        //     // don't overwrite it with the cached version.
        //     if (!context.MessageFormatter.PlaceholderValues.ContainsKey("CollectionIndex")) {
        //         context.MessageFormatter.AppendArgument("CollectionIndex", index);
        //     }
        // }
    }
}
