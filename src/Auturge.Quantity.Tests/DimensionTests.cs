namespace Auturge.Quantity.Tests;

public class DimensionTests
{
    [Test]
    public void GetStaticDimensions_ReturnsTheStaticallyDefinedDimensions()
    {
        var dimensions = Dimensions.GetStaticElements();
        
        // Assert.That(dimensions.Count, Is.EqualTo(42));
    }
    
    [Test]
    public void Velocity_IsCorrect()
    {
        var dimension = Dimensions.Velocity;
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dimension.Time, Is.EqualTo(-1));
            Assert.That(dimension.Length, Is.EqualTo(1));
            Assert.That(dimension.Mass, Is.EqualTo(0));
            Assert.That(dimension.ElectricCurrent, Is.EqualTo(0));
            Assert.That(dimension.AbsoluteTemperature, Is.EqualTo(0));
            Assert.That(dimension.AmountOfSubstance, Is.EqualTo(0));
            Assert.That(dimension.LuminousIntensity, Is.EqualTo(0));
            Assert.That(dimension.Analysis, Is.EqualTo("T^-1 L"));
        }
    }
    
    [Test]
    public void Force_IsCorrect()
    {
        var dimension = Dimensions.Force;
       
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dimension.Time, Is.EqualTo(-2));
            Assert.That(dimension.Length, Is.EqualTo(1));
            Assert.That(dimension.Mass, Is.EqualTo(1));
            Assert.That(dimension.ElectricCurrent, Is.EqualTo(0));
            Assert.That(dimension.AbsoluteTemperature, Is.EqualTo(0));
            Assert.That(dimension.AmountOfSubstance, Is.EqualTo(0));
            Assert.That(dimension.LuminousIntensity, Is.EqualTo(0));
            Assert.That(dimension.Analysis, Is.EqualTo("T^-2 L M"));
        }
    }
}
