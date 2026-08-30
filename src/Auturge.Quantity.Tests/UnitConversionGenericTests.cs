using Auturge.Numerics;
using static Auturge.Quantity.Units;

namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers the <see cref="UnitConversion{T}"/> API surface directly (rather than only through
/// <see cref="Quantity{T}.ConvertTo"/>): the factory methods, direction handling, inversion,
/// composition, the <c>CanHandle</c> predicate, chained conversions, and value equality.
/// </summary>
[TestFixture]
public class UnitConversionGenericTests
{
    [Test]
    public void Create_FromSameTypedFactor_ConvertsAndInverts()
    {
        UnitConversion<double> cmPerInch = UnitConversion<double>.Create(Inches, Centimeters, x => x * 2.54, x => x / 2.54);

        Assert.Multiple(() =>
        {
            Assert.That(cmPerInch.Convert(2), Is.EqualTo(5.08));
            Assert.That(cmPerInch.Invert().Convert(5.08), Is.EqualTo(2).Within(1e-9));
        });
    }

    [Test]
    public void CreateFromFactor_AcceptsAFactorOfAnyNumericType()
    {
        // factor supplied as an int, target type is double
        UnitConversion<double> inchesPerFoot = UnitConversion<double>.Create(Feet, Inches, 12);

        Assert.That(inchesPerFoot.Convert(3), Is.EqualTo(36));
    }

    [Test]
    public void Invert_SwapsSourceAndTarget()
    {
        UnitConversion<double> forward = UnitConversion<double>.Create(Feet, Inches, x => x * 12, x => x / 12);

        UnitConversion<double> backward = forward.Invert();

        Assert.Multiple(() =>
        {
            Assert.That(backward.SourceUnit, Is.EqualTo(Inches));
            Assert.That(backward.TargetUnit, Is.EqualTo(Feet));
            Assert.That(backward.Convert(24), Is.EqualTo(2));
        });
    }

    [Test]
    public void CanHandle_IsTrueInEitherDirectionAndFalseOtherwise()
    {
        UnitConversion<double> conversion = UnitConversion<double>.Create(Feet, Inches, x => x * 12, x => x / 12);

        Assert.Multiple(() =>
        {
            Assert.That(conversion.CanHandle(Feet, Inches), Is.True);
            Assert.That(conversion.CanHandle(Inches, Feet), Is.True);
            Assert.That(conversion.CanHandle(Feet, Centimeters), Is.False);
        });
    }

    [Test]
    public void Multiplication_ChainsTwoConversions()
    {
        UnitConversion<double> ftToIn = UnitConversion<double>.Create(Feet, Inches, x => x * 12, x => x / 12);
        UnitConversion<double> inToCm = UnitConversion<double>.Create(Inches, Centimeters, x => x * 2.54, x => x / 2.54);

        UnitConversion<double> ftToCm = ftToIn * inToCm;

        Assert.Multiple(() =>
        {
            Assert.That(ftToCm.SourceUnit, Is.EqualTo(Feet));
            Assert.That(ftToCm.TargetUnit, Is.EqualTo(Centimeters));
            Assert.That(ftToCm.Convert(1), Is.EqualTo(30.48).Within(1e-9));
        });
    }

    [Test]
    public void ChainConstructor_ComposesAListOfConversionsEndToEnd()
    {
        List<UnitConversion<double>> chain =
        [
            UnitConversion<double>.Create(Feet, Inches, x => x * 12, x => x / 12),
            UnitConversion<double>.Create(Inches, Centimeters, x => x * 2.54, x => x / 2.54),
        ];

        UnitConversion<double> composed = new(chain);

        Assert.That(composed.Convert(1), Is.EqualTo(30.48).Within(1e-9));
    }

    [Test]
    public void Equality_IsBySourceAndTargetOnly()
    {
        UnitConversion<double> a = UnitConversion<double>.Create(Feet, Inches, x => x * 12, x => x / 12);
        UnitConversion<double> b = UnitConversion<double>.Create(Feet, Inches, x => x * 999, x => x / 999);
        UnitConversion<double> c = UnitConversion<double>.Create(Feet, Centimeters, x => x, x => x);

        Assert.Multiple(() =>
        {
            Assert.That(a.Equals(b), Is.True, "conversion lambdas are not compared");
            Assert.That(a.Equals(c), Is.False);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        });
    }

    [Test]
    public void StaticFind_LocatesARegisteredConversion_UsableInBothDirections()
    {
        bool found = UnitConversions<Number>.TryFind(Inches, Centimeters, out UnitConversion<Number>? converter);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.True);
            Assert.That(converter, Is.Not.Null);
            Assert.That((decimal)converter!.Convert(new Number(1L)), Is.EqualTo(2.54m));
            Assert.That((decimal)converter.Invert().Convert(new Number(new System.Numerics.BigInteger(254), 2)),
                Is.EqualTo(1m));
        });
    }

    [Test]
    public void StaticFind_WhenNoPathExists_ReturnsFalse()
    {
        bool found = UnitConversions<double>.TryFind(Meters, Grams, out _);

        Assert.That(found, Is.False);
    }
}
