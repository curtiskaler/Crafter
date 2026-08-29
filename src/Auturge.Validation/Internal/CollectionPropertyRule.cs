using System.Linq.Expressions;
using System.Reflection;

namespace Auturge.Validation;

/// <summary>
/// Rule definition for collection properties
/// </summary>
internal class CollectionPropertyRule<TObj, TElement> :
    RuleBase<TObj, IEnumerable<TElement>, TElement>,
    ICollectionRule<TObj, TElement>,
    IValidationRuleInternal<TObj, TElement> where TObj : notnull
{
    /// <summary>
    /// Initialize a new instance of a CollectionPropertyRule
    /// </summary>
    public CollectionPropertyRule(MemberInfo member, Func<TObj?, IEnumerable<TElement>?> propertyFunc,
        LambdaExpression expression, Func<FailureMode> failureModeFunc, Type typeToValidate)
        : base(member, propertyFunc, expression, failureModeFunc, typeToValidate)
    {
    }
    
    /// <inheritdoc/>
    public Func<TElement, bool>? Filter { get; set; }
    
    /// <inheritdoc/>
    public Func<TObj?, IEnumerable<TElement>, TElement, int, string>? IndexBuilder { get; set; }

    /// <inheritdoc/>
    void IValidationRuleInternal<TObj>.AddDependentRules(IEnumerable<IValidationRuleInternal<TObj>> rules) 
        => DependentRules.AddRange(rules);

    public void Validate(ValidationContext<TObj> context)
    {
        string? displayName = GetDisplayName(context);

		if (PropertyName == null && displayName == null) {
			//No name has been specified. Assume this is a model-level rule, so we should use empty string instead.
			displayName = string.Empty;
		}

		// Construct the full name of the property, taking into account overridden property names and the chain (if we're in a nested validator)
		string propertyName = context.PropertyChain.BuildPropertyPath(PropertyName ?? displayName);

		if (string.IsNullOrEmpty(propertyName)) {
			propertyName = InferPropertyName(Expression);
		}

		// Ensure that this rule is allowed to run.
		// The validator selector has the opportunity to veto this before any of the validators execute.
		if (!context.Selector.CanExecute(this, propertyName, context)) {
			return;
		}

		if (Condition != null) {
			if (!Condition(context)) {
				return;
			}
		}

		List<RuleComponent<TObj, TElement>> filteredValidators = GetValidatorsToExecute(context);

		if (filteredValidators.Count == 0) {
			// If there are no property validators to execute after running the conditions, bail out.
			return;
		}

		FailureMode failureMode = FailureMode;

        IEnumerable<TElement> collection = PropertyFunc(context.InstanceToValidate) ?? 
                                           throw new NullReferenceException($"NullReferenceException occurred when executing rule for {Expression}. If this property can be null you should add a null check using a When condition");

		int count = 0;
		int totalFailures = context.Failures.Count;

        if (string.IsNullOrEmpty(propertyName)) {
            throw new InvalidOperationException("Could not automatically determine the property name ");
        }

        // call toList to avoid changing the list during enumeration
        IEnumerable<TElement> elements = collection.ToList();
        foreach (TElement element in elements) {
            int index = count++;

            if (Filter != null && !Filter(element)) {
                continue;
            }

            string indexer = index.ToString();
            bool useDefaultIndexFormat = true;

            if (IndexBuilder != null) {
                indexer = IndexBuilder(context.InstanceToValidate, elements, element, index);
                useDefaultIndexFormat = false;
            }

            context.PrepareForChildCollectionValidator();
            context.PropertyChain.Add(propertyName);
            context.PropertyChain.AddIndexer(indexer, useDefaultIndexFormat);

            TElement valueToValidate = element;
            string propertyPath = context.PropertyChain.ToString();
            int totalFailuresInner = context.Failures.Count;
            context.InitializeForPropertyValidator(propertyPath, _displayNameFunc, PropertyName);

            foreach (RuleComponent<TObj, TElement> component in filteredValidators) {
                context.MessageFormatter.Reset();
                context.MessageFormatter.AppendArgument("CollectionIndex", index);

                bool valid = component.Validate(context, valueToValidate);

                if (!valid) {
                    PrepareMessageFormatterForValidationError(context, valueToValidate);
                    ValidationFailure failure = CreateValidationError(context, valueToValidate, component);
                    context.Failures.Add(failure);
                }

                // If there has been at least one failure, and our CascadeMode has been set to Stop
                // then don't continue to the next rule
                if (context.Failures.Count > totalFailuresInner && failureMode == FailureMode.Stop) {
                    context.RestoreState();
                    goto AfterValidate;
                }
            }

            context.RestoreState();
        }

        AfterValidate:

		if (context.Failures.Count <= totalFailures) {
			foreach (IValidationRuleInternal<TObj> dependentRule in DependentRules) {
				dependentRule.Validate(context);
			}
		}
    }
    
    private List<RuleComponent<TObj, TElement>> GetValidatorsToExecute(ValidationContext<TObj> context) {
        // Loop over each validator and check if its condition allows it to run.
        // This needs to be done prior to the main loop as within a collection rule
        // validators' conditions still act upon the root object, not upon the collection property.
        // This allows the property validators to cancel their execution prior to the collection
        // being retrieved (thereby possibly avoiding NullReferenceExceptions).
        // Must call ToList so we don't modify the original collection mid-loop.
        var validators = Components.ToList();

        foreach (RuleComponent<TObj, TElement> component in Components
                     .Where(component => component.HasCondition)
                     .Where(component => !component.InvokeCondition(context)))
        {
            validators.Remove(component);
        }

        return validators;
    }

    /// <summary>
    /// Creates a new property rule from a lambda expression.
    /// </summary>
    public static CollectionPropertyRule<TObj, TElement> Create(Expression<Func<TObj, IEnumerable<TElement>>> expression, Func<FailureMode> failureModeFn, bool bypassCache = false) 
    {
        MemberInfo? member = expression.GetMember();
        Func<TObj?, IEnumerable<TElement>?> compiled = AccessorCache<TObj>.GetCachedAccessor(member, expression!, bypassCache, "FV_RuleForEach");
        return new CollectionPropertyRule<TObj, TElement>(member, x => compiled(x), expression, failureModeFn, typeof(TElement));
    }
    
    private static string InferPropertyName(LambdaExpression expression) {
        if (expression.Body is not ParameterExpression paramExp) {
            throw new InvalidOperationException("Could not infer property name for expression: " + expression + ". Please explicitly specify a property name by calling OverridePropertyName as part of the rule chain. Eg: RuleForEach(x => x).NotNull().OverridePropertyName(\"MyProperty\")");
        }

        return paramExp.Name ?? throw new ArgumentException("parameter name must not be null");
    }
}
