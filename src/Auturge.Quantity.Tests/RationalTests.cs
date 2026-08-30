using System.Numerics;
using Auturge.Quantity;

namespace Auturge.Quantity.Tests;

public class RationalTests
{
    [Test]
    public void Constructor_WhenGivenReducibleFraction_ReducesToLowestTerms()
    {
        // Arrange / Act
        var rational = new Rational(4, 8);

        // Assert
        Assert.That(rational.Numerator, Is.EqualTo((BigInteger)1));
        Assert.That(rational.Denominator, Is.EqualTo((BigInteger)2));
    }

    [Test]
    public void Constructor_WhenDenominatorIsNegative_NormalizesSignToNumerator()
    {
        // Arrange / Act
        var rational = new Rational(1, -2);

        // Assert
        Assert.That(rational.Numerator, Is.EqualTo((BigInteger)(-1)));
        Assert.That(rational.Denominator, Is.EqualTo((BigInteger)2));
    }

    [Test]
    public void Constructor_WhenDenominatorIsZero_ThrowsDivideByZeroException()
    {
        // Arrange / Act / Assert
        Assert.Throws<DivideByZeroException>(() => _ = new Rational(1, 0));
    }

    [Test]
    public void Constructor_WhenNumeratorIsZero_NormalizesDenominatorToOne()
    {
        // Arrange / Act
        var rational = new Rational(0, 5);

        // Assert
        Assert.That(rational.Denominator, Is.EqualTo((BigInteger)1));
    }

    [Test]
    public void ImplicitConversion_FromInt_ProducesWholeNumberRatio()
    {
        // Arrange / Act
        Rational rational = 12;

        // Assert
        Assert.That(rational, Is.EqualTo(new Rational(12, 1)));
    }

    [Test]
    public void ImplicitConversion_FromDouble_CapturesExactDecimalDefinition()
    {
        // Arrange / Act — the 1959 treaty pound-to-kilogram factor: an exact decimal, not
        // an approximation, so it must round-trip to precisely 45359237/100000000.
        Rational rational = 0.45359237;

        // Assert
        Assert.That(rational, Is.EqualTo(new Rational(45359237, 100000000)));
    }

    [Test]
    public void ImplicitConversion_FromLargeExponentDouble_IsExactWithNoOverflow()
    {
        // Arrange / Act
        Rational rational = Math.Pow(10, 30);

        // Assert
        Assert.That(rational, Is.EqualTo(new Rational(BigInteger.Pow(10, 30), 1)));
    }

    [Test]
    public void ImplicitConversion_FromSmallExponentDouble_IsExactWithNoUnderflow()
    {
        // Arrange / Act
        Rational rational = Math.Pow(10, -30);

        // Assert
        Assert.That(rational, Is.EqualTo(new Rational(1, BigInteger.Pow(10, 30))));
    }

    [Test]
    public void Reciprocal_WhenCalled_SwapsNumeratorAndDenominator()
    {
        // Arrange
        var rational = new Rational(3, 4);

        // Act
        Rational reciprocal = rational.Reciprocal();

        // Assert
        Assert.That(reciprocal, Is.EqualTo(new Rational(4, 3)));
    }

    [Test]
    public void Multiply_WhenBothOperandsAreFractions_ReturnsReducedProduct()
    {
        // Arrange
        var a = new Rational(2, 3);
        var b = new Rational(3, 4);

        // Act
        Rational product = a * b;

        // Assert
        Assert.That(product, Is.EqualTo(new Rational(1, 2)));
    }

    [Test]
    public void Divide_WhenDividingByAFraction_ReturnsReducedQuotient()
    {
        // Arrange
        var a = new Rational(1, 2);
        var b = new Rational(1, 4);

        // Act
        Rational quotient = a / b;

        // Assert
        Assert.That(quotient, Is.EqualTo(new Rational(2, 1)));
    }

    [Test]
    public void Add_WhenDenominatorsDiffer_ReturnsCommonDenominatorSum()
    {
        // Arrange
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);

        // Act
        Rational sum = a + b;

        // Assert
        Assert.That(sum, Is.EqualTo(new Rational(5, 6)));
    }

    [Test]
    public void Subtract_WhenDenominatorsDiffer_ReturnsCommonDenominatorDifference()
    {
        // Arrange
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);

        // Act
        Rational difference = a - b;

        // Assert
        Assert.That(difference, Is.EqualTo(new Rational(1, 6)));
    }

    [Test]
    public void UnaryNegation_WhenApplied_FlipsSignOfNumerator()
    {
        // Arrange
        var rational = new Rational(3, 4);

        // Act
        Rational negated = -rational;

        // Assert
        Assert.That(negated, Is.EqualTo(new Rational(-3, 4)));
    }

    [Test]
    public void EqualityOperator_WhenValuesAreEquivalentButUnreduced_ReturnsTrue()
    {
        // Arrange
        var a = new Rational(1, 2);
        var b = new Rational(2, 4);

        // Act / Assert
        Assert.That(a == b, Is.True);
    }

    [Test]
    public void ComparisonOperators_WhenComparingDifferentFractions_OrderCorrectly()
    {
        // Arrange
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);

        // Act / Assert
        Assert.That(a < b, Is.True);
        Assert.That(b > a, Is.True);
    }

    [Test]
    public void ExplicitDoubleConversion_WhenApplied_ReturnsApproximateValue()
    {
        // Arrange
        var rational = new Rational(1, 4);

        // Act
        double asDouble = (double)rational;

        // Assert
        Assert.That(asDouble, Is.EqualTo(0.25));
    }

    [Test]
    public void To_WhenTargetIsDecimal_BridgesViaGenericMath()
    {
        // Arrange
        var rational = new Rational(1, 4);

        // Act
        decimal asDecimal = rational.To<decimal>();

        // Assert
        Assert.That(asDecimal, Is.EqualTo(0.25m));
    }

    [Test]
    public void FromDecimal_WhenGivenScaledValue_IsExact()
    {
        // Arrange / Act
        Rational rational = Rational.FromDecimal(0.125m);

        // Assert
        Assert.That(rational, Is.EqualTo(new Rational(125, 1000)));
    }

    [Test]
    public void ToString_WhenDenominatorIsOne_OmitsSlash()
    {
        // Arrange
        var rational = new Rational(5, 1);

        // Act / Assert
        Assert.That(rational.ToString(), Is.EqualTo("5"));
    }

    [Test]
    public void ToString_WhenDenominatorIsNotOne_UsesSlashFormat()
    {
        // Arrange
        var rational = new Rational(1, 4);

        // Act / Assert
        Assert.That(rational.ToString(), Is.EqualTo("1/4"));
    }
}
