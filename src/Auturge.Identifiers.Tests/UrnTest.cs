namespace Auturge.Identifiers.Tests;

internal class TestURN : URN
{
    public TestURN(string s) : base(s)
    {
    }

    public TestURN(string nid, string nss) : base(nid, nss)
    {
    }
}

// URN composed with Reference<T> / ResourceLink<T>.
public class UrnCompositionTests
{
    [Test]
    public void Urn_Should_ComposeFromNidAndNss()
    {
        long id = Flake.NewFlake();

        var urn = new TestURN("recipe", $"bongo:{id}");

        Assert.That(urn.NID, Is.EqualTo("recipe"));
        Assert.That(urn.ToString(), Is.EqualTo($"urn:recipe:bongo:{id}"));
    }

    [Test]
    public void Reference_Should_ExposeTheUrnResource()
    {
        long id = Flake.NewFlake();
        var urn = new TestURN("recipe", $"bongo:{id}");

        var reference = new Reference<TestURN>("Chocolate Chip Cookies", urn);

        Assert.That(reference.Resource.ToString(), Is.EqualTo($"urn:recipe:bongo:{id}"));
        Assert.That(reference.DisplayName, Is.EqualTo("Chocolate Chip Cookies"));
    }

    [Test]
    public void ResourceLink_Should_ExposeTheUrnResourceAndLink()
    {
        long id = Flake.NewFlake();
        var urn = new TestURN("recipe", $"bongo:{id}");
        var reference = new Reference<TestURN>("Chocolate Chip Cookies", urn);
        var hyperlink = new Uri("http://localhost:8080/foundation/hub");

        var link = new ResourceLink<TestURN>(reference, hyperlink);

        Assert.That(link.DisplayName, Is.EqualTo("Chocolate Chip Cookies"));
        Assert.That(link.Resource.ToString(), Is.EqualTo($"urn:recipe:bongo:{id}"));
        Assert.That(link.Link, Is.EqualTo(hyperlink));
    }
}
