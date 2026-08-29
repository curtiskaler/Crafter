using Crafter.Model.Identifiers;

namespace Crafter.Model.Tests.Identifiers;

public class UrnTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void URN_Test()
    {
        const string entityType = "recipe:random";
        long id = Flake.NewFlake();

        var urn = new URN(entityType, id.ToString());

        Assert.That(urn, Is.Not.Null);
        Assert.That(urn.Id, Is.EqualTo(id.ToString()));
        Assert.That(urn.EntityType, Is.EqualTo(entityType));
    }

    [Test]
    public void URNReference_Test()
    {
        const string displayName = "Chocolate Chip Cookies";
        const string entityType = "recipe:random";
        long id = Flake.NewFlake();
        var urn = new URN(entityType, id.ToString());

        var reference = new URNReference(displayName, urn);

        Assert.That(reference, Is.Not.Null);
        Assert.That(reference.DisplayName, Is.EqualTo(displayName));
        Assert.That(reference.URN.Id, Is.EqualTo(id.ToString()));
        Assert.That(reference.URN.EntityType, Is.EqualTo(entityType));
    }

    [Test]
    public void URNLink_Test()
    {
        const string displayName = "Chocolate Chip Cookies";
        const string entityType = "recipe:random";
        const string uriString = "http://localhost:8080/foundation/hub";
        long id = Flake.NewFlake();
        var urn = new URN(entityType, id.ToString());
        var reference = new URNReference(displayName, urn);
        var hyperlink = new Uri(uriString);

        var link = new URNLink(reference, hyperlink);

        Assert.That(link, Is.Not.Null);
        Assert.That(link.DisplayName, Is.EqualTo(displayName));
        Assert.That(link.Resource.Id, Is.EqualTo(id.ToString()));
        Assert.That(link.Resource.EntityType, Is.EqualTo(entityType));
        Assert.That(link.Link, Is.EqualTo(hyperlink));
    }
}
