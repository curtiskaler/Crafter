namespace Auturge.Quantity.Tests;

public class QuantityLengthTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CanConvert_KmToMeters()
    {
        var qt1 = new Quantity(42, Units.Kilometers);

        var qt2 = qt1.ConvertTo(Units.Meters);

        // 42km * (1000 m/km) = 42000m
        Assert.That((decimal)qt2.Amount, Is.EqualTo(42000));
        Assert.That(qt2.Unit, Is.EqualTo(Units.Meters));
    }

    [Test]
    public void CanConvert_mToKm()
    {
        var qt1 = new Quantity(42000, Units.Meters);

        var qt2 = qt1.ConvertTo(Units.Kilometers);

        // 42000m * (1/1000 km/m) = 42km
        Assert.That((decimal)qt2.Amount, Is.EqualTo(42));
        Assert.That(qt2.Unit, Is.EqualTo(Units.Kilometers));
    }

    [Test]
    public void CanConvert_ftToMeters()
    {
        var qt1 = new Quantity(1, Units.Feet);

        var qt2 = qt1.ConvertTo(Units.Meters);

        // 1ft * (12 in/ft) * (2.54 cm/in) * (1/100 cm/m) = 0.3048m
        Assert.That((decimal)qt2.Amount, Is.EqualTo(0.3048d));
        Assert.That(qt2.Unit, Is.EqualTo(Units.Meters));
    }

    [Test]
    public void CanConvert_miToMeters()
    {
        var qt1 = new Quantity(1, Units.Miles);

        var qt2 = qt1.ConvertTo(Units.Meters);

        // 1mi * (5280 ft/mi) * (12 in/ft) * (2.54 cm/in) * (1/100 cm/m) = 1609.3440m
        Assert.That((decimal)qt2.Amount, Is.EqualTo(1609.3440d));
        Assert.That(qt2.Unit, Is.EqualTo(Units.Meters));
    }

    [Test]
    public void CanConvert_cmToMeters()
    {
        var qt1 = new Quantity(150, Units.Centimeters);

        var qt2 = qt1.ConvertTo(Units.Meters);

        // 150cm * 1/100 (m/cm) = 1.50m
        Assert.That((decimal)qt2.Amount, Is.EqualTo(1.50d));
        Assert.That(qt2.Unit, Is.EqualTo(Units.Meters));
    }

    [Test]
    public void CanConvert_cmToInches()
    {
        var qt1 = new Quantity<double>(2.54d, Units.Centimeters);

        Quantity<double> qt2 = qt1.ConvertTo(Units.Inches);

        // 2.54 cm * 1/2.54 (cm/in) = 1in
        Assert.That(qt2.Amount, Is.EqualTo(1));
        Assert.That(qt2.Unit, Is.EqualTo(Units.Inches));
    }

    [Test]
    public void CanConvert_inToMeters()
    {
        var qt1 = new Quantity(150d, Units.Inches);

        var qt2 = qt1.ConvertTo(Units.Meters);

        // 150 in * 2.54 (cm/in) * 1/100 (m/cm) = 3.81m 
        Assert.That((decimal)qt2.Amount, Is.EqualTo(3.81d));
        Assert.That(qt2.Unit, Is.EqualTo(Units.Meters));
    }

    [Test]
    public void CanConvert_inToMm()
    {
        var qt1 = new Quantity(1, Units.Inches);

        var qt2 = qt1.ConvertTo(Units.Millimeters);

        // 1 in * 2.54 (cm/in) * 10 (mm/cm) = 25.40mm  
        Assert.That((decimal)qt2.Amount, Is.EqualTo(25.40d));
        Assert.That(qt2.Unit, Is.EqualTo(Units.Millimeters));
    }
}
