namespace Auturge.Validation;

public static partial class DefaultValidatorExtensions
{
    /// <summary>
    /// Defines a 'not null' validator on the current rule builder.
    /// Validation will fail if the property is null.
    /// </summary>
    /// <typeparam name="T">Type of object being validated</typeparam>
    /// <typeparam name="TProperty">Type of property being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator should be defined</param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> NotNull<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new NotNullValidator<T,TProperty>());
    } 
}
