using Auturge.Numerics;
using static Auturge.Quantity.Units;

namespace Auturge.Quantity.Tests;

public class UnitConversionTests
{
    [Test]
    public void in_To_cm()
    {
        bool found = UnitConversions
            .TryFind(Inches, Centimeters, out UnitConversion<Number>? converter);

        Assert.That(found, Is.True);
        Assert.That(converter, Is.Not.Null);

        Number cm = converter.Convert(1.0);

        Assert.That((decimal)cm, Is.EqualTo(2.54));

        var convertBack = converter.Invert().Convert(cm);
        Assert.That((decimal)convertBack, Is.EqualTo(1.0));
    }

    [Test]
    public void mps_To_ftPerMinute()
    {
        // This tests undefined conversions, for example:
        // - we've defined conversions for m->cm, cm->in, in->ft, and s/min, 
        // - we've NOT explicitly defined a conversion from m/s->ft/min
        // - we've combined the units into fractions  (m/s -> ft/min) 
        
        // var mps = 1.0;
        // var fps ~= 3.2808398950131235;
        // var fpm ~= 196.8503937007874;  196.85!
        // also, don't be an ass over the 10th digit... closeness isn't a concern due to sig-figs

        bool found = UnitConversions.TryFind(MetersPerSecond, FtPerMinute, out UnitConversion<Number>? converter);

        Assert.That(found, Is.True);
        Assert.That(converter, Is.Not.Null);

        Number fpm = converter.Convert(1.0);
        
        Assert.That((decimal)fpm, Is.EqualTo(196.85047214m));
    }

    [Test]
    public void CanConvertQuantities()
    {
        // var mps = 1.0;
        // var fps = 3.2808398950131235;
        var mps = new Quantity(1, MetersPerSecond);
        
        Quantity<Number> fps = mps.ConvertTo(FtPerSecond);
        Assert.That((decimal)fps.Amount, Is.EqualTo(3.2808398950131235).Within(0.0000001));
        
        // now convert it back
        Quantity<Number> backToMps = fps.ConvertTo(MetersPerSecond);
        Assert.That((decimal)backToMps.Amount, Is.EqualTo(1.0d).Within(0.0000001));
    
        // FIXME: this is important. If we can't have this, then why are we using Number?
        // Number is supposed to NOT lose precision!
        Assert.That(backToMps.Amount, Is.EqualTo(mps.Amount));
    }
}
