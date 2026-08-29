namespace Auturge.Numerics.Tests;

public class FractionTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Fraction_Test()
    {
        int numerator = 12;
        int denominator = 293;
        decimal num = (decimal)numerator / denominator;
        
        var fraction = new Fraction<decimal>(num);
        
        Assert.That((int)fraction.Numerator, Is.EqualTo(numerator));
        Assert.That((int)fraction.Denominator, Is.EqualTo(denominator));
    }
}
