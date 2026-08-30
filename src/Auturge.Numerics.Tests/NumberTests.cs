using System.Numerics;

namespace Auturge.Numerics.Tests;

public class NumberTests
{
    // Number.CreateChecked isn't directly callable on the concrete type — INumberBase<T>.CreateChecked
    // is a static-virtual interface member reached only through a type parameter constrained to
    // INumberBase<T>, which is exactly how UnitConversion<T>.Create<TFactor> and Rational.To<T>
    // consume it. This helper exercises that same path.
    private static TSelf CreateChecked<TSelf, TOther>(TOther value)
        where TSelf : INumberBase<TSelf>
        where TOther : INumberBase<TOther>
        => TSelf.CreateChecked(value);

    [Test]
    public void CreateChecked_WhenSourceIsInt_ProducesEquivalentNumber()
    {
        // Arrange / Act
        Number result = CreateChecked<Number, int>(42);

        // Assert
        Assert.That((int)result, Is.EqualTo(42));
    }

    [Test]
    public void CreateChecked_WhenSourceIsNegativeInt_PreservesSign()
    {
        // Arrange / Act
        Number result = CreateChecked<Number, int>(-7);

        // Assert
        Assert.That((int)result, Is.EqualTo(-7));
    }

    [Test]
    public void CreateChecked_WhenSourceIsLong_ProducesEquivalentNumber()
    {
        // Arrange / Act
        Number result = CreateChecked<Number, long>(9_000_000_000L);

        // Assert
        Assert.That(result.ToInt64(), Is.EqualTo(9_000_000_000L));
    }

    [Test]
    public void CreateChecked_WhenSourceIsUlong_ProducesEquivalentNumber()
    {
        // Arrange / Act
        Number result = CreateChecked<Number, ulong>(18_000_000_000_000_000_000UL);

        // Assert
        Assert.That(result.ToUInt64(), Is.EqualTo(18_000_000_000_000_000_000UL));
    }

    [Test]
    public void CreateChecked_WhenSourceIsBigInteger_ProducesEquivalentNumber()
    {
        // Arrange
        BigInteger source = BigInteger.Pow(10, 30);

        // Act
        Number result = CreateChecked<Number, BigInteger>(source);

        // Assert
        Assert.That(result.ToString(), Is.EqualTo(source.ToString()));
    }

    [Test]
    public void CreateChecked_WhenSourceIsShort_ProducesEquivalentNumber()
    {
        // Arrange / Act
        Number result = CreateChecked<Number, short>(-123);

        // Assert
        Assert.That((int)result, Is.EqualTo(-123));
    }
}
