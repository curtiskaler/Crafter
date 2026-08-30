using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers <see cref="Number.Round(Number, int, MidpointRounding)"/> and the
/// <see cref="NumberExtensions"/> helpers built on it (<c>Floor</c>, <c>Truncate</c>,
/// <c>TruncateTo</c>), across every <see cref="MidpointRounding"/> mode and both signs.
/// </summary>
[TestFixture]
public class NumberRoundingTests
{
    private static Number N(long significand, int offset) => new(new BigInteger(significand), offset);

    [Test]
    public void Round_AwayFromZero_RoundsHalvesOutward()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.Round(N(125, 1), 0, MidpointRounding.AwayFromZero), Is.EqualTo(new Number(13L)));
            Assert.That(Number.Round(N(-125, 1), 0, MidpointRounding.AwayFromZero), Is.EqualTo(new Number(-13L)));
            Assert.That(Number.Round(N(124, 1), 0, MidpointRounding.AwayFromZero), Is.EqualTo(new Number(12L)));
        });
    }

    [Test]
    public void Round_ToEven_RoundsHalvesToTheEvenNeighbour()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.Round(N(125, 1), 0, MidpointRounding.ToEven), Is.EqualTo(new Number(12L)));
            Assert.That(Number.Round(N(135, 1), 0, MidpointRounding.ToEven), Is.EqualTo(new Number(14L)));
            Assert.That(Number.Round(N(126, 1), 0, MidpointRounding.ToEven), Is.EqualTo(new Number(13L)));
        });
    }

    [Test]
    public void Round_ToZero_Truncates()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.Round(N(19, 1), 0, MidpointRounding.ToZero), Is.EqualTo(new Number(1L)));
            Assert.That(Number.Round(N(-19, 1), 0, MidpointRounding.ToZero), Is.EqualTo(new Number(-1L)));
        });
    }

    [Test]
    public void Round_TowardNegativeInfinity_IsFloor()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.Round(N(37, 1), 0, MidpointRounding.ToNegativeInfinity), Is.EqualTo(new Number(3L)));
            Assert.That(Number.Round(N(-32, 1), 0, MidpointRounding.ToNegativeInfinity), Is.EqualTo(new Number(-4L)));
        });
    }

    [Test]
    public void Round_TowardPositiveInfinity_IsCeiling()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.Round(N(31, 1), 0, MidpointRounding.ToPositiveInfinity), Is.EqualTo(new Number(4L)));
            Assert.That(Number.Round(N(-38, 1), 0, MidpointRounding.ToPositiveInfinity), Is.EqualTo(new Number(-3L)));
        });
    }

    [Test]
    public void Round_ToMoreDigitsThanValueHas_ReturnsValueUnchanged()
    {
        // Regression: rounding an already-shorter value to N places must not divide by zero.
        Assert.Multiple(() =>
        {
            Assert.That(Number.Round(new Number(5L), 2, MidpointRounding.AwayFromZero), Is.EqualTo(new Number(5L)));
            Assert.That(Number.Round(N(15, 1), 4, MidpointRounding.ToEven), Is.EqualTo(N(15, 1)));
        });
    }

    [Test]
    public void Round_AtExactRequestedPrecision_ReturnsSameInstanceValue()
    {
        Assert.That(Number.Round(N(1234, 2), 2, MidpointRounding.AwayFromZero), Is.EqualTo(N(1234, 2)));
    }

    [Test]
    public void Round_ToPartialPrecision_RoundsTheTrailingDigits()
    {
        // 3.14159 -> 3.14 (away from zero)
        Assert.That(Number.Round(N(314159, 5), 2, MidpointRounding.AwayFromZero), Is.EqualTo(N(314, 2)));
    }

    [Test]
    public void Round_WithNegativeDigits_ThrowsArgumentOutOfRange()
    {
        Assert.That(() => Number.Round(N(1, 1), -1, MidpointRounding.ToEven),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void FloorExtension_DropsFractionTowardNegativeInfinity()
    {
        Assert.Multiple(() =>
        {
            Assert.That(N(37, 1).Floor(), Is.EqualTo(new Number(3L)));
            Assert.That(N(-32, 1).Floor(), Is.EqualTo(new Number(-4L)));
            Assert.That(new Number(5L).Floor(), Is.EqualTo(new Number(5L)));
        });
    }

    [Test]
    public void TruncateExtension_DropsFractionTowardZero()
    {
        Assert.Multiple(() =>
        {
            Assert.That(N(37, 1).Truncate(), Is.EqualTo(new Number(3L)));
            Assert.That(N(-37, 1).Truncate(), Is.EqualTo(new Number(-3L)));
        });
    }

    [Test]
    public void TruncateTo_KeepsRequestedFractionalDigitsWithoutRounding()
    {
        Assert.Multiple(() =>
        {
            Assert.That(N(319999, 5).TruncateTo(2), Is.EqualTo(N(319, 2)));
            Assert.That(new Number(7L).TruncateTo(3), Is.EqualTo(new Number(7L)));
        });
    }
}
