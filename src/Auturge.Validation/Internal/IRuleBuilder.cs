namespace Auturge.Validation;

public interface IRuleBuilder<TObj, out TProperty>
{
    /// <summary>
    /// Associates a validator with this the property for this rule builder.
    /// </summary>
    /// <param name="validator">The validator to set</param>
    /// <returns></returns>
    IRuleBuilderOptions<TObj, TProperty> SetValidator(IPropertyValidator<TObj, TProperty?> validator);
}

/// <summary> Rule builder for a property. </summary>
public interface IPropertyRuleBuilder<TObj, out TProperty> : IRuleBuilder<TObj, TProperty>
{
}

/// <summary> Rule builder for a child collection. </summary>
public interface ICollectionRuleBuilder<TObj, out TElement> : IRuleBuilder<TObj, TElement>
{
}

public interface IConditionRuleBuilder<TObj, out TProperty> : IRuleBuilder<TObj, TProperty>
{
    IConditionRuleBuilder<TObj, TProperty> DependentRules(Action action);
}
