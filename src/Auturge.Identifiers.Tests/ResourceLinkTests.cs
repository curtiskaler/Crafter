namespace Auturge.Identifiers.Tests;

public class ResourceLinkTests
{
    private static ResourceLink<string> Link(string display, string resource, string url)
        => new(new Reference<string>(display, resource), new Uri(url));

    [Test]
    public void Equals_Should_ReturnTrue_When_ReferenceAndLinkMatch()
    {
        ResourceLink<string> a = Link("Cookies", "urn:recipe:1", "http://localhost/hub");
        ResourceLink<string> b = Link("Cookies", "urn:recipe:1", "http://localhost/hub");

        Assert.That(a.Equals(b), Is.True);
        Assert.That(a == b, Is.True);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void EqualityOperator_Should_AgreeWithEquals_When_LinkDiffers()
    {
        ResourceLink<string> a = Link("Cookies", "urn:recipe:1", "http://localhost/hub");
        ResourceLink<string> b = Link("Cookies", "urn:recipe:1", "http://localhost/other");

        Assert.That(a.Equals(b), Is.False);
        Assert.That(a == b, Is.False);
    }

    [Test]
    public void EqualityOperator_Should_ConsiderLink_When_ComparedThroughBaseReferenceType()
    {
        Reference<string> a = Link("Cookies", "urn:recipe:1", "http://localhost/hub");
        Reference<string> b = Link("Cookies", "urn:recipe:1", "http://localhost/other");

        Assert.That(a == b, Is.False);
        Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Ctor_Should_Throw_When_LinkIsNull()
    {
        var reference = new Reference<string>("Cookies", "urn:recipe:1");

        Assert.That(() => new ResourceLink<string>(reference, null!),
            Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void InequalityOperator_Should_ReturnTrue_When_LinkDiffers()
    {
        ResourceLink<string> a = Link("Cookies", "urn:recipe:1", "http://localhost/hub");
        ResourceLink<string> b = Link("Cookies", "urn:recipe:1", "http://localhost/other");

        Assert.That(a != b, Is.True);
    }

    [Test]
    public void GetHashCode_Should_Differ_When_LinkDiffers()
    {
        ResourceLink<string> a = Link("Cookies", "urn:recipe:1", "http://localhost/hub");
        ResourceLink<string> b = Link("Cookies", "urn:recipe:1", "http://localhost/other");

        Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_DisplayNameDiffers()
    {
        ResourceLink<string> a = Link("Cookies", "urn:recipe:1", "http://localhost/hub");
        ResourceLink<string> b = Link("Cake", "urn:recipe:1", "http://localhost/hub");

        Assert.That(a.Equals(b), Is.False);
        Assert.That(a == b, Is.False);
    }

    [Test]
    public void Equals_Should_ReturnTrue_When_ComparedWithSameInstanceOrBoxedEqual()
    {
        ResourceLink<string> a = Link("Cookies", "urn:recipe:1", "http://localhost/hub");
        var same = a;
        object boxed = Link("Cookies", "urn:recipe:1", "http://localhost/hub");

        Assert.That(a.Equals(same), Is.True);
        Assert.That(a == same, Is.True);
        Assert.That(a.Equals(boxed), Is.True);
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_ComparedWithNullOrUnrelatedType()
    {
        ResourceLink<string> a = Link("Cookies", "urn:recipe:1", "http://localhost/hub");
        object unrelated = new();
        bool equalsUnrelated = a.Equals(unrelated);

        Assert.That(a.Equals((object?)null), Is.False);
        Assert.That(equalsUnrelated, Is.False);
    }
}
