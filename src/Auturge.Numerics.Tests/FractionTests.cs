using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers <see cref="Fraction{T}"/>'s Stern-Brocot rational approximation: exact whole numbers,
/// proper fractions, values near an integer boundary, negatives, and the effect of the error term.
/// </summary>
[TestFixture]
public class FractionTests
{
    [Test]
    public void Constructor_WhenValueIsWholeNumber_ProducesDenominatorOfOne()
    {
        var fraction = new Fraction<decimal>(5m);

        Assert.Multiple(() =>
        {
            Assert.That(fraction.Numerator, Is.EqualTo((BigInteger)5));
            Assert.That(fraction.Denominator, Is.EqualTo((BigInteger)1));
            Assert.That(fraction.Value, Is.EqualTo(5m));
        });
    }

    [Test]
    public void Constructor_WhenValueIsZero_ProducesZeroOverOne()
    {
        var fraction = new Fraction<decimal>(0m);

        Assert.Multiple(() =>
        {
            Assert.That(fraction.Numerator, Is.EqualTo(BigInteger.Zero));
            Assert.That(fraction.Denominator, Is.EqualTo((BigInteger)1));
        });
    }

    [Test]
    public void Constructor_WhenValueIsSimpleProperFraction_RecoversTheRatio()
    {
        var fraction = new Fraction<double>(0.75);

        Assert.Multiple(() =>
        {
            Assert.That(fraction.Numerator, Is.EqualTo((BigInteger)3));
            Assert.That(fraction.Denominator, Is.EqualTo((BigInteger)4));
        });
    }

    [Test]
    public void Constructor_RecoversAnAwkwardRatioWithinTheErrorTolerance()
    {
        const int numerator = 12;
        const int denominator = 293;
        decimal value = (decimal)numerator / denominator;

        var fraction = new Fraction<decimal>(value);

        Assert.Multiple(() =>
        {
            Assert.That(fraction.Numerator, Is.EqualTo((BigInteger)numerator));
            Assert.That(fraction.Denominator, Is.EqualTo((BigInteger)denominator));
        });
    }

    [Test]
    public void Constructor_WhenValueIsJustBelowAnInteger_SnapsToThatIntegerWithLooseError()
    {
        var fraction = new Fraction<decimal>(2.9999999999m, error: 0.001m);

        Assert.Multiple(() =>
        {
            Assert.That(fraction.Numerator, Is.EqualTo((BigInteger)3));
            Assert.That(fraction.Denominator, Is.EqualTo((BigInteger)1));
        });
    }

    [Test]
    public void Constructor_WhenValueIsNegative_KeepsSignOnNumerator()
    {
        var fraction = new Fraction<decimal>(-0.5m);

        Assert.Multiple(() =>
        {
            Assert.That(fraction.Numerator, Is.EqualTo((BigInteger)(-1)));
            Assert.That(fraction.Denominator, Is.EqualTo((BigInteger)2));
        });
    }

    [Test]
    public void Constructor_WithMixedNumber_CombinesIntegerAndFractionalParts()
    {
        // 2.25 == 9/4
        var fraction = new Fraction<decimal>(2.25m);

        Assert.Multiple(() =>
        {
            Assert.That(fraction.Numerator, Is.EqualTo((BigInteger)9));
            Assert.That(fraction.Denominator, Is.EqualTo((BigInteger)4));
        });
    }

    [Test]
    public void Constructor_AcceptsNumberAsTheUnderlyingType()
    {
        var fraction = new Fraction<Number>(new Number(new BigInteger(2), 1)); // 0.2 == 1/5

        Assert.Multiple(() =>
        {
            Assert.That(fraction.Numerator, Is.EqualTo((BigInteger)1));
            Assert.That(fraction.Denominator, Is.EqualTo((BigInteger)5));
        });
    }
}
