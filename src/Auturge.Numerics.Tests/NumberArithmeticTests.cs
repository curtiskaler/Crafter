using System.Numerics;

namespace Auturge.Numerics.Tests;

/// <summary>
/// Exercises <see cref="Number"/>'s operator set: addition, subtraction, multiplication, the
/// fixed- and variable-precision division paths, modulus, and the unary operators.
/// </summary>
[TestFixture]
public class NumberArithmeticTests
{
    private static Number N(long significand, int offset = 0) => new(new BigInteger(significand), offset);

    [Test]
    public void Addition_WhenOperandsHaveDifferentScales_AlignsBeforeAdding()
    {
        // 0.1 + 0.02 == 0.12
        Assert.That(N(1, 1) + N(2, 2), Is.EqualTo(N(12, 2)));
    }

    [Test]
    public void Addition_WhenResultCrossesZero_KeepsCorrectSign()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(-3L) + new Number(10L), Is.EqualTo(new Number(7L)));
            Assert.That(new Number(3L) + new Number(-10L), Is.EqualTo(new Number(-7L)));
            Assert.That(new Number(5L) + new Number(-5L), Is.EqualTo(Number.Zero));
        });
    }

    [Test]
    public void Subtraction_IsInverseOfAddition()
    {
        Number a = N(12345, 2);
        Number b = N(678, 1);

        Assert.That(a - b + b, Is.EqualTo(a));
    }

    [Test]
    public void Multiplication_AddsDecimalOffsetsAndCombinesSigns()
    {
        Assert.Multiple(() =>
        {
            Assert.That(N(15, 1) * N(15, 1), Is.EqualTo(N(225, 2)));          // 1.5 * 1.5 == 2.25
            Assert.That(new Number(-4L) * new Number(3L), Is.EqualTo(new Number(-12L)));
            Assert.That(new Number(-4L) * new Number(-3L), Is.EqualTo(new Number(12L)));
            Assert.That(new Number(0L) * new Number(9L), Is.EqualTo(Number.Zero));
        });
    }

    [Test]
    public void Division_WhenExact_ProducesExactQuotient()
    {
        Assert.That(new Number(10L) / new Number(2L), Is.EqualTo(new Number(5L)));
    }

    [Test]
    public void Division_WhenInexact_TruncatesToDefaultFractionalDigits()
    {
        // 1 / 3 to the default 8 fractional digits
        Assert.That(new Number(1L) / new Number(3L), Is.EqualTo(N(33333333, 8)));
    }

    [Test]
    public void Divide_WithExplicitFractionalDigits_ControlsPrecision()
    {
        Assert.That(Number.Divide(new Number(1L), new Number(3L), 3), Is.EqualTo(N(333, 3)));
    }

    [Test]
    public void Divide_WithNegativeFractionalDigits_ThrowsArgumentOutOfRange()
    {
        Assert.That(() => Number.Divide(new Number(1L), new Number(2L), -1),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Division_ByZero_ThrowsDivideByZero()
    {
        Assert.That(() => new Number(5L) / Number.Zero, Throws.InstanceOf<DivideByZeroException>());
    }

    [Test]
    public void Division_ZeroByZero_IsDefinedAsOne()
    {
        // Keeps the identity x/x == 1 total, even at x == 0.
        Assert.That(Number.Zero / Number.Zero, Is.EqualTo(Number.One));
    }

    [Test]
    public void Modulus_ReturnsRemainderWithDividendSign()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Number(7L) % new Number(3L), Is.EqualTo(new Number(1L)));
            Assert.That(new Number(10L) % new Number(5L), Is.EqualTo(Number.Zero));
            Assert.That(N(55, 1) % new Number(2L), Is.EqualTo(N(15, 1))); // 5.5 % 2 == 1.5
        });
    }

    [Test]
    public void UnaryNegation_FlipsSignWithoutChangingMagnitude()
    {
        Assert.Multiple(() =>
        {
            Assert.That(-new Number(5L), Is.EqualTo(new Number(-5L)));
            Assert.That(-new Number(-5L), Is.EqualTo(new Number(5L)));
            Assert.That(-Number.Zero, Is.EqualTo(Number.Zero));
        });
    }

    [Test]
    public void UnaryPlus_ReturnsSameValue()
    {
        Number value = N(-1234, 2);
        Assert.That(+value, Is.EqualTo(value));
    }

    [Test]
    public void IncrementAndDecrement_StepByOne()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Step(new Number(4L), increment: true), Is.EqualTo(new Number(5L)));
            Assert.That(Step(new Number(4L), increment: false), Is.EqualTo(new Number(3L)));
        });
    }

    // ++ and -- are explicit interface implementations, reachable only through a constrained
    // type parameter — the same way generic-math consumers would use them.
    private static T Step<T>(T value, bool increment)
        where T : IIncrementOperators<T>, IDecrementOperators<T>
        => increment ? ++value : --value;
}
