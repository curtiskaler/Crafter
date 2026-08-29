namespace Auturge.Validation;

internal class RuleBuilder<TObj, TProperty>(IValidationRuleInternal<TObj, TProperty?> rule, Validator<TObj> parent) :
    IPropertyRuleBuilder<TObj, TProperty>,
    ICollectionRuleBuilder<TObj, TProperty>,
    IRuleBuilderOptions<TObj, TProperty>,
    IRuleBuilderOptionsConditions<TObj, TProperty>,
    IRuleBuilderInternal<TObj, TProperty>
{
    /// <summary> The rule being created by this RuleBuilder. </summary>
    public IValidationRuleInternal<TObj, TProperty?> Rule { get; } = rule;

    /// <summary> The parent validator. </summary>
    public Validator<TObj> ParentValidator { get; } = parent;

    public void AddComponent(RuleComponent<TObj, TProperty?> component) => Rule.Components.Add(component);
    
    public IRuleBuilderOptions<TObj, TProperty> SetValidator(IPropertyValidator<TObj, TProperty?> validator) {
        ArgumentNullException.ThrowIfNull(validator);
        Rule.AddValidator(validator);
        return this;
    }
    
    IRuleBuilderOptions<TObj, TProperty?> IRuleBuilderOptions<TObj, TProperty>.DependentRules(Action action)
    {
        DependentRulesInternal(action);
        return this;
    }
    
    IRuleBuilderOptionsConditions<TObj, TProperty> IRuleBuilderOptionsConditions<TObj, TProperty>.DependentRules(Action action) {
        DependentRulesInternal(action);
        return this;
    }
    
    private void DependentRulesInternal(Action action) {
        var dependencyContainer = new List<IValidationRuleInternal<TObj>>();
        // Capture any rules added to the parent validator inside this delegate.
        using (ParentValidator.Rules.Capture(dependencyContainer.Add)) {
            action();
        }

        // if (Rule.RuleSets != null && Rule.RuleSets.Length > 0) {
        //     foreach (var dependentRule in dependencyContainer) {
        //         if (dependentRule.RuleSets == null) {
        //             dependentRule.RuleSets = Rule.RuleSets;
        //         }
        //     }
        // }

        Rule.AddDependentRules(dependencyContainer);
    }
}

// public IRuleBuilderOptions<TObj, TProperty> SetValidator(IValidator<TProperty> validator, params string[] ruleSets) {
//     ArgumentNullException.ThrowIfNull(validator);
//     var adaptor = new ChildValidatorAdaptor<TObj,TProperty>(validator, validator.GetType()) {
//         RuleSets = ruleSets
//     };
//     // ChildValidatorAdaptor supports both sync and async execution.
//     Rule.AddAsyncValidator(adaptor, adaptor);
//     return this;
// }
