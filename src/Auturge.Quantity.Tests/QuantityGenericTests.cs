using Auturge.Quantity.Exceptions;
using static Auturge.Quantity.Units;

namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="Quantity{T}"/> — an amount paired with a <see cref="Unit"/> — using
/// <see cref="double"/> as the backing type: construction, equality, the arithmetic operators
/// and their compatibility guards, and dimensioned unit conversion.
/// </summary>
[TestFixture]
public class QuantityGenericTests
{
    [Test]
    [SetCulture("en-US")]
    public void Constructor_ExposesAmountAndUnit()
    {
        var quantity = new Quantity<double>(2.5, Meters);

        Assert.Multiple(() =>
        {
            Assert.That(quantity.Amount, Is.EqualTo(2.5));
            Assert.That(quantity.Unit, Is.EqualTo(Meters));
            Assert.That(quantity.ToString(), Is.EqualTo("2.5 m"));
        });
    }

    [Test]
    public void Equality_RequiresSameAmountAndSameUnit()
    {
        var a = new Quantity<double>(3, Meters);
        var b = new Quantity<double>(3, Meters);
        var differentUnit = new Quantity<double>(3, Seconds);
        var differentAmount = new Quantity<double>(4, Meters);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a == b, Is.True);
            Assert.That(a != differentUnit, Is.True);
            Assert.That(a != differentAmount, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        });
    }

    [Test]
    public void Addition_OfSameUnit_AddsAmounts()
    {
        var sum = new Quantity<double>(3, Meters) + new Quantity<double>(4, Meters);

        Assert.That(sum, Is.EqualTo(new Quantity<double>(7, Meters)));
    }

    [Test]
    public void AdditionAndSubtraction_WithBareScalar_AdjustTheAmountOnly()
    {
        var quantity = new Quantity<double>(10, Meters);

        Assert.Multiple(() =>
        {
            Assert.That(quantity + 5.0, Is.EqualTo(new Quantity<double>(15, Meters)));
            Assert.That(quantity - 5.0, Is.EqualTo(new Quantity<double>(5, Meters)));
        });
    }

    [Test]
    public void Addition_OfDifferentDimensions_ThrowsIncompatibleUnitType()
    {
        Assert.That(() => new Quantity<double>(1, Meters) + new Quantity<double>(1, Seconds),
            Throws.InstanceOf<IncompatibleUnitTypeException>());
    }

    [Test]
    public void Addition_OfSameDimensionButDifferentUnit_ThrowsIncompatibleUnit()
    {
        Assert.That(() => new Quantity<double>(1, Meters) + new Quantity<double>(1, Kilometers),
            Throws.InstanceOf<IncompatibleUnitException>());
    }

    [Test]
    public void Multiplication_OfTwoQuantities_MultipliesAmountsAndUnits()
    {
        var area = new Quantity<double>(3, Meters) * new Quantity<double>(4, Meters);

        Assert.Multiple(() =>
        {
            Assert.That(area.Amount, Is.EqualTo(12));
            Assert.That(area.Unit.Dimension, Is.EqualTo(Dimensions.Area));
        });
    }

    [Test]
    public void ScalarMultiplicationAndDivision_ScaleTheAmount()
    {
        var quantity = new Quantity<double>(10, Meters);

        Assert.Multiple(() =>
        {
            Assert.That((quantity * 3.0).Amount, Is.EqualTo(30));
            Assert.That((quantity / 4.0).Amount, Is.EqualTo(2.5));
        });
    }

    [Test]
    public void ConvertTo_SameDimension_ScalesTheAmount()
    {
        var kilometres = new Quantity<double>(2, Kilometers);

        Quantity<double> metres = kilometres.ConvertTo(Meters);

        Assert.Multiple(() =>
        {
            Assert.That(metres.Amount, Is.EqualTo(2000));
            Assert.That(metres.Unit, Is.EqualTo(Meters));
        });
    }

    [Test]
    public void ConvertTo_DifferentDimension_ThrowsArgumentException()
    {
        var quantity = new Quantity<double>(1, Meters);

        Assert.That(() => quantity.ConvertTo(Seconds), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public void ConvertTo_RoundTrip_RecoversTheOriginalAmount()
    {
        var start = new Quantity<double>(5, Meters);

        Quantity<double> roundTrip = start.ConvertTo(Centimeters).ConvertTo(Meters);

        Assert.That(roundTrip.Amount, Is.EqualTo(5).Within(1e-9));
    }
}
