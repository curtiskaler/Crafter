using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ValidationResult = Auturge.Validation.ValidationResult;

namespace Auturge.Validation.Tests;

// EXAMPLE BELOW

public class ValidatorTests
{
    private TestUserValidator _validator;

    [SetUp]
    public void Setup() => _validator = new TestUserValidator();

    [Test]
    public void Validator_CatchesMissingProperties()
    {
        var user = new TestUser("Curt");

        ValidationResult? result = _validator.Validate(user);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors.FirstMessage(), Is.EqualTo("Email is required"));
    }

    [Test]
    public void Validator_CatchesBadUrl()
    {
        var user = new TestUser("Curt", "Dingo@email.com", 69) { Website = "dongle" };

        ValidationResult? result = _validator.Validate(user);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors.FirstMessage(), Is.EqualTo("Invalid URL"));
    }

    [Test]
    public void Validator_GoldenPath()
    {
        var user = new TestUser("Curt", "Dingo@email.com", 69) { Website = "https://www.google.com" };

        ValidationResult? result = _validator.Validate(user);

        Assert.That(result, Is.EqualTo(ValidationResult.Success));
    }

    // [Test]
    // public void Validator_CatchesNullValues()
    // {
    //     TestUser? user = null;
    //
    //     ValidationResult? result = _validator.Validate(user);
    //
    //     Assert.That(result, Is.Not.Null);
    //     Assert.That(result.Errors.FirstMessage(), Is.EqualTo("Value cannot be null"));
    // }

    // [Test]
    // public void Validator_CatchesExtensions()
    // {
    //     var user = new TestUser("lowercase", "Dingo@email.com", 69);
    //     _validator.AddRuleFor(x => x.Name).MustBeCapitalized();
    //
    //     ValidationResult? result = _validator.Validate(user);
    //
    //     Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
    // }
}
