namespace Auturge.Validation.Tests;

public class TestUserValidator : Validator<TestUser>
{
    public TestUserValidator()
    {
        AddRuleFor(x => x).NotNull();
        AddRuleFor(x => x.Name).Required().WithMessage("Name is required");
        AddRuleFor(x => x.Email).Required().IsEmail().WithMessage("Email is required");
        AddRuleFor(x => x.Website).IsUrl();
    }
}
