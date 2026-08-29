using System.Linq.Expressions;
using System.Reflection;

namespace Auturge.Validation;

/// <summary> A rule associated with a property. </summary>
internal class PropertyRule<TInput, TProperty> : RuleBase<TInput, TProperty, TProperty>,
    IValidationRuleInternal<TInput, TProperty>
{
    public PropertyRule(MemberInfo member, Func<TInput?, TProperty?> propertyFunc, LambdaExpression expression,
        Func<FailureMode> failureModeFunc,
        Type typeToValidate) : base(member, propertyFunc, expression, failureModeFunc, typeToValidate)
    {
    }

    /// <summary> Creates a new property rule from a lambda expression. </summary>
    public static PropertyRule<TInput, TProperty> Create(Expression<Func<TInput, TProperty?>> expression,
        Func<FailureMode> failureModeFunc, bool bypassCache = false)
    {
        MemberInfo? member = expression.GetMember();
        ArgumentNullException.ThrowIfNull(member);

        Func<TInput?, TProperty?> compiled = AccessorCache<TInput>.GetCachedAccessor(member, expression, bypassCache);
        return new PropertyRule<TInput, TProperty>(member, compiled, expression, failureModeFunc, typeof(TProperty));
    }

    public void Validate(ValidationContext<TInput> context)
    {
        string? displayName = GetDisplayName(context);

        if (PropertyName == null && displayName == null)
        {
            //No name has been specified. Assume this is a model-level rule, so we should use empty string instead.
            displayName = string.Empty;
        }

        // Construct the full name of the property, taking into account overridden property names
        // and the chain (if we're in a nested validator)
        string propertyPath = context.PropertyChain.BuildPropertyPath(PropertyName ?? displayName);

        // Ensure that this rule is allowed to run.
        // The validator selector has the opportunity to veto this before any of the validators execute.
        if (!context.Selector.CanExecute(this, propertyPath, context))
        {
            return;
        }

        if (Condition != null)
        {
            if (!Condition(context))
            {
                return;
            }
        }

        bool first = true;
        var propValue = default(TProperty);

        FailureMode failureMode = FailureMode;
        int totalFailures = context.Failures.Count;

        context.InitializeForPropertyValidator(propertyPath, _displayNameFunc, PropertyName);

        // Invoke each validator and collect its results.
        foreach (RuleComponent<TInput, TProperty> component in Components)
        {
            context.MessageFormatter.Reset();

            if (!component.InvokeCondition(context))
            {
                continue;
            }

            if (first)
            {
                first = false;
                try
                {
                    propValue = PropertyFunc(context.InstanceToValidate);
                }
                catch (NullReferenceException nre)
                {
                    throw new NullReferenceException(
                        $"NullReferenceException occurred when executing rule for {Expression}. If this property can be null you should add a null check using a When condition",
                        nre);
                }
            }

            bool valid = component.Validate(context, propValue);

            if (!valid)
            {
                PrepareMessageFormatterForValidationError(context, propValue);
                ValidationFailure failure = CreateValidationError(context, propValue, component);
                context.Failures.Add(failure);
            }

            // If there has been at least one failure, and our CascadeMode has been set to Stop
            // then don't continue to the next rule
            if (context.Failures.Count > totalFailures && failureMode == FailureMode.Stop)
            {
                break;
            }
        }

        if (context.Failures.Count <= totalFailures)
        {
            foreach (IValidationRuleInternal<TInput> dependentRule in DependentRules)
            {
                dependentRule.Validate(context);
            }
        }
    }

    void IValidationRuleInternal<TInput>.AddDependentRules(IEnumerable<IValidationRuleInternal<TInput>> rules)
        => DependentRules.AddRange(rules);
}
