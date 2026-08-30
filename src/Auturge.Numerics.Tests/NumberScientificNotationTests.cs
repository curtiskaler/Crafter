using System.Globalization;
using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Regression tests for scientific-notation ("1.5E+10") parsing. Previously <see cref="Number"/>
/// only stripped the letter 'E'/'e' from the string and left the exponent digits behind, so
/// <c>double.ToString()</c> output for large/small magnitudes (e.g. "1E+30", "1E-30") parsed to
/// wildly wrong values or threw.
/// </summary>
public class NumberScientificNotationTests
{
    private static Number Parse(string s) => Number.Parse(s, CultureInfo.InvariantCulture);

    [Test]
    public void Parse_LargePositiveExponent_ShiftsDecimalPointLeft()
    {
        Number expected = new(BigInteger.Pow(10, 30));

        Assert.Multiple(() =>
        {
            Assert.That(Parse("1E+30"), Is.EqualTo(expected));
            Assert.That(Parse("1E30"), Is.EqualTo(expected));
            Assert.That(Parse("1e30"), Is.EqualTo(expected));
        });
    }

    [Test]
    public void Parse_SmallNegativeExponent_ShiftsDecimalPointRight()
    {
        Number expected = new(BigInteger.One, 30); // 1 x 10^-30

        Assert.Multiple(() =>
        {
            Assert.That(Parse("1E-30"), Is.EqualTo(expected));
            Assert.That(Parse("1e-30"), Is.EqualTo(expected));
        });
    }

    [Test]
    public void Parse_ExponentCombinesWithFractionalMantissa()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("1.5E+10"), Is.EqualTo(new Number(15_000_000_000L)));
            Assert.That(Parse("1.5E10"), Is.EqualTo(new Number(15_000_000_000L)));
            Assert.That(Parse("2.5e-3"), Is.EqualTo(Parse("0.0025")));
            Assert.That(Parse("2.5e-3"), Is.EqualTo(new Number(new BigInteger(25), 4)));
            Assert.That(Parse("1.5E-10"), Is.EqualTo(Parse("0.00000000015")));
        });
    }

    [Test]
    public void Parse_NegativeMantissaWithExponent_KeepsSign()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("-1.5E+10"), Is.EqualTo(new Number(-15_000_000_000L)));
            Assert.That(Parse("-2.5e-3"), Is.EqualTo(Parse("-0.0025")));
            Assert.That(Parse("-1E-30"), Is.EqualTo(new Number(BigInteger.MinusOne, 30)));
        });
    }

    [Test]
    public void Parse_ZeroExponent_IsIdentity()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("5E0"), Is.EqualTo(new Number(5L)));
            Assert.That(Parse("5E+0"), Is.EqualTo(new Number(5L)));
            Assert.That(Parse("1.25E0"), Is.EqualTo(Parse("1.25")));
        });
    }

    [Test]
    public void Parse_ExponentRoundTripsThroughToString()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Parse("1E+30").ToString(), Is.EqualTo(new Number(BigInteger.Pow(10, 30)).ToString()));
            Assert.That(Parse("1E-30").ToString(), Is.EqualTo(new Number(BigInteger.One, 30).ToString()));
        });
    }

    [TestCase("1E")]
    [TestCase("1E+")]
    [TestCase("1E-")]
    [TestCase("1EE5")]
    [TestCase("1E1.5")]
    [TestCase("1E2e3")]
    [TestCase("E5")]
    public void Parse_MalformedExponent_Throws(string value)
        => Assert.That(() => Number.Parse(value, CultureInfo.InvariantCulture),
            Throws.InstanceOf<ArgumentException>());

    [TestCase("1E")]
    [TestCase("1E1.5")]
    public void TryParse_MalformedExponent_ReturnsFalse(string value)
        => Assert.That(Number.TryParse(value, CultureInfo.InvariantCulture, out _), Is.False);

    // The IConvertible/double constructor round-trips through double.ToString(), which emits
    // "1E+30" / "1E-30" for these magnitudes - the exact form that used to break.
    [Test]
    public void Construct_FromDoubleMagnitudesNeedingExponentNotation_DoesNotThrow()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number((IConvertible)1e30), Is.EqualTo(new Number(BigInteger.Pow(10, 30))));
            Assert.That(new Number((IConvertible)1e-30), Is.EqualTo(new Number(BigInteger.One, 30)));
            Assert.That(new Number((IConvertible)1.5e10), Is.EqualTo(new Number(15_000_000_000L)));
        });
    }
}
