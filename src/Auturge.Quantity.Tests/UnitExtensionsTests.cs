using static Auturge.Quantity.Units;

namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="UnitExtensions"/>: splitting a compound unit's definition into its numerator
/// and denominator unit lists, with <see cref="Unit.One"/> standing in when a side is empty.
/// </summary>
[TestFixture]
public class UnitExtensionsTests
{
    [Test]
    public void GetNumeratorUnits_ReturnsUnitsWithPositiveExponents()
    {
        List<Unit> numerator = MetersPerSecond.GetNumeratorUnits();

        Assert.That(numerator, Is.EquivalentTo(new[] { Meters }));
    }

    [Test]
    public void GetDenominatorUnits_ReturnsUnitsWithNegativeExponents()
    {
        List<Unit> denominator = MetersPerSecond.GetDenominatorUnits();

        Assert.That(denominator, Is.EquivalentTo(new[] { Seconds }));
    }

    [Test]
    public void GetDenominatorUnits_WhenUnitHasNoDenominator_FallsBackToUnitOne()
    {
        Assert.That(Meters.GetDenominatorUnits(), Is.EqualTo(new[] { Unit.One }));
    }

    [Test]
    public void GetNumeratorUnits_ForAPlainBaseUnit_IsThatUnit()
    {
        Assert.That(Kilograms.GetNumeratorUnits(), Is.EquivalentTo(new[] { Kilograms }));
    }

    [Test]
    public void Numerator_And_Denominator_ForForce_MatchItsDefinition()
    {
        // Newtons == kg·m·s⁻²
        Assert.Multiple(() =>
        {
            Assert.That(Newtons.GetNumeratorUnits(), Is.EquivalentTo(new[] { Kilograms, Meters }));
            Assert.That(Newtons.GetDenominatorUnits(), Is.EquivalentTo(new[] { Seconds }));
        });
    }
}
