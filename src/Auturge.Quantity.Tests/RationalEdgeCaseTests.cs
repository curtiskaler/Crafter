using System.Numerics;
using Auturge.Quantity;

namespace Auturge.Quantity.Tests;

/// <summary>
/// Edge cases for <see cref="Rational"/> that complement <see cref="RationalTests"/>: division
/// that produces a zero denominator, the NaN/Infinity guard on the <see cref="double"/> bridge,
/// exact capture of scientific-notation and negative decimals, and the ordering operators.
/// </summary>
[TestFixture]
public class RationalEdgeCaseTests
{
    [Test]
    public void Division_ByZeroRational_ThrowsDivideByZero()
    {
        Assert.That(() => new Rational(1, 2) / Rational.Zero, Throws.InstanceOf<DivideByZeroException>());
    }

    [Test]
    public void ImplicitFromDouble_WhenNaNOrInfinity_ThrowsArgumentException()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => { Rational _ = double.NaN; }, Throws.InstanceOf<ArgumentException>());
            Assert.That(() => { Rational _ = double.PositiveInfinity; }, Throws.InstanceOf<ArgumentException>());
            Assert.That(() => { Rational _ = double.NegativeInfinity; }, Throws.InstanceOf<ArgumentException>());
        });
    }

    [Test]
    public void ImplicitFromDouble_CapturesScientificNotationExactly()
    {
        Rational tiny = 5.29e-11;

        Assert.That(tiny, Is.EqualTo(new Rational(529, BigInteger.Pow(10, 13))));
    }

    [Test]
    public void FromDecimal_WithNegativeScaledValue_KeepsSignAndScale()
    {
        Rational value = Rational.FromDecimal(-0.125m);

        Assert.That(value, Is.EqualTo(new Rational(-125, 1000)));
    }

    [Test]
    public void Zero_IsAdditiveIdentityAndAbsorbsMultiplication()
    {
        var half = new Rational(1, 2);

        Assert.Multiple(() =>
        {
            Assert.That(half + Rational.Zero, Is.EqualTo(half));
            Assert.That(half * Rational.Zero, Is.EqualTo(Rational.Zero));
            Assert.That(Rational.Zero.Numerator, Is.EqualTo(BigInteger.Zero));
            Assert.That(Rational.Zero.Denominator, Is.EqualTo(BigInteger.One));
        });
    }

    [Test]
    public void ComparisonOperators_AreConsistentAcrossEqualValues()
    {
        var twoThirds = new Rational(2, 3);
        var alsoTwoThirds = new Rational(4, 6);

        Assert.Multiple(() =>
        {
            Assert.That(twoThirds <= alsoTwoThirds, Is.True);
            Assert.That(twoThirds >= alsoTwoThirds, Is.True);
            Assert.That(twoThirds < new Rational(1, 1), Is.True);
            Assert.That(new Rational(1, 1) > twoThirds, Is.True);
            Assert.That(new Rational(-1, 2) < Rational.Zero, Is.True);
        });
    }

    [Test]
    public void To_WhenTargetIsIntegerType_UsesIntegerDivision()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Rational(7, 2).To<int>(), Is.EqualTo(3));
            Assert.That(new Rational(1, 3).To<long>(), Is.EqualTo(0L));
        });
    }

    [Test]
    public void MultiplicativeIdentity_LeavesValueUnchanged()
    {
        var value = new Rational(9, 7);

        Assert.That(value * Rational.MultiplicativeIdentity, Is.EqualTo(value));
    }

    [Test]
    public void Reciprocal_OfNegative_KeepsSignOnNumerator()
    {
        Rational reciprocal = new Rational(-3, 4).Reciprocal();

        Assert.That(reciprocal, Is.EqualTo(new Rational(-4, 3)));
    }
}
