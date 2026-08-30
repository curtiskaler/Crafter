namespace Auturge.Identifiers.Tests;

public class URNTests
{
    private sealed class Subject : URN
    {
        public Subject(string s) : base(s)
        {
        }

        public Subject(string nid, string nss) : base(nid, nss)
        {
        }

        public static bool TryParts(string? s, out string? nid, out string? nss)
            => TryParseParts(s, out nid, out nss);
    }

    [Test]
    public void Equals_Should_ReturnTrue_When_NidDiffersOnlyByCase()
    {
        var lower = new Subject("recipe", "cookie");
        var upper = new Subject("RECIPE", "cookie");

        Assert.That(lower.Equals(upper), Is.True);
    }

    [Test]
    public void GetHashCode_Should_Match_When_NidDiffersOnlyByCase()
    {
        var lower = new Subject("recipe", "cookie");
        var upper = new Subject("RECIPE", "cookie");

        Assert.That(lower.GetHashCode(), Is.EqualTo(upper.GetHashCode()));
    }

    [Test]
    public void GetHashCode_Should_Differ_When_NssDiffers()
    {
        var cookie = new Subject("recipe", "cookie");
        var cake = new Subject("recipe", "cake");

        Assert.That(cookie.GetHashCode(), Is.Not.EqualTo(cake.GetHashCode()));
    }

    [Test]
    public void Ctor_Should_Throw_When_SchemeIsNotUrn()
    {
        Assert.That(() => new Subject("http://example.com/x"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_NidIsEmpty()
    {
        Assert.That(() => new Subject("urn::nss"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void EqualityOperator_Should_TreatCaseInsensitiveNidAsEqual_When_Compared()
    {
        var lower = new Subject("recipe", "cookie");
        var upper = new Subject("RECIPE", "cookie");

        Assert.That(lower == upper, Is.True);
    }

    [Test]
    public void Ctor_Should_ParseNid_When_NidContainsDigits()
    {
        var urn = new Subject("iso3166", "US");

        Assert.That(urn.NID, Is.EqualTo("iso3166"));
        Assert.That(urn.NSS, Is.EqualTo("US"));
    }

    [Test]
    public void Ctor_Should_Throw_When_NidContainsPipe()
    {
        Assert.That(() => new Subject("a|b", "x"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_NssIsEmpty()
    {
        Assert.That(() => new Subject("recipe", ""), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void InequalityOperator_Should_ReturnTrue_When_NssDiffers()
    {
        var cookie = new Subject("recipe", "cookie");
        var cake = new Subject("recipe", "cake");

        Assert.That(cookie != cake, Is.True);
        Assert.That(cookie == cake, Is.False);
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_NssDiffersOnlyByCase()
    {
        var lower = new Subject("recipe", "cookie");
        var upper = new Subject("recipe", "COOKIE");

        Assert.That(lower.Equals(upper), Is.False);
        Assert.That(lower != upper, Is.True);
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_ComparedWithNullOrNonUrn()
    {
        var urn = new Subject("recipe", "cookie");
        object unrelated = new();
        bool equalsUnrelated = urn.Equals(unrelated);

        Assert.That(urn.Equals((object?)null), Is.False);
        Assert.That(equalsUnrelated, Is.False);
    }

    [Test]
    public void Equals_Should_ReturnTrue_When_ComparedWithSameInstance()
    {
        var urn = new Subject("recipe", "cookie");
        var same = urn;

        Assert.That(urn.Equals(same), Is.True);
        Assert.That(urn == same, Is.True);
    }

    [Test]
    public void HashSet_Should_DeduplicateUrns_When_TheyAreEqual()
    {
        var set = new HashSet<URN>
        {
            new Subject("recipe", "cookie"),
            new Subject("RECIPE", "cookie"),
        };

        Assert.That(set, Has.Count.EqualTo(1));
    }

    [Test]
    public void Ctor_Should_Throw_When_NssContainsWhitespace()
    {
        Assert.That(() => new Subject("recipe", "choc chip"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_NssContainsAFragmentDelimiter()
    {
        Assert.That(() => new Subject("recipe", "cookie#crumb"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_NssHasIncompletePercentEncoding()
    {
        Assert.That(() => new Subject("urn:recipe:a%2"), Throws.InstanceOf<FormatException>());
    }

    [Test]
    public void Ctor_Should_Accept_PercentEncodedNss()
    {
        var urn = new Subject("urn:recipe:choc%20chip");

        Assert.That(urn.NSS, Is.EqualTo("choc%20chip"));
    }

    [Test]
    public void Ctor_Should_Throw_ArgumentNullException_When_APartIsNull()
    {
        Assert.That(() => new Subject(null!, "cookie"), Throws.InstanceOf<ArgumentNullException>());
        Assert.That(() => new Subject("recipe", null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void EqualityOperator_Should_HandleNullOperands()
    {
        var urn = new Subject("recipe", "cookie");

        Assert.That(urn == null, Is.False);
        Assert.That(null == urn, Is.False);
        Assert.That((URN?)null == (URN?)null, Is.True);
        Assert.That(urn != null, Is.True);
    }

    [Test]
    public void TryParseParts_Should_ReturnPartsForAValidUrn()
    {
        bool ok = Subject.TryParts("urn:iso3166:US", out string? nid, out string? nss);

        Assert.That(ok, Is.True);
        Assert.That(nid, Is.EqualTo("iso3166"));
        Assert.That(nss, Is.EqualTo("US"));
    }

    [Test]
    public void TryParseParts_Should_ReturnFalseForInvalidOrNullInput()
    {
        Assert.That(Subject.TryParts("not a urn", out _, out _), Is.False);
        Assert.That(Subject.TryParts(null, out string? nid, out string? nss), Is.False);
        Assert.That(nid, Is.Null);
        Assert.That(nss, Is.Null);
    }

    [Test]
    public void Ctor_Should_ComposeFromNidAndNss()
    {
        long id = Flake.NewFlake();

        var urn = new Subject("recipe", $"bongo:{id}");

        Assert.That(urn.NID, Is.EqualTo("recipe"));
        Assert.That(urn.ToString(), Is.EqualTo($"urn:recipe:bongo:{id}"));
    }

    [Test]
    public void Reference_Should_ExposeAUrnResource()
    {
        var urn = new Subject("recipe", "cookies");
        var reference = new Reference<Subject>("Chocolate Chip Cookies", urn);

        Assert.That(reference.Resource, Is.SameAs(urn));
        Assert.That(reference.DisplayName, Is.EqualTo("Chocolate Chip Cookies"));
    }

    [Test]
    public void ResourceLink_Should_ExposeAUrnResourceAndLink()
    {
        var urn = new Subject("recipe", "cookies");
        var reference = new Reference<Subject>("Chocolate Chip Cookies", urn);
        var hyperlink = new Uri("http://localhost:8080/foundation/hub");

        var link = new ResourceLink<Subject>(reference, hyperlink);

        Assert.That(link.DisplayName, Is.EqualTo("Chocolate Chip Cookies"));
        Assert.That(link.Resource, Is.SameAs(urn));
        Assert.That(link.Link, Is.EqualTo(hyperlink));
    }
}
