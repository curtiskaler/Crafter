namespace Auturge.Validation.Tests;

public static class UserValidatorExtensions
{
    // public static TestUserValidator MustBeCapitalized(this TestUserValidator validator,
    //     Expression<Func<TestUser, string?>> expression)
    // {
    //     validator.RuleFor<TestUserValidator, TestUser, string>(expression,
    //         (s) => s?.ToUpperInvariant() == s,
    //         "{0} must be capitalized.");
    //
    //     return validator;
    // }

    // public static StringValidator MustBeCapitalized(this StringValidator validator)
    // {
    //     validator.RuleFor<StringValidator, string>(
    //         (string? s) => s?.ToUpperInvariant() == s,
    //         "{0} must be capitalized.");
    //     return validator;
    // }


    public static string FirstMessage(this List<ValidationFailure> failures)
        => failures.FirstOrDefault()?.ErrorMessage ?? string.Empty;
}
