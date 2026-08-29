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

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void URN_Test()
    {
        long id = Flake.NewFlake();

        var urn = new TestURN("recipe:bongo", id.ToString());

        Assert.That(urn, Is.Not.Null);
        Assert.That(urn.ToString(), Is.EqualTo($"urn:recipe:bongo:{id}"));
    }

    [Test]
    public void URNReference_Test()
    {
        long id = Flake.NewFlake();
        var urn = new TestURN("recipe:bongo", id.ToString());

        var reference = new Reference<TestURN>("Chocolate Chip Cookies", urn);

        Assert.That(reference, Is.Not.Null);
        Assert.That(reference.Resource.ToString(), Is.EqualTo($"urn:recipe:bongo:{id}"));
        Assert.That(reference.DisplayName, Is.EqualTo("Chocolate Chip Cookies"));
    }

    [Test]
    public void URNLink_Test()
    {
        long id = Flake.NewFlake();
        var urn = new TestURN("recipe:bongo", id.ToString());
        var reference = new Reference<TestURN>("Chocolate Chip Cookies", urn);
        var hyperlink = new Uri("http://localhost:8080/foundation/hub");

        var link = new ResourceLink<TestURN>(reference, hyperlink);

        Assert.That(link, Is.Not.Null);
        Assert.That(link.DisplayName, Is.EqualTo("Chocolate Chip Cookies"));
        Assert.That(link.Resource.ToString(), Is.EqualTo($"urn:recipe:bongo:{id}"));
        Assert.That(link.Link, Is.EqualTo(hyperlink));
    }
}
