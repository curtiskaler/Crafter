using static Auturge.Quantity.Dimensions;
using static Auturge.Quantity.Units;

namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="Unit"/> behaviour beyond the static library spot-checks in
/// <see cref="UnitTests"/>: the identity unit, base/derived classification, the SI-prefix and
/// multiply / divide / reciprocal operators, equality, synonyms, and rendering.
/// </summary>
[TestFixture]
public class UnitOperationTests
{
    [Test]
    public void One_IsDimensionlessAndItsOwnBase()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Unit.One.IsBase, Is.True);
            Assert.That(Unit.One.Symbol, Is.EqualTo("1"));
            Assert.That(Unit.One.Dimension, Is.EqualTo(Dimensions.One));
        });
    }

    [Test]
    public void IsBase_DistinguishesBaseUnitsFromPrefixedOnes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Meters.IsBase, Is.True);
            Assert.That(Kilometers.IsBase, Is.False);
            Assert.That(Kilometers.Base, Is.EqualTo(Meters));
        });
    }

    [Test]
    public void SIPrefixOperator_ProducesAScaledDerivedUnit()
    {
        Unit kilometre = SIPrefixes.Kilo * Meters;

        Assert.Multiple(() =>
        {
            Assert.That(kilometre.DisplayName, Is.EqualTo("kilometers"));
            Assert.That(kilometre.Symbol, Is.EqualTo("km"));
            Assert.That(kilometre.Dimension, Is.EqualTo(Length));
            Assert.That(kilometre.ToBase, Is.EqualTo(new Rational(1000, 1)));
        });
    }

    [Test]
    public void MultiplyOperator_CombinesDimensionsAndDefinitions()
    {
        Unit area = Meters * Meters;

        Assert.Multiple(() =>
        {
            Assert.That(area.Dimension, Is.EqualTo(Area));
            Assert.That(area.Definition[Meters], Is.EqualTo(2));
        });
    }

    [Test]
    public void DivideOperator_CombinesDimensionsAndDefinitions()
    {
        Unit speed = Meters / Seconds;

        Assert.Multiple(() =>
        {
            Assert.That(speed.Dimension, Is.EqualTo(Velocity));
            Assert.That(speed.Definition[Meters], Is.EqualTo(1));
            Assert.That(speed.Definition[Seconds], Is.EqualTo(-1));
        });
    }

    [Test]
    public void ReciprocalOperator_InvertsTheDefinition()
    {
        Unit perSecond = Seconds.Reciprocal();

        Assert.That(perSecond.Definition[Seconds], Is.EqualTo(-1));
    }

    [Test]
    public void ReciprocalViaOneOverUnit_MatchesReciprocalMethod()
    {
        Unit perSecond = 1.0 / Seconds;

        Assert.That(perSecond.Definition[Seconds], Is.EqualTo(-1));
    }

    [Test]
    public void MultiplyOperator_WithNullOperand_Throws()
    {
        Assert.That(() => { Unit _ = Meters * (Unit?)null; }, Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void Equality_IsByIdentityThenByDimensionAndDefinition()
    {
        Unit alsoKilometers = SIPrefixes.Kilo * Meters;

        Assert.Multiple(() =>
        {
            Assert.That(alsoKilometers == Kilometers, Is.True);
            Assert.That(Meters != Seconds, Is.True);
            Assert.That(Meters.Equals((object)Meters), Is.True);
            Assert.That(Meters.Equals("m"), Is.False);
        });
    }

    [Test]
    public void GetHashCode_Should_MatchTheEqualUnit_When_TheyDifferOnlyByLabel()
    {
        var relabelled = new Unit(Meters, "metres", "metre");

        Assert.Multiple(() =>
        {
            Assert.That(relabelled, Is.EqualTo(Meters));
            Assert.That(relabelled.GetHashCode(), Is.EqualTo(Meters.GetHashCode()));
        });
    }

    [Test]
    public void GetHashCode_Should_LetAnEqualUnitResolveTheSameKey_When_UsedInADictionary()
    {
        var relabelled = new Unit(Meters, "metres", "metre");
        var lookup = new Dictionary<Unit, string> { [Meters] = "length" };

        Assert.That(lookup[relabelled], Is.EqualTo("length"));
    }

    [Test]
    public void Distinct_Should_CollapseEqualUnits_When_TheyDifferOnlyByLabel()
    {
        var relabelled = new Unit(Meters, "metres", "metre");

        List<Unit> distinct = new List<Unit> { Meters, relabelled }.Distinct().ToList();

        Assert.That(distinct, Has.Count.EqualTo(1));
    }

    [Test]
    public void AddSynonym_AppendsAndReturnsSameInstance()
    {
        var unit = new Unit("smoots", "smoot", Length);

        Unit returned = unit.AddSynonym("Oxford unit", "ox");

        Assert.Multiple(() =>
        {
            Assert.That(returned, Is.SameAs(unit));
            Assert.That(unit.Synonyms.Single().Symbol, Is.EqualTo("ox"));
        });
    }

    [Test]
    public void ToString_RendersDisplayNameAndSymbol()
    {
        Assert.That(Meters.ToString(), Is.EqualTo("meters (m)"));
    }
}
