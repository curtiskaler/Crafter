using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers ordering (<see cref="IComparable{T}"/>, comparison operators) and equality
/// (<see cref="IEquatable{T}"/>, <c>==</c>/<c>!=</c>, <see cref="object.GetHashCode"/>) for
/// <see cref="Number"/>, including the scale-independence that makes 2.50 equal 2.5.
/// </summary>
[TestFixture]
public class NumberComparisonTests
{
    private static Number N(long significand, int offset = 0) => new(new BigInteger(significand), offset);

    [Test]
    public void CompareTo_OrdersAcrossSignAndScale()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(5L).CompareTo(new Number(3L)), Is.GreaterThan(0));
            Assert.That(new Number(3L).CompareTo(new Number(5L)), Is.LessThan(0));
            Assert.That(new Number(5L).CompareTo(new Number(5L)), Is.EqualTo(0));
            Assert.That(new Number(-5L).CompareTo(new Number(3L)), Is.LessThan(0));
            Assert.That(new Number(-5L).CompareTo(new Number(-3L)), Is.LessThan(0), "-5 < -3");
            Assert.That(N(-1, 1).CompareTo(N(-2, 1)), Is.GreaterThan(0), "-0.1 > -0.2");
        });
    }

    [Test]
    public void ComparisonOperators_AgreeWithCompareTo()
    {
        Number small = N(125, 2);   // 1.25
        Number large = N(1251, 3);  // 1.251

        Assert.Multiple(() =>
        {
            Assert.That(small < large, Is.True);
            Assert.That(large > small, Is.True);
            Assert.That(small <= large, Is.True);
            Assert.That(large >= small, Is.True);
            Assert.That(small <= N(125, 2), Is.True);
            Assert.That(small >= N(125, 2), Is.True);
        });
    }

    [Test]
    public void CompareTo_Object_TreatsNullAsSmallestAndRejectsForeignTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(1L).CompareTo(null), Is.EqualTo(1));
            Assert.That(() => new Number(1L).CompareTo("not a number"),
                Throws.InstanceOf<ArgumentException>());
        });
    }

    [Test]
    public void Equality_IsScaleIndependent()
    {
        // 2.50, built two different ways, is a single value.
        Number viaTrailingZero = new(new BigInteger(250), 2);
        Number viaClean = N(25, 1);

        Assert.Multiple(() =>
        {
            Assert.That(viaTrailingZero == viaClean, Is.True);
            Assert.That(viaTrailingZero.Equals(viaClean), Is.True);
            Assert.That(viaTrailingZero.Equals((object)viaClean), Is.True);
            Assert.That(viaTrailingZero.GetHashCode(), Is.EqualTo(viaClean.GetHashCode()));
        });
    }

    [Test]
    public void Inequality_DistinguishesValueSignAndScale()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(5L) != new Number(6L), Is.True);
            Assert.That(new Number(5L) != new Number(-5L), Is.True);
            Assert.That(new Number(5L).Equals("5"), Is.False);
        });
    }

    [Test]
    public void EqualityOperators_WithPrimitiveOverloads_Work()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(5L) == 5, Is.True);
            Assert.That(new Number(5L) != 6, Is.True);
            Assert.That(new Number(5L) == 5.0, Is.True);
            Assert.That(new Number(5L) != 4.0, Is.True);
        });
    }

    [Test]
    public void Sort_UsesNaturalNumericOrder()
    {
        List<Number> values = [new(3L), N(-15, 1), new(0L), N(1, 2), new(2L)];

        values.Sort();

        Assert.That(values, Is.EqualTo(new[] { N(-15, 1), new Number(0L), N(1, 2), new Number(2L), new Number(3L) }));
    }
}
