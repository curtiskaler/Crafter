using Auturge.Numerics;
using Auturge.Quantity.Exceptions;
using static Auturge.Quantity.Units;

namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers the <see cref="Number"/>-backed conveniences in <c>Auturge.Quantity.Numerics</c>: the
/// non-generic <see cref="Quantity"/> (with its <see cref="int"/> conversion and operators) and
/// the <see cref="UnitConversions"/> / <see cref="SIPrefix"/> shortcuts.
/// </summary>
[TestFixture]
public class QuantityNumericsTests
{
    [Test]
    public void Constructor_FromIntAmount_UsesNumberBacking()
    {
        var quantity = new Quantity(42, Kilometers);

        Assert.Multiple(() =>
        {
            Assert.That((decimal)quantity.Amount, Is.EqualTo(42m));
            Assert.That(quantity.Unit, Is.EqualTo(Kilometers));
        });
    }

    [Test]
    public void ImplicitConversion_FromInt_ProducesADimensionlessCount()
    {
        Quantity five = 5;

        Assert.Multiple(() =>
        {
            Assert.That((decimal)five.Amount, Is.EqualTo(5m));
            Assert.That(five.Unit, Is.EqualTo(Each));
        });
    }

    [Test]
    public void Addition_OfSameUnit_AddsAmountsExactly()
    {
        var sum = new Quantity(new Number(new System.Numerics.BigInteger(1), 1), Meters)
                  + new Quantity(new Number(new System.Numerics.BigInteger(2), 1), Meters);

        // 0.1 m + 0.2 m == 0.3 m, with no floating-point drift
        Assert.That(sum.Amount, Is.EqualTo(new Number(new System.Numerics.BigInteger(3), 1)));
    }

    [Test]
    public void Addition_OfMismatchedDimensions_Throws()
    {
        Assert.That(() => new Quantity(1, Meters) + new Quantity(1, Seconds),
            Throws.InstanceOf<IncompatibleUnitTypeException>());
    }

    [Test]
    public void ScalarOperators_AdjustTheAmount()
    {
        var quantity = new Quantity(10, Meters);

        Assert.Multiple(() =>
        {
            Assert.That((decimal)(quantity + new Number(5L)).Amount, Is.EqualTo(15m));
            Assert.That((decimal)(quantity * new Number(3L)).Amount, Is.EqualTo(30m));
            Assert.That((decimal)(quantity / new Number(4L)).Amount, Is.EqualTo(2.5m));
        });
    }

    [Test]
    public void Equality_ComparesAmountAndUnit()
    {
        var threeMetres = new Quantity(3, Meters);
        var alsoThreeMetres = new Quantity(3, Meters);
        var threeSeconds = new Quantity(3, Seconds);

        Assert.Multiple(() =>
        {
            Assert.That(threeMetres, Is.EqualTo(alsoThreeMetres));
            Assert.That(threeMetres == alsoThreeMetres, Is.True);
            Assert.That(threeMetres != threeSeconds, Is.True);
        });
    }

    [Test]
    public void ConvertTo_PreservesPrecisionOnRoundTrip()
    {
        var start = new Quantity(1, MetersPerSecond);

        Quantity<Number> feetPerSecond = start.ConvertTo(FtPerSecond);
        Quantity<Number> backToStart = feetPerSecond.ConvertTo(MetersPerSecond);

        // ConvertedQuantity reverts along the conversion chain rather than re-deriving, so the
        // original amount comes back exactly.
        Assert.That(backToStart.Amount, Is.EqualTo(start.Amount));
    }

    [Test]
    public void UnitConversions_NonGeneric_IsTheNumberSpecialisation()
    {
        bool found = UnitConversions.TryFind(Inches, Centimeters, out UnitConversion<Number>? converter);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.True);
            Assert.That((decimal)converter!.Convert(new Number(1L)), Is.EqualTo(2.54m));
        });
    }

    [Test]
    public void SIPrefix_NonGeneric_BacksOntoNumber()
    {
        var kilo = new SIPrefix("kilo", "k", new Number(1000L));

        Assert.That((decimal)kilo.Factor, Is.EqualTo(1000m));
    }
}
