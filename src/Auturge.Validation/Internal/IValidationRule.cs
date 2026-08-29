using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Auturge.Validation;

/// <summary>
/// Defines a rule associated with a property which can have multiple validators.
/// </summary>
public interface IValidationRule
{
    /// <summary> The components in this rule. </summary>
    List<IRuleComponent> Components { get; }

    /// <summary> Dependent rules. </summary>
    List<IValidationRule> DependentRules { get; }

    /// <summary> Expression that was used to create the rule. </summary>
    LambdaExpression Expression { get; }

    /// <summary> Whether the rule has a condition defined. </summary>
    bool HasCondition { get; }

    /// <summary> Property associated with this rule. </summary>
    MemberInfo Member { get; }

    /// <summary>
    /// Returns the property name for the property being validated.
    /// Returns null if it is not a property being validated (eg a method call)
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary> Type of the property being validated. </summary>
    public Type TypeToValidate { get; }

    /// <summary> Clears the components of the rule. </summary>
    void Clear();
    
    /// <summary> Gets the display name for the property. </summary>
    /// <param name="context">Current context</param>
    /// <returns>Display name</returns>
    string? GetDisplayName(IValidationContext context);
}

/// <inheritdoc/>
/// <typeparam name="T"></typeparam>
public interface IValidationRule<T> : IValidationRule
{
    /// <summary>
    /// Applies a condition to a single rule chain.
    /// The condition can be applied to either the current property validator in the chain,
    /// or all preceding property validators in the chain (the default).
    /// </summary>
    /// <param name="predicate">The condition to apply</param>
    /// <param name="applyConditionTo">Whether the condition should be applied to the current property validator in the chain, or all preceding property validators in the chain.</param>
    void ApplyCondition(Func<ValidationContext<T>, bool> predicate,
        ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators);

    /// <summary>
    /// Gets the property value for this rule. Note that this bypasses all conditions.
    /// </summary>
    /// <param name="instance">The model from which the property value should be retrieved.</param>
    /// <returns>The property value.</returns>
    object? GetPropertyValue(T instance);

    /// <summary>
    /// Attempts to get the value of a property from the specified instance.
    /// </summary>
    /// <typeparam name="TProp">The type of the property to retrieve.</typeparam>
    /// <param name="instance">The instance from which to retrieve the property value.</param>
    /// <param name="value">When this method returns, contains the value of the property, if the retrieval was successful; otherwise, the default value for the type of the property.</param>
    /// <returns>
    /// true if the property value was successfully retrieved and is of type <typeparamref name="TProp"/>; otherwise, false.
    /// </returns>
    bool TryGetPropertyValue<TProp>(T instance, [MaybeNullWhen(false)] out TProp value);
}

/// <inheritdoc/>
/// <typeparam name="TObj"></typeparam>
/// <typeparam name="TProperty"></typeparam>
public interface IValidationRule<TObj, out TProperty> : IValidationRule<TObj>
{
    /// <summary> The current rule component. </summary>
    IRuleComponent<TObj, TProperty>? Current { get; }

    /// <summary> Failure mode for this rule. </summary>
    FailureMode FailureMode { get; set; }
    
    /// <summary>
    /// Allows custom creation of an error message
    /// </summary>
    Func<IMessageBuilderContext<TObj, TProperty>, string>? MessageBuilder { set; }
    
    /// <summary> Adds a validator to this rule. </summary>
    void AddValidator(IPropertyValidator<TObj, TProperty?> validator);
}

/// <summary> Internal - for built-in validator classes. </summary>
internal interface IValidationRuleInternal<T> : IValidationRule<T>
{
    void Validate(ValidationContext<T> context);
    void AddDependentRules(IEnumerable<IValidationRuleInternal<T>> rules);
}

/// <summary> Internal - for built-in validator classes. </summary>
internal interface IValidationRuleInternal<T, TProperty> : IValidationRule<T, TProperty>, IValidationRuleInternal<T> {
    new List<RuleComponent<T,TProperty>> Components { get; }
}
