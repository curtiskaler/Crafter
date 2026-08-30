using static Auturge.Quantity.Dimensions;

namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="Dimension"/> beyond the static library checks in <see cref="DimensionTests"/>:
/// the multiply / divide / reciprocal operators, the "find an existing named dimension or wrap a
/// fresh one" behaviour, equality (by id or by exponent vector), synonyms, and rendering.
/// </summary>
[TestFixture]
public class DimensionOperationTests
{
    [Test]
    public void Multiplication_OfLengthByLength_ResolvesToArea()
    {
        Dimension result = Length * Length;

        Assert.That(result, Is.EqualTo(Area));
    }

    [Test]
    public void Division_OfLengthByTime_ResolvesToVelocity()
    {
        Dimension result = Length / Time;

        Assert.That(result, Is.EqualTo(Velocity));
    }

    [Test]
    public void Division_ForAnUnnamedCombination_WrapsTheVectorInANewDimension()
    {
        // There is no named dimension for L / M.
        Dimension exotic = Length / Mass;

        Assert.Multiple(() =>
        {
            Assert.That(exotic.Length, Is.EqualTo(1));
            Assert.That(exotic.Mass, Is.EqualTo(-1));
            Assert.That(exotic.Analysis, Is.EqualTo("L M^-1"));
        });
    }

    [Test]
    public void Reciprocal_OfTime_IsFrequency()
    {
        Dimension reciprocal = Time.Reciprocal();

        Assert.Multiple(() =>
        {
            Assert.That(reciprocal.Time, Is.EqualTo(-1));
            Assert.That(reciprocal, Is.EqualTo(Frequency));
        });
    }

    [Test]
    public void Equality_TreatsSameExponentVectorAsEqual()
    {
        var custom = new Dimension("my velocity", "v'", -1, 1, 0, 0, 0, 0, 0);

        Assert.Multiple(() =>
        {
            Assert.That(custom, Is.EqualTo(Velocity), "equal by exponent vector even with a different id");
            Assert.That(custom == Velocity, Is.True);
        });
    }

    [Test]
    public void Equality_TreatsDifferentExponentVectorAsNotEqual()
    {
        Assert.That(Velocity, Is.Not.EqualTo(Acceleration));
    }

    [Test]
    public void GetHashCode_Should_MatchTheEqualDimension_When_ExponentVectorsAreEqual()
    {
        var custom = new Dimension("my velocity", "v'", -1, 1, 0, 0, 0, 0, 0);

        Assert.Multiple(() =>
        {
            Assert.That(custom, Is.EqualTo(Velocity));
            Assert.That(custom.GetHashCode(), Is.EqualTo(Velocity.GetHashCode()));
        });
    }

    [Test]
    public void GetHashCode_Should_IgnoreDisplayNameAndSymbol_When_ExponentVectorsAreEqual()
    {
        var first = new Dimension("dimensions.alpha", "a", 0, 2, 0, 0, 0, 0, 0);
        var second = new Dimension("dimensions.beta", "b", 0, 2, 0, 0, 0, 0, 0);

        Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
    }

    [Test]
    public void GetHashCode_Should_LetAnEqualDimensionResolveTheSameKey_When_UsedInADictionary()
    {
        var custom = new Dimension("my velocity", "v'", -1, 1, 0, 0, 0, 0, 0);
        var lookup = new Dictionary<Dimension, string> { [Velocity] = "v" };

        Assert.That(lookup[custom], Is.EqualTo("v"));
    }

    [Test]
    public void GetHashCode_Should_CollapseVectorEqualDimensions_When_AddedToAHashSet()
    {
        var custom = new Dimension("my velocity", "v'", -1, 1, 0, 0, 0, 0, 0);
        var set = new HashSet<Dimension> { Velocity, custom };

        Assert.That(set, Has.Count.EqualTo(1));
    }

    [Test]
    public void AddSynonym_AppendsAndReturnsSameInstance()
    {
        var dimension = new Dimension("throughput", "tp", 0, 0, 0, 0, 0, 0, 0);

        Dimension returned = dimension.AddSynonym("rate", "r");

        Assert.Multiple(() =>
        {
            Assert.That(returned, Is.SameAs(dimension));
            Assert.That(dimension.Synonyms, Has.Count.EqualTo(1));
            Assert.That(dimension.Synonyms[0].DisplayName, Is.EqualTo("rate"));
        });
    }

    [Test]
    public void ToString_RendersDisplayNameAndSymbol()
    {
        Assert.That(Velocity.ToString(), Is.EqualTo("dimensions.velocity (v)"));
    }

    [Test]
    public void FindOrAdd_ReturnsTheExistingNamedDimensionForAKnownVector()
    {
        Dimension found = Dimension.FindOrAdd(new DimensionVector(-1, 1, 0, 0, 0, 0, 0));

        Assert.That(found, Is.EqualTo(Velocity));
    }
}
