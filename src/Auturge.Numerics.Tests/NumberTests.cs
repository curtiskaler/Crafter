using System.Numerics;

namespace Auturge.Numerics.Tests;

public class NumberTests
{
    [Test]
    public void Sign_WhenValueIsPositiveInteger_ReturnsOne()
    {
        Assert.That(new Number(5L).Sign, Is.EqualTo(1));
    }

    [Test]
    public void Sign_WhenValueIsNegativeInteger_ReturnsMinusOne()
    {
        Assert.That(new Number(-5L).Sign, Is.EqualTo(-1));
    }

    [Test]
    public void Sign_WhenValueIsZero_ReturnsZero()
    {
        Assert.That(new Number(0L).Sign, Is.EqualTo(0));
        Assert.That(Number.Zero.Sign, Is.EqualTo(0));
    }

    [Test]
    public void Sign_WhenValueIsOne_ReturnsOne()
    {
        Assert.That(Number.One.Sign, Is.EqualTo(1));
    }

    [Test]
    public void Sign_WhenValueIsPositiveFraction_ReturnsOne()
    {
        Assert.That(new Number(0.25m).Sign, Is.EqualTo(1));
    }

    [Test]
    public void Sign_WhenValueIsNegativeFraction_ReturnsMinusOne()
    {
        Assert.That(new Number(-0.25m).Sign, Is.EqualTo(-1));
    }

    [Test]
    public void Sign_WhenSignificandHasExponentAndIsZero_ReturnsZeroAndNotNegative()
    {
        Number number = new(BigInteger.Zero, -3);

        Assert.That(number.Sign, Is.EqualTo(0));
        Assert.That(number.IsNegative, Is.False);
    }
}
