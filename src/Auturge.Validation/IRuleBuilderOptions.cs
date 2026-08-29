namespace Auturge.Validation;

/// <summary> Rule builder. </summary>
public interface IRuleBuilderOptions<T, out TProperty> : IRuleBuilder<T, TProperty>
{
    /// <summary> Creates a scope for declaring dependent rules. </summary>
    IRuleBuilderOptions<T, TProperty?> DependentRules(Action action);
}

/// <summary>
/// Rule builder (for validators that only support conditions, but no other options)
/// </summary>
public interface IRuleBuilderOptionsConditions<T, out TProperty> : IRuleBuilder<T, TProperty>
{
    /// <summary>
    /// Creates a scope for declaring dependent rules.
    /// </summary>
    IRuleBuilderOptionsConditions<T, TProperty> DependentRules(Action action);
}

internal interface IRuleBuilderInternal<T, out TProperty> : IRuleBuilderInternal<T> {
    IValidationRuleInternal<T, TProperty?> Rule { get; }
}

internal interface IRuleBuilderInternal<T> {
    Validator<T> ParentValidator { get; }
}
