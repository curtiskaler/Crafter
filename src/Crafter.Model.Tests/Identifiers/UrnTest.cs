using Crafter.Model.Identifiers;

namespace Crafter.Model.Tests.Identifiers;

public class UrnTests
{
    [Test]
    public void Urn_Should_SplitNssIntoEntityTypeAndId()
    {
        long id = Flake.NewFlake();

        var urn = new URN("recipe:random", id.ToString());

        Assert.That(urn.EntityType, Is.EqualTo("recipe:random"));
        Assert.That(urn.Id, Is.EqualTo(id.ToString()));
        Assert.That(urn.ToString(), Is.EqualTo($"urn:auturge-crafter:recipe:random:{id}"));
    }

    [Test]
    public void Urn_Should_RoundTripThroughAString()
    {
        var original = new URN("recipe", "42");

        var parsed = new URN(original.ToString());

        Assert.That(parsed.EntityType, Is.EqualTo("recipe"));
        Assert.That(parsed.Id, Is.EqualTo("42"));
        Assert.That(parsed, Is.EqualTo(original));
    }

    [Test]
    public void Ctor_Should_AcceptTheNidRegardlessOfCase()
    {
        var urn = new URN("urn:AUTURGE-CRAFTER:recipe:42");

        Assert.That(urn.EntityType, Is.EqualTo("recipe"));
        Assert.That(urn.Id, Is.EqualTo("42"));
    }

    [Test]
    public void Ctor_Should_Throw_When_NidIsNotAuturgeCrafter()
    {
        Assert.That(() => new URN("urn:other:recipe:42"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_IdContainsAColon()
    {
        Assert.That(() => new URN("recipe", "a:b"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_EntityTypeContainsWhitespace()
    {
        Assert.That(() => new URN("choc chip", "42"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_NssHasNoIdSegment()
    {
        Assert.That(() => new URN("urn:auturge-crafter:recipe"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void URNReference_Should_ExposeUrnAndUseInheritedEquality()
    {
        var a = new URNReference("Cookies", "recipe", "1");
        var b = new URNReference("Cookies", "recipe", "1");
        var different = new URNReference("Cookies", "recipe", "2");

        Assert.That(a.URN.Id, Is.EqualTo("1"));
        Assert.That(a, Is.EqualTo(b));
        Assert.That(a == b, Is.True);
        Assert.That(a, Is.Not.EqualTo(different));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void URNLink_Should_ExposeTheReferenceAndLink()
    {
        var link = new URNLink("Cookies", "recipe", "1", "http://localhost/hub");

        Assert.That(link.DisplayName, Is.EqualTo("Cookies"));
        Assert.That(link.Resource.EntityType, Is.EqualTo("recipe"));
        Assert.That(link.Resource.Id, Is.EqualTo("1"));
        Assert.That(link.Link, Is.EqualTo(new Uri("http://localhost/hub")));
    }

    [Test]
    public void URNLink_Should_ConsiderTheLinkInEquality()
    {
        var a = new URNLink("Cookies", "recipe", "1", "http://localhost/a");
        var b = new URNLink("Cookies", "recipe", "1", "http://localhost/b");

        Assert.That(a == b, Is.False);
    }
}
