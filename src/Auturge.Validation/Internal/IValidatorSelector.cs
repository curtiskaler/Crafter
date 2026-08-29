namespace Auturge.Validation;

/// <summary>
/// Determines whether a rule should execute.
/// </summary>
public interface IValidatorSelector
{
    /// <summary>
    /// Determines whether a rule should execute.
    /// </summary>
    /// <param name="rule">The rule</param>
    /// <param name="propertyPath">Property path (eg Customer.Address.Line1)</param>
    /// <param name="context">Contextual information</param>
    /// <returns>Whether the validator can execute.</returns>
    bool CanExecute(IValidationRule rule, string propertyPath, IValidationContext context);
}

/// <summary>
/// Default validator selector that will execute all rules that do not belong to a RuleSet.
/// </summary>
public class DefaultValidatorSelector : IValidatorSelector
{
    ///<inheritdoc/>
    public bool CanExecute(IValidationRule rule, string propertyPath, IValidationContext context) => true;
}
