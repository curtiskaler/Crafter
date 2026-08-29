using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Auturge.Validation.Exceptions;

namespace Auturge.Validation;

public class Validator<T> : IValidator<T>, IEnumerable<IValidationRule>
{
    internal ObservableCollection<IValidationRuleInternal<T>> Rules { get; } = new();
    private Func<FailureMode> _classLevelFailureMode = () => ValidatorOptions.DefaultClassLevelFailureMode;
    private Func<FailureMode> _ruleLevelFailureMode = () => ValidatorOptions.DefaultRuleLevelFailureMode;

    /// <summary>
    /// <para>
    /// Sets the default failure behavior <i>within</i> each rule in this validator.
    /// </para>
    /// <para>
    /// This overrides the default value set in <see cref="ValidatorConfiguration.DefaultRuleLevelCascadeMode"/>.
    /// </para>
    /// <para>
    /// It can be further overridden for specific rules by calling
    /// <see cref="DefaultValidatorOptions.Cascade{T, TProperty}(IRuleBuilderInitial{T, TProperty}, FluentValidation.CascadeMode)"/>.
    /// <seealso cref="RuleBase{T, TProperty, TValue}.FailureMode"/>.
    /// </para>
    /// <para>
    /// Note that failure behavior <i>between</i> rules is controlled by <see cref="Validator{T}.ClassLevelFailureMode"/>.
    /// </para>
    /// </summary>
    public FailureMode ClassLevelFailureMode
    {
        get => _ruleLevelFailureMode();
        set => _ruleLevelFailureMode = () => value;
    }

    /// <summary>
    /// <para>
    /// Sets the default failure behavior <i>within</i> each rule in this validator.
    /// </para>
    /// <para>
    /// This overrides the default value set in <see cref="ValidatorConfiguration.DefaultRuleLevelFailureMode"/>.
    /// </para>
    /// <para>
    /// It can be further overridden for specific rules by calling
    /// <see cref="DefaultValidatorOptions.Cascade{T, TProperty}(IRuleBuilderInitial{T, TProperty}, FailureMode)"/>.
    /// <seealso cref="RuleBase{T, TProperty, TValue}.FailureMode"/>.
    /// </para>
    /// <para>
    /// Note that failure behavior <i>between</i> rules is controlled by <see cref="Validator{T}.ClassLevelFailureMode"/>.
    /// </para>
    /// </summary>
    public FailureMode RuleLevelFailureMode
    {
        get => _ruleLevelFailureMode();
        set => _ruleLevelFailureMode = () => value;
    }

    /// <summary>
    /// Defines a validation rule for a specific property.
    /// </summary>
    /// <example>
    /// AddRuleFor(x => x.Prop)...
    /// </example>
    /// <typeparam name="TProperty">The type of property being validated</typeparam>
    /// <param name="expression">The expression representing the property to validate</param>
    /// <returns>an IRuleBuilder instance on which validators can be defined</returns>
    public IRuleBuilder<T, TProperty> AddRuleFor<TProperty>(Expression<Func<T, TProperty?>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        var rule = PropertyRule<T, TProperty>.Create(expression, () => RuleLevelFailureMode);
        Rules.Add(rule);
        OnRuleAdded(rule);
        return new RuleBuilder<T, TProperty>(rule, this);
    }

    /// <summary>
    /// Invokes a rule for each item in the collection.
    /// </summary>
    /// <typeparam name="TElement">Type of property</typeparam>
    /// <param name="expression">Expression representing the collection to validate</param>
    /// <returns>An IRuleBuilder instance on which validators can be defined</returns>
    public ICollectionRuleBuilder<T, TElement> AddRuleForEach<TElement>(
        Expression<Func<T, IEnumerable<TElement>>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        var rule = CollectionPropertyRule<T, TElement>.Create(expression, () => RuleLevelFailureMode);
        Rules.Add(rule);
        OnRuleAdded(rule);
        return new RuleBuilder<T, TElement>(rule, this);
    }

    /// <inheritdoc/>
    bool IValidator.CanHandle(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return typeof(T).IsAssignableFrom(type);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection of validation rules.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public IEnumerator<IValidationRule> GetEnumerator() => Rules.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// This method is invoked when a rule has been created (via AddRuleFor/AddRuleForEach) and has been added to the validator.
    /// You can override this method to provide customizations to all rule instances.
    /// </summary>
    /// <param name="rule"></param>
    protected virtual void OnRuleAdded(IValidationRule<T> rule)
    {
    }

    /// <summary>
    /// Determines if validation should occur and provides a means to modify the context and ValidationResult prior to execution.
    /// If this method returns false, then the ValidationResult is immediately returned from Validate/ValidateAsync.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    protected virtual bool PreValidate(ValidationContext<T> context, ValidationResult result) => true;

    /// <summary>
    /// Throws a ValidationException. This method will only be called if the validator has been configured
    /// to throw exceptions if validation fails. The default behaviour is not to throw an exception.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <exception cref="ValidationException"></exception>
    protected virtual void RaiseValidationException(ValidationContext<T> context, ValidationResult result)
        => throw new ValidationException(result.Errors);

    /// <summary> Validates the specified instance. </summary>
    /// <param name="instance">The object to validate.</param>
    /// <returns>A ValidationResult object containing any validation failures.</returns>
    public ValidationResult Validate(T instance) => Validate(new ValidationContext<T>(instance));

    ValidationResult IValidator.Validate(IValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Validate(ValidationContext<T>.GetFromNonGenericContext(context));
    }

    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="context">Validation Context</param>
    /// <returns>A ValidationResult object containing any validation failures.</returns>
    public virtual ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult(context.Failures);
        bool shouldContinue = PreValidate(context, result);

        if (!shouldContinue)
        {
            if (!result.IsValid && context.ThrowOnFailures)
            {
                RaiseValidationException(context, result);
            }

            return result;
        }

        if (context.InstanceToValidate == null)
        {
            throw new InvalidOperationException(
                "Cannot pass a null model to Validate/ValidateAsync. The root model must be non-null.");
        }

        int count = Rules.Count;

        // Performance: Use for loop rather than foreach to reduce allocations.
        for (int i = 0; i < count; i++)
        {
            int totalFailures = context.Failures.Count;
            Rules[i].Validate(context);
            if (ClassLevelFailureMode == FailureMode.Stop && result.Errors.Count > totalFailures)
            {
                // Bail out if we're "failing-fast". Check to see if the number of failures
                // has been increased by this rule (which could've generated 1 or more failures).
                break;
            }
        }

        // SetExecutedRuleSets(result, context);

        if (!result.IsValid && context.ThrowOnFailures)
        {
            RaiseValidationException(context, result);
        }

        return result;
    }
}
