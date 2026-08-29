using System.Linq.Expressions;
using System.Reflection;

namespace Auturge.Validation;

/// <summary> Runtime options for all validators. </summary>
public static class ValidatorOptions
{
    /// <summary> Default severity level. </summary>
    public static Severity Severity { get; set; } = Severity.Error;

    /// <summary> Defines a hook that runs when a <see cref="ValidationFailure"/> is created. </summary>
    public static Func<ValidationFailure, IValidationContext, object?, IValidationRule, IRuleComponent,
            ValidationFailure>?
        OnFailureCreated { get; set; }

    /// <summary> Logic for resolving display names. </summary>
    public static Func<Type, MemberInfo, LambdaExpression, string> DisplayNameResolver { get; set; } =
        DefaultDisplayNameResolver;

    /// <summary> Resolver for default error codes. </summary>
    public static Func<IPropertyValidator, string> ErrorCodeResolver { get; set; } = DefaultErrorCodeResolver;

    /// <summary> Factory for creating MessageFormatter instances. </summary>
    public static Func<MessageFormatter> MessageFormatterFactory { get; set; } = () => new MessageFormatter();
    
    /// <summary> Logic for resolving property names. </summary>
    public static Func<Type, MemberInfo, LambdaExpression, string> PropertyNameResolver { get; set; } =
        DefaultPropertyNameResolver;

    /// <summary>
    /// <para>
    /// Sets the default value for <see cref="Validator{T}.ClassLevelFailureMode"/>.
    /// Defaults to <see cref="FailureMode.Continue"/> if not set.
    /// </para>
    /// </summary>
    public static FailureMode DefaultClassLevelFailureMode { get; set; } = FailureMode.Continue;

    /// <summary>
    /// <para>
    /// Sets the default value for <see cref="Validator{T}.RuleLevelFailureMode"/>
    /// Defaults to <see cref="FailureMode.Continue"/> if not set.
    /// </para>
    /// </summary>
    public static FailureMode DefaultRuleLevelFailureMode { get; set; } = FailureMode.Continue;
    
    private static string DefaultErrorCodeResolver(IPropertyValidator validator) => validator.Name;

    private static string? DefaultDisplayNameResolver(Type type, MemberInfo memberInfo, LambdaExpression expression) =>
        null;

    private static string DefaultPropertyNameResolver(Type type, MemberInfo? memberInfo, LambdaExpression? expression)
    {
        if (expression != null)
        {
            var chain = PropertyChain.FromExpression(expression);
            if (chain.Count > 0) return chain.ToString();
        }

        return memberInfo?.Name ?? string.Empty;
    }
}
