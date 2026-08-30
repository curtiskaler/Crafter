using System.Globalization;
using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers the many <see cref="Number"/> constructors and the invariants every constructed
/// value must satisfy: normalized significand/offset, sign, digit count, and integrality.
/// </summary>
[TestFixture]
public class NumberConstructionTests
{
    [Test]
    public void Constructor_WhenGivenPositiveLong_HasZeroOffsetAndPositiveSign()
    {
        // Arrange / Act
        Number number = new(12345L);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(number.RawValue, Is.EqualTo((BigInteger)12345));
            Assert.That(number.DecimalOffset, Is.EqualTo(0));
            Assert.That(number.Sign, Is.EqualTo(1));
            Assert.That(number.IsNegative, Is.False);
            Assert.That(number.IsIntegral, Is.True);
            Assert.That(number.DigitCount, Is.EqualTo(5));
        });
    }

    [Test]
    public void Constructor_WhenGivenNegativeLong_StoresMagnitudeAndNegativeSign()
    {
        // Arrange / Act
        Number number = new(-42L);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(number.RawValue, Is.EqualTo((BigInteger)42), "significand is stored as magnitude");
            Assert.That(number.Sign, Is.EqualTo(-1));
            Assert.That(number.IsNegative, Is.True);
        });
    }

    [Test]
    public void Constructor_WhenSignificandHasTrailingZeros_TrimsThemAndReducesOffset()
    {
        // Arrange / Act — 15.00 == 15
        Number number = new(new BigInteger(1500), 2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(number.RawValue, Is.EqualTo((BigInteger)15));
            Assert.That(number.DecimalOffset, Is.EqualTo(0));
            Assert.That(number.DigitCount, Is.EqualTo(2));
            Assert.That(number.IsIntegral, Is.True);
        });
    }

    [Test]
    public void Constructor_WhenValueHasFractionalPart_IsNotIntegral()
    {
        // Arrange / Act — 1.23
        Number number = new(new BigInteger(123), 2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(number.DecimalOffset, Is.EqualTo(2));
            Assert.That(number.DigitCount, Is.EqualTo(3));
            Assert.That(number.IsIntegral, Is.False);
        });
    }

    [Test]
    public void Constructor_WhenSignificandIsZeroWithOffset_NormalizesToNonNegativeZero()
    {
        // Arrange / Act
        Number number = new(BigInteger.Zero, -5);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(number.Sign, Is.EqualTo(0));
            Assert.That(number.IsNegative, Is.False);
            Assert.That(number, Is.EqualTo(Number.Zero));
        });
    }

    [Test]
    public void Constructor_WhenGivenPositiveExponent_BakesItIntoSignificand()
    {
        // Arrange / Act — 25 x 10^2 has no place in the (significand, negative-exponent) shape
        Number number = new(new BigInteger(25), -2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(number.DecimalOffset, Is.EqualTo(0));
            Assert.That(number.RawValue, Is.EqualTo((BigInteger)2500));
            Assert.That(number, Is.EqualTo(new Number(2500L)));
        });
    }

    [Test]
    public void Constructor_FromIntegralPrimitives_RoundTripsThroughToInt64()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number((byte)7).ToInt64(), Is.EqualTo(7L));
            Assert.That(new Number((sbyte)-7).ToInt64(), Is.EqualTo(-7L));
            Assert.That(new Number((short)-300).ToInt64(), Is.EqualTo(-300L));
            Assert.That(new Number((ushort)300).ToInt64(), Is.EqualTo(300L));
            Assert.That(new Number(70000).ToInt64(), Is.EqualTo(70000L));
            Assert.That(new Number(70000u).ToInt64(), Is.EqualTo(70000L));
            Assert.That(new Number('A').ToInt64(), Is.EqualTo(65L));
            Assert.That(new Number(9_000_000_000L).ToInt64(), Is.EqualTo(9_000_000_000L));
            Assert.That(new Number(18_000_000_000_000_000_000UL).ToUInt64(), Is.EqualTo(18_000_000_000_000_000_000UL));
        });
    }

    [Test]
    public void Constructor_FromInt128AndUInt128_PreservesFullMagnitude()
    {
        // Arrange
        Int128 signed = Int128.MinValue;
        UInt128 unsigned = UInt128.MaxValue;

        // Act
        Number fromSigned = new(signed);
        Number fromUnsigned = new(unsigned);

        // Assert
        Assert.That(fromSigned.ToString(), Is.EqualTo(signed.ToString(CultureInfo.InvariantCulture)));
        Assert.That(fromUnsigned.ToString(), Is.EqualTo(unsigned.ToString(CultureInfo.InvariantCulture)));
    }

    [Test]
    public void Constructor_FromDecimalViaIConvertible_CapturesExactValue()
    {
        // Arrange / Act
        Number number = new((IConvertible)0.25m);

        // Assert
        Assert.That(number, Is.EqualTo(new Number(new BigInteger(25), 2)));
        Assert.That(number.IsIntegral, Is.False);
    }

    [Test]
    public void CopyConstructor_ReproducesEveryField()
    {
        // Arrange
        Number original = new(new BigInteger(-98765), 3);

        // Act
        Number copy = new(original);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(copy.RawValue, Is.EqualTo(original.RawValue));
            Assert.That(copy.DecimalOffset, Is.EqualTo(original.DecimalOffset));
            Assert.That(copy.IsNegative, Is.EqualTo(original.IsNegative));
            Assert.That(copy.Sign, Is.EqualTo(original.Sign));
            Assert.That(copy.IsIntegral, Is.EqualTo(original.IsIntegral));
            Assert.That(copy.DigitCount, Is.EqualTo(original.DigitCount));
            Assert.That(copy, Is.EqualTo(original));
        });
    }

    [Test]
    public void StaticConstants_HaveExpectedValues()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.Zero, Is.EqualTo(new Number(0L)));
            Assert.That(Number.One, Is.EqualTo(new Number(1L)));
            Assert.That(Number.Two, Is.EqualTo(new Number(2L)));
        });
    }

    [Test]
    [SetCulture("en-US")]
    public void ToString_RendersSignSeparatorAndLeadingZeroCorrectly()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(0L).ToString(), Is.EqualTo("0"));
            Assert.That(new Number(-5L).ToString(), Is.EqualTo("-5"));
            Assert.That(new Number(new BigInteger(123), 2).ToString(), Is.EqualTo("1.23"));
            Assert.That(new Number(new BigInteger(5), 3).ToString(), Is.EqualTo("0.005"));
            Assert.That(new Number(new BigInteger(-4), 1).ToString(), Is.EqualTo("-0.4"));
        });
    }
}
