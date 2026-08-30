using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Covers <see cref="Number"/>'s conversions out to the BCL numeric types: the
/// <see cref="IConvertible"/> surface, <c>ToType</c>, the cast operators, the "smallest type
/// that fits" analysis, and the <c>ConvertsTo</c> / lossy-conversion guard rails.
/// </summary>
[TestFixture]
public class NumberConversionTests
{
    private static Number N(long significand, int offset) => new(new BigInteger(significand), offset);

    [Test]
    public void ToInt32AndToInt64_OnIntegralValue_ReturnExactValue()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(1234L).ToInt32(), Is.EqualTo(1234));
            Assert.That(new Number(-9_000_000_000L).ToInt64(), Is.EqualTo(-9_000_000_000L));
            Assert.That(new Number(1234L).ToType<int>(), Is.EqualTo(1234));
        });
    }

    [Test]
    public void ToDoubleAndToDecimal_OnFractionalValue_ReturnApproxValue()
    {
        Assert.Multiple(() =>
        {
            Assert.That(N(125, 2).ToDouble(), Is.EqualTo(1.25));
            Assert.That(N(125, 2).ToDecimal(), Is.EqualTo(1.25m));
            Assert.That(N(125, 2).ToSingle(), Is.EqualTo(1.25f));
        });
    }

    [Test]
    public void ToType_FromFractionalValueToIntegralType_ThrowsInvalidCast()
    {
        Assert.That(() => N(15, 1).ToInt32(), Throws.InstanceOf<InvalidCastException>());
    }

    [Test]
    public void ToType_FromNegativeValueToUnsignedType_ThrowsInvalidCast()
    {
        Assert.That(() => new Number(-5L).ToType(typeof(uint)), Throws.InstanceOf<InvalidCastException>());
    }

    [Test]
    public void ToType_WhenValueOverflowsTarget_ThrowsInvalidCast()
    {
        Assert.That(() => new Number(300L).ToType(typeof(byte)), Throws.InstanceOf<InvalidCastException>());
    }

    [Test]
    public void ToType_ToUnsupportedType_ThrowsInvalidCast()
    {
        Assert.That(() => new Number(5L).ToType(typeof(string)), Throws.InstanceOf<InvalidCastException>());
    }

    [Test]
    public void ToType_ToObjectOrNumber_ReturnsSelf()
    {
        Number value = N(42, 1);

        Assert.Multiple(() =>
        {
            Assert.That(value.ToType(typeof(object)), Is.EqualTo(value));
            Assert.That(value.ToType(typeof(Number)), Is.EqualTo(value));
        });
    }

    [Test]
    public void ToBoolean_IsTrueForNonZeroAndFalseForZero()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(3L).ToBoolean(), Is.True);
            Assert.That(Number.Zero.ToBoolean(), Is.False);
        });
    }

    [Test]
    public void SmallestType_PicksTheNarrowestIntegerTypeThatFits()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(5L).SmallestType, Is.EqualTo(typeof(sbyte)));
            Assert.That(new Number(200L).SmallestType, Is.EqualTo(typeof(byte)));
            Assert.That(new Number(40000L).SmallestType, Is.EqualTo(typeof(ushort)));
            Assert.That(new Number(-40000L).SmallestType, Is.EqualTo(typeof(int)));
            Assert.That(new Number(BigInteger.Pow(10, 40)).SmallestType, Is.EqualTo(typeof(BigInteger)));
        });
    }

    [Test]
    public void ConvertsTo_ReflectsWhetherTheValueFitsLosslessly()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(5L).ConvertsTo(typeof(int)), Is.True);
            Assert.That(new Number(300L).ConvertsTo(typeof(byte)), Is.False);
            Assert.That(N(15, 1).ConvertsTo(typeof(int)), Is.False, "1.5 is not integral");
            Assert.That(new Number(5L).ConvertsTo(typeof(string)), Is.False);
        });
    }

    [Test]
    public void ExplicitCastOperators_FloorBeforeNarrowing()
    {
        Assert.Multiple(() =>
        {
            Assert.That((byte)N(2555, 1), Is.EqualTo((byte)255));
            Assert.That((uint)N(425, 2), Is.EqualTo(4u));
            Assert.That((ulong)new Number(9L), Is.EqualTo(9ul));
        });
    }

    [Test]
    public void CheckedCastOperator_ThrowsOnOverflow()
    {
        Assert.That(() => checked((byte)new Number(300L)), Throws.InstanceOf<OverflowException>());
    }

    [Test]
    public void ImplicitOperators_BridgeToAndFromBclTypes()
    {
        Number fromInt = 7;
        Number fromDouble = 3.5;
        int backToInt = new Number(7L);
        double backToDouble = N(7, 1);

        Assert.Multiple(() =>
        {
            Assert.That(fromInt, Is.EqualTo(new Number(7L)));
            Assert.That(fromDouble, Is.EqualTo(N(35, 1)));
            Assert.That(backToInt, Is.EqualTo(7));
            Assert.That(backToDouble, Is.EqualTo(0.7));
        });
    }

    [Test]
    public void ToSmallest_ReturnsTheValueBoxedAsItsNarrowestType()
    {
        object boxed = new Number(200L).ToSmallest();

        Assert.Multiple(() =>
        {
            Assert.That(boxed, Is.TypeOf<byte>());
            Assert.That(boxed, Is.EqualTo((byte)200));
        });
    }
}
