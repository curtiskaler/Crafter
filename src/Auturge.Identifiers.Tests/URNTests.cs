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
}
