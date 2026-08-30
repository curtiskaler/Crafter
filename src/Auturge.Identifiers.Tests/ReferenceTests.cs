namespace Auturge.Identifiers.Tests;

public class ReferenceTests
{
    [Test]
    public void Equals_Should_ReturnTrue_When_DisplayNameAndResourceMatch()
    {
        var a = new Reference<string>("Cookies", "urn:recipe:1");
        var b = new Reference<string>("Cookies", "urn:recipe:1");

        Assert.That(a.Equals(b), Is.True);
        Assert.That(a == b, Is.True);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_ResourceDiffers()
    {
        var a = new Reference<string>("Cookies", "urn:recipe:1");
        var b = new Reference<string>("Cookies", "urn:recipe:2");

        Assert.That(a == b, Is.False);
        Assert.That(a != b, Is.True);
    }

    [Test]
    public void EqualityOperator_Should_ReturnTrue_When_BothOperandsAreNull()
    {
        Reference<string>? a = null;
        Reference<string>? b = null;

        Assert.That(a == b, Is.True);
    }

    [Test]
    public void Ctor_Should_Throw_When_DisplayNameIsNull()
    {
        Assert.That(() => new Reference<string>(null!, "urn:recipe:1"),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_ResourceIsNull()
    {
        Assert.That(() => new Reference<string>("Cookies", null!),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_ComparedWithResourceLinkCarryingSameReferenceData()
    {
        var reference = new Reference<string>("Cookies", "urn:recipe:1");
        var link = new ResourceLink<string>(reference, new Uri("http://localhost/hub"));

        Assert.That(reference.Equals(link), Is.False);
        Assert.That(reference == link, Is.False);
    }

    [Test]
    public void InequalityOperator_Should_ReturnTrue_When_DisplayNameDiffers()
    {
        var a = new Reference<string>("Cookies", "urn:recipe:1");
        var b = new Reference<string>("Cake", "urn:recipe:1");

        Assert.That(a != b, Is.True);
        Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void EqualityOperator_Should_ReturnFalse_When_OneOperandIsNull()
    {
        var a = new Reference<string>("Cookies", "urn:recipe:1");

        Assert.That(a == null, Is.False);
        Assert.That(null == a, Is.False);
        Assert.That(a != null, Is.True);
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_ComparedWithNullOrUnrelatedType()
    {
        var a = new Reference<string>("Cookies", "urn:recipe:1");

        Assert.That(a.Equals("Cookies"), Is.False);
        Assert.That(a.Equals((object?)null), Is.False);
    }

    [Test]
    public void Equals_Should_ReturnTrue_When_ComparedWithSameInstanceOrBoxedEqual()
    {
        var a = new Reference<string>("Cookies", "urn:recipe:1");
        var same = a;
        object boxed = new Reference<string>("Cookies", "urn:recipe:1");

        Assert.That(a.Equals(same), Is.True);
        Assert.That(a == same, Is.True);
        Assert.That(a.Equals(boxed), Is.True);
    }
}
