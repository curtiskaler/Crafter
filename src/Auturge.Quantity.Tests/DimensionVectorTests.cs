namespace Auturge.Quantity.Tests;

/// <summary>
/// Covers <see cref="DimensionVector"/>: the seven SI base-quantity exponents, the
/// <see cref="DimensionVector.Analysis"/> rendering, value equality, and the
/// multiply / divide / reciprocal algebra used to derive compound dimensions.
/// </summary>
[TestFixture]
public class DimensionVectorTests
{
    [Test]
    public void Constructor_StoresEveryExponent()
    {
        var vector = new DimensionVector(1, 2, 3, 4, 5, 6, 7);

        Assert.Multiple(() =>
        {
            Assert.That(vector.Time, Is.EqualTo(1));
            Assert.That(vector.Length, Is.EqualTo(2));
            Assert.That(vector.Mass, Is.EqualTo(3));
            Assert.That(vector.ElectricCurrent, Is.EqualTo(4));
            Assert.That(vector.AbsoluteTemperature, Is.EqualTo(5));
            Assert.That(vector.AmountOfSubstance, Is.EqualTo(6));
            Assert.That(vector.LuminousIntensity, Is.EqualTo(7));
        });
    }

    [Test]
    public void One_IsTheDimensionlessVector()
    {
        Assert.Multiple(() =>
        {
            Assert.That(DimensionVector.One, Is.EqualTo(new DimensionVector(0, 0, 0, 0, 0, 0, 0)));
            Assert.That(DimensionVector.One.Analysis, Is.Empty);
        });
    }

    [Test]
    public void Analysis_OmitsZeroExponentsAndShowsPowersOnlyWhenNotOne()
    {
        Assert.Multiple(() =>
        {
            Assert.That(new DimensionVector(0, 1, 0, 0, 0, 0, 0).Analysis, Is.EqualTo("L"));
            Assert.That(new DimensionVector(-1, 1, 0, 0, 0, 0, 0).Analysis, Is.EqualTo("T^-1 L"));
            Assert.That(new DimensionVector(-2, 1, 1, 0, 0, 0, 0).Analysis, Is.EqualTo("T^-2 L M"));
        });
    }

    [Test]
    public void Equality_IsByValueAcrossAllComponents()
    {
        var a = new DimensionVector(-2, 1, 1, 0, 0, 0, 0);
        var b = new DimensionVector(-2, 1, 1, 0, 0, 0, 0);
        var c = new DimensionVector(-2, 1, 0, 0, 0, 0, 0);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a == b, Is.True);
            Assert.That(a != c, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        });
    }

    [Test]
    public void EqualityOperator_HandlesNullOperands()
    {
        DimensionVector? nothing = null;

        Assert.Multiple(() =>
        {
            Assert.That(nothing == null, Is.True);
            Assert.That(DimensionVector.One == null, Is.False);
            Assert.That(null == DimensionVector.One, Is.False);
        });
    }

    [Test]
    public void Multiplication_AddsExponents()
    {
        DimensionVector length = new(0, 1, 0, 0, 0, 0, 0);

        DimensionVector area = length * length;

        Assert.That(area, Is.EqualTo(new DimensionVector(0, 2, 0, 0, 0, 0, 0)));
    }

    [Test]
    public void Division_SubtractsExponents()
    {
        DimensionVector length = new(0, 1, 0, 0, 0, 0, 0);
        DimensionVector time = new(1, 0, 0, 0, 0, 0, 0);

        DimensionVector velocity = length / time;

        Assert.That(velocity, Is.EqualTo(new DimensionVector(-1, 1, 0, 0, 0, 0, 0)));
    }

    [Test]
    public void Reciprocal_NegatesEveryExponent()
    {
        var vector = new DimensionVector(-1, 2, -3, 0, 0, 0, 0);

        DimensionVector reciprocal = vector.Reciprocal();

        Assert.Multiple(() =>
        {
            Assert.That(reciprocal, Is.EqualTo(new DimensionVector(1, -2, 3, 0, 0, 0, 0)));
            Assert.That(reciprocal.Reciprocal(), Is.EqualTo(vector));
        });
    }

    [Test]
    public void MultiplyThenDivideBySameVector_ReturnsOriginal()
    {
        var start = new DimensionVector(1, 1, 1, 0, 0, 0, 0);
        var factor = new DimensionVector(2, -1, 0, 3, 0, 0, 0);

        Assert.That(start * factor / factor, Is.EqualTo(start));
    }
}
