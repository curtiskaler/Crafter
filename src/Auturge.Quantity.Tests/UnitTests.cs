using static Auturge.Quantity.Dimensions;
using static Auturge.Quantity.Units;

namespace Auturge.Quantity.Tests;

public class UnitTests
{
    [Test]
    public void Newtons_IsCorrect()
    {
        var unit = Newtons;

        Assert.That(unit.DisplayName, Is.EqualTo("Newtons"));
        Assert.That(unit.Symbol, Is.EqualTo("N"));

        Assert.That(unit.Base, Is.Null);
        Assert.That((double)unit.Factor, Is.EqualTo(1.0));
        Assert.That((double)unit.Divisor, Is.EqualTo(1.0));

        Assert.That(unit.Dimension, Is.EqualTo(Force));
        Assert.That(unit.Dimension.Analysis, Is.EqualTo("T^-2 L M"));
        Assert.That(unit.Dimension.Time, Is.EqualTo(-2));
        Assert.That(unit.Dimension.Length, Is.EqualTo(1));
        Assert.That(unit.Dimension.Mass, Is.EqualTo(1));

        Assert.That(unit.Definition.Count, Is.EqualTo(3));
        Assert.That(unit.Definition.ContainsKey(Kilograms), Is.True);
        Assert.That(unit.Definition[Kilograms], Is.EqualTo(1));
        Assert.That(unit.Definition.ContainsKey(Meters), Is.True);
        Assert.That(unit.Definition[Meters], Is.EqualTo(1));
        Assert.That(unit.Definition.ContainsKey(Seconds), Is.True);
        Assert.That(unit.Definition[Seconds], Is.EqualTo(-2));

        Assert.That(unit.Definition.ToString(), Is.EqualTo("kg m s^-2"));
    }

    [Test]
    public void MetersPerSecond_IsCorrect()
    {
        var unit = MetersPerSecond;

        Assert.That(unit.DisplayName, Is.EqualTo("meters per second"));
        Assert.That(unit.Symbol, Is.EqualTo("m/s"));

        Assert.That(unit.Base, Is.Null);
        Assert.That((double)unit.Factor, Is.EqualTo(1.0));
        Assert.That((double)unit.Divisor, Is.EqualTo(1.0));
        Assert.That(unit.IsBase);

        Assert.That(unit.Dimension, Is.EqualTo(Velocity));
        Assert.That(unit.Dimension.Analysis, Is.EqualTo("T^-1 L"));
        Assert.That(unit.Dimension.Time, Is.EqualTo(-1));
        Assert.That(unit.Dimension.Length, Is.EqualTo(1));

        Assert.That(unit.Definition.Count, Is.EqualTo(2));
        Assert.That(unit.Definition.ContainsKey(Meters), Is.True);
        Assert.That(unit.Definition[Meters], Is.EqualTo(1));
        Assert.That(unit.Definition.ContainsKey(Seconds), Is.True);
        Assert.That(unit.Definition[Seconds], Is.EqualTo(-1));

        Assert.That(unit.Definition.ToString(), Is.EqualTo("m s^-1"));
    }

    [Test]
    public void Grams_IsCorrect()
    {
        var unit = Grams;

        Assert.That(unit.DisplayName, Is.EqualTo("grams"));
        Assert.That(unit.Symbol, Is.EqualTo("g"));

        Assert.That(unit.Base, Is.Null);
        Assert.That((double)unit.Factor, Is.EqualTo(1.0));
        Assert.That((double)unit.Divisor, Is.EqualTo(1.0));
        Assert.That(unit.IsBase);

        Assert.That(unit.Dimension, Is.EqualTo(Mass));
        Assert.That(unit.Dimension.Analysis, Is.EqualTo("M"));
        Assert.That(unit.Dimension.Mass, Is.EqualTo(1));

        Assert.That(unit.Definition.Count, Is.EqualTo(1));
        Assert.That(unit.Definition.ContainsKey(Grams), Is.True);
        Assert.That(unit.Definition[Grams], Is.EqualTo(1));
        Assert.That(unit.Definition.ToString(), Is.EqualTo("g"));
    }

    [Test]
    public void Kilograms_IsCorrect()
    {
        var unit = Kilograms;

        Assert.That(unit.DisplayName, Is.EqualTo("kilograms"));
        Assert.That(unit.Symbol, Is.EqualTo("kg"));

        Assert.That(unit.Base, Is.EqualTo(Grams));
        Assert.That((double)unit.Factor, Is.EqualTo(1000.0));
        Assert.That((double)unit.Divisor, Is.EqualTo(1.0));
        Assert.That(unit.IsBase, Is.False);

        Assert.That(unit.Dimension, Is.EqualTo(Mass));
        Assert.That(unit.Dimension.Analysis, Is.EqualTo("M"));
        Assert.That(unit.Dimension.Mass, Is.EqualTo(1));

        Assert.That(unit.Definition.Count, Is.EqualTo(1));
        Assert.That(unit.Definition.ContainsKey(Kilograms), Is.True);
        Assert.That(unit.Definition[Kilograms], Is.EqualTo(1));
        Assert.That(unit.Definition.ToString(), Is.EqualTo("kg"));
    }

    // [Test]
    // public void Examine_List()
    // {
    //     var list = Units.List;
    //     
    //     Assert.That(list.Count, Is.Not.EqualTo(0));
    // }
}
