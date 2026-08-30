using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers the <see cref="INumberBase{TSelf}"/> / <see cref="ISignedNumber{TSelf}"/> surface that
/// makes <see cref="Number"/> usable from generic-math code: the classification predicates,
/// <c>Abs</c>, the magnitude selectors, the identities, and the generic <c>Create*</c> bridges.
/// </summary>
[TestFixture]
public class NumberBaseApiTests
{
    private static Number N(long significand, int offset) => new(new BigInteger(significand), offset);

    private static TSelf CreateChecked<TSelf, TOther>(TOther value)
        where TSelf : INumberBase<TSelf>
        where TOther : INumberBase<TOther>
        => TSelf.CreateChecked(value);

    private static TSelf CreateSaturating<TSelf, TOther>(TOther value)
        where TSelf : INumberBase<TSelf>
        where TOther : INumberBase<TOther>
        => TSelf.CreateSaturating(value);

    [Test]
    public void Radix_IsBaseTen()
    {
        Assert.That(Number.Radix, Is.EqualTo(10));
    }

    [Test]
    public void Abs_ReturnsMagnitude()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.Abs(new Number(-7L)), Is.EqualTo(new Number(7L)));
            Assert.That(Number.Abs(new Number(7L)), Is.EqualTo(new Number(7L)));
            Assert.That(Number.Abs(N(-25, 1)), Is.EqualTo(N(25, 1)));
        });
    }

    [Test]
    public void IntegerPredicates_ClassifyValuesCorrectly()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.IsInteger(new Number(4L)), Is.True);
            Assert.That(Number.IsInteger(N(45, 1)), Is.False);
            Assert.That(Number.IsEvenInteger(new Number(4L)), Is.True);
            Assert.That(Number.IsEvenInteger(new Number(5L)), Is.False);
            Assert.That(Number.IsOddInteger(new Number(5L)), Is.True);
            Assert.That(Number.IsOddInteger(new Number(4L)), Is.False);
            Assert.That(Number.IsOddInteger(N(45, 1)), Is.False, "non-integers are never odd");
        });
    }

    [Test]
    public void SignPredicates_MatchSign()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.IsZero(Number.Zero), Is.True);
            Assert.That(Number.IsZero(new Number(1L)), Is.False);
            Assert.That(Number.IsPositive(new Number(1L)), Is.True);
            Assert.That(Number.IsPositive(new Number(-1L)), Is.False);
            Assert.That(new Number(-1L).IsNegative, Is.True);
        });
    }

    [Test]
    public void IsFinite_IsAlwaysTrue()
    {
        Assert.That(Number.IsFinite(N(123456789, 3)), Is.True);
    }

    [Test]
    public void MaxMagnitude_PicksLargerAbsoluteValue_TieGoesToPositive()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.MaxMagnitude(new Number(-5L), new Number(3L)), Is.EqualTo(new Number(-5L)));
            Assert.That(Number.MaxMagnitude(new Number(-5L), new Number(5L)), Is.EqualTo(new Number(5L)));
        });
    }

    [Test]
    public void MinMagnitude_PicksSmallerAbsoluteValue_TieGoesToNegative()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.MinMagnitude(new Number(-5L), new Number(3L)), Is.EqualTo(new Number(3L)));
            Assert.That(Number.MinMagnitude(new Number(-5L), new Number(5L)), Is.EqualTo(new Number(-5L)));
        });
    }

    [Test]
    public void Identities_AreZeroAndOne()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.AdditiveIdentity, Is.EqualTo(Number.Zero));
            Assert.That(Number.MultiplicativeIdentity, Is.EqualTo(Number.One));
            Assert.That(N(123, 2) + Number.AdditiveIdentity, Is.EqualTo(N(123, 2)));
            Assert.That(N(123, 2) * Number.MultiplicativeIdentity, Is.EqualTo(N(123, 2)));
        });
    }

    [Test]
    public void CreateChecked_BridgesFromOtherNumericTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CreateChecked<Number, int>(-7), Is.EqualTo(new Number(-7L)));
            Assert.That(CreateChecked<Number, long>(9_000_000_000L), Is.EqualTo(new Number(9_000_000_000L)));
            Assert.That(CreateChecked<Number, BigInteger>(BigInteger.Pow(10, 30)),
                Is.EqualTo(new Number(BigInteger.Pow(10, 30))));
        });
    }

    [Test]
    public void CreateSaturating_FromNumberToNarrowType_ClampsToRange()
    {
        Assert.Multiple(() =>
        {
            Assert.That(CreateSaturating<byte, Number>(new Number(300L)), Is.EqualTo((byte)255));
            Assert.That(CreateSaturating<byte, Number>(new Number(-4L)), Is.EqualTo((byte)0));
            Assert.That(CreateSaturating<byte, Number>(new Number(100L)), Is.EqualTo((byte)100));
        });
    }

    [Test]
    public void CreateChecked_FromNumberThatOverflowsTarget_Throws()
    {
        Assert.That(() => CreateChecked<byte, Number>(new Number(300L)), Throws.InstanceOf<OverflowException>());
    }
}
