// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInitializerReferesToMemberBelow
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Diagnostics.CodeAnalysis;
using Auturge.Quantity;
using static Auturge.Quantity.SIPrefixes;

namespace Auturge.Quantity;

public sealed class Units : MemberCache<Units, Unit>
{
    #region List Methods

    /// <summary>
    /// Find all known units that measure a specific dimension.
    /// </summary>
    public static List<Unit> FindAll(DimensionVector dimension)
    {
        return Items.Where(it => it.Dimension == dimension).ToList();
    }

    public static bool TryFind(DimensionVector dimension, UnitDefinition definition,
        [MaybeNullWhen(false)] out Unit unit)
        => TryFind(x => x.Dimension == dimension && x.Definition == definition, out unit);

    #endregion List Methods

    // =======================================================================
    //   UNITS 
    // =======================================================================

    // ANY: A unit that corresponds to ANY unit
    public static readonly Unit Any = new(-1, "", "", Dimensions.Any);

    public static readonly Unit One = new(1, "1", "1", Dimensions.None);
    public static readonly Unit Each = new(2, "each", "ea", Dimensions.None);

    #region Base Units

    #region Time Units

    public static readonly Unit Seconds = new("seconds", "s", Dimensions.Time);
    public static Unit s => Seconds;

    // Derived SI Unit
    public static readonly Unit Quectoseconds = new(Quecto * Seconds);
    public static readonly Unit Rontoseconds = new(Ronto * Seconds);
    public static readonly Unit Yoctoseconds = new(Yocto * Seconds);
    public static readonly Unit Zeptoseconds = new(Zepto * Seconds);
    public static readonly Unit Attoseconds = new(Atto * Seconds);

    public static readonly Unit Femtoseconds = new(Femto * Seconds);
    public static Unit fs => Femtoseconds;

    public static readonly Unit Picoseconds = new(Pico * Seconds);
    public static Unit ps => Picoseconds;

    public static readonly Unit Nanoseconds = new(Nano * Seconds);
    public static Unit ns => Nanoseconds;

    public static readonly Unit Microseconds = new(Micro * Seconds);
    public static Unit μs => Microseconds;
    public static Unit us => Microseconds;

    public static readonly Unit Milliseconds = new(Milli * Seconds);
    public static Unit ms => Milliseconds;

    public static readonly Unit Centiseconds = new(Centi * Seconds);
    public static Unit cs => Centiseconds;

    public static readonly Unit Deciseconds = new(Deci * Seconds);
    public static Unit ds => Deciseconds;

    public static readonly Unit Decaseconds = new(Deca * Seconds);
    public static Unit das => Decaseconds;

    public static readonly Unit Hectoseconds = new(Hecto * Seconds);
    public static Unit hs => Hectoseconds;

    public static readonly Unit Kiloseconds = new(Kilo * Seconds);
    public static Unit ks => Kiloseconds;

    public static readonly Unit Megaseconds = new(Mega * Seconds);
    public static Unit Ms => Megaseconds;

    public static readonly Unit Gigaseconds = new(Giga * Seconds);
    public static Unit Gs => Gigaseconds;

    public static readonly Unit Teraseconds = new(Tera * Seconds);
    public static readonly Unit Petaseconds = new(Peta * Seconds);
    public static readonly Unit Exaseconds = new(Exa * Seconds);
    public static readonly Unit Zettaseconds = new(Zetta * Seconds);
    public static readonly Unit Yottaseconds = new(Yotta * Seconds);
    public static readonly Unit Ronnaseconds = new(Ronna * Seconds);
    public static readonly Unit Quettaseconds = new(Quetta * Seconds);


    public static readonly Unit Minutes = new("minutes", "m", Dimensions.Time);
    public static Unit min => Minutes;

    public static readonly Unit Hours = new("hours", "hr", Dimensions.Time);
    public static Unit hr => Hours;

    public static readonly Unit Days = new("days", "d", Dimensions.Time);
    public static Unit d => Days;

    public static readonly Unit Weeks = new("weeks", "wk", Dimensions.Time);
    public static Unit wk => Weeks;

    public static readonly Unit Fortnights = new("fortnights", "fn", Dimensions.Time);
    public static Unit fn => Fortnights;

    public static readonly Unit Months = new("months", "mo", Dimensions.Time);
    public static Unit mo => Months;

    public static readonly Unit Years = new("years", "y", Dimensions.Time);
    public static Unit yr => Years;

    public static readonly Unit Decades = new("decades", "dec", Dimensions.Time);
    public static Unit dec => Decades;

    public static readonly Unit Centuries = new("centuries", "c", Dimensions.Time);
    public static Unit c => Centuries;

    public static readonly Unit Kiloyears = new(Kilo * Years);
    public static Unit ky => Kiloyears;
    public static readonly Unit Kiloannum = Kiloyears.AddSynonym("kiloannum", "ka");
    public static readonly Unit Millennia = Kiloyears.AddSynonym("millenia", "ka");

    public static readonly Unit Megayears = new(Mega * Years);
    public static Unit My => Megayears;
    public static readonly Unit Megaannum = Megayears.AddSynonym("megaannum", "Ma");

    public static readonly Unit Gigayears = new(Giga * Years);
    public static Unit Gy => Gigayears;
    public static readonly Unit Gigaannum = Gigayears.AddSynonym("gigaannum", "Ga");

    public static readonly Unit Terayears = new(Tera * Years);
    public static Unit Ty => Terayears;
    public static readonly Unit Teraannum = Terayears.AddSynonym("teraannum", "Ta");

    public static readonly Unit Petayears = new(Peta * Years);
    public static Unit Py => Petayears;
    public static readonly Unit Petaannum = Petayears.AddSynonym("petaannum", "Pa");

    public static readonly Unit Exayears = new(Exa * Years);
    public static Unit Ey => Exayears;
    public static readonly Unit Exaannum = Exayears.AddSynonym("exaannum", "Ea");

    public static readonly Unit Zettayears = new(Zetta * Years);
    public static readonly Unit Yottayears = new(Yotta * Years);
    public static readonly Unit Ronnayears = new(Ronna * Years);
    public static readonly Unit Quettayears = new(Quetta * Years);

    public static readonly Unit Jiffies = new("jiffies", "j", Dimensions.Time)
    {
        Base = Seconds,
        Factor = 3 * Math.Pow(10, -24)
    };

    #endregion Time Units

    #region Mass Units

    // The SI Unit
    public static readonly Unit Grams = new("grams", "g", Dimensions.Mass);
    public static Unit g => Grams;

    // Derived units
    public static readonly Unit Quectograms = new(Quecto * Grams);
    public static readonly Unit Rontograms = new(Ronto * Grams);
    public static readonly Unit Yoctograms = new(Yocto * Grams);
    public static readonly Unit Zeptograms = new(Zepto * Grams);
    public static readonly Unit Attograms = new(Atto * Grams);
    public static Unit ag => Attograms;

    public static readonly Unit Femtograms = new(Femto * Grams);
    public static Unit fg => Femtograms;

    public static readonly Unit Picograms = new(Pico * Grams);
    public static Unit pg => Picograms;

    public static readonly Unit Nanograms = new(Nano * Grams);
    public static Unit ng => Nanograms;

    public static readonly Unit Micrograms = new(Micro * Grams);
    public static Unit μg => Micrograms;
    public static Unit ug => Micrograms;

    public static readonly Unit Milligrams = new(Milli * Grams);
    public static Unit mg => Milligrams;

    public static readonly Unit Centigrams = new(Centi * Grams);
    public static Unit cg => Centigrams;

    public static readonly Unit Decigrams = new(Deci * Grams);
    public static Unit dg => Decigrams;

    public static readonly Unit Decagrams = new(Deca * Grams);
    public static Unit dag => Decagrams;

    public static readonly Unit Hectograms = new(Hecto * Grams);
    public static Unit hg => Hectograms;

    public static readonly Unit Kilograms = new(Kilo * Grams);
    public static Unit kg => Kilograms;

    public static readonly Unit Megagrams = new(Mega * Grams);
    public static Unit Mg => Megagrams;

    public static readonly Unit Gigagrams = new(Giga * Grams);
    public static Unit Gg => Gigagrams;

    public static readonly Unit Teragrams = new(Tera * Grams);
    public static readonly Unit Petagrams = new(Peta * Grams);
    public static readonly Unit Exagrams = new(Exa * Grams);
    public static readonly Unit Zettagrams = new(Zetta * Grams);
    public static readonly Unit Yottagrams = new(Yotta * Grams);
    public static readonly Unit Ronnagrams = new(Ronna * Grams);
    public static readonly Unit Quettagrams = new(Quetta * Grams);

    public static readonly Unit Tonne = new("tonnes", "t", Dimensions.Mass)
    {
        Factor = 1000, Base = Kilograms
    };

    public static readonly Unit AvoirdupoisPounds = new("pounds", "lb", Dimensions.Mass)
    {
        // "pound" as a unit of mass if defined in a 1959 agreement as exactly:
        Factor = 0.45359237, Base = Kilograms
    };

    public static Unit Pounds => AvoirdupoisPounds;

    public static Unit lbm => Pounds;

    public static readonly Unit Ounces = new("ounces", "oz", Dimensions.Mass)
    {
        Divisor = 16, Base = Pounds
    };

    public static Unit oz => Ounces;

    public static readonly Unit Grains = new("grains", "gr", Dimensions.Mass)
    {
        Factor = 64.79891, Base = Milligrams
        // Divisor = 7000, Base = AvoirdupoisPounds
    };


    public static readonly Unit ShortTon = new("tons", "ton", Dimensions.Mass)
    {
        Factor = 2000, Base = Pounds
    };

    public static Unit TonUS => ShortTon;

    public static readonly Unit LongTon = new("tons", "ton", Dimensions.Mass)
    {
        Factor = 2240, Base = Pounds
    };

    public static Unit TonUK => LongTon;


    #endregion Mass Units

    #region Volume Units

    // The SI Unit
    public static readonly Unit Liters = new("liters", "L", Dimensions.Volume);
    public static Unit L => Liters;

    // Derived units
    public static readonly Unit Quectoliters = new(Quecto * Liters);
    public static readonly Unit Rontoliters = new(Ronto * Liters);
    public static readonly Unit Yoctoliters = new(Yocto * Liters);
    public static readonly Unit Zeptoliters = new(Zepto * Liters);
    public static readonly Unit Attoliters = new(Atto * Liters);
    public static Unit aL => Attoliters;

    public static readonly Unit Femtoliters = new(Femto * Liters);
    public static Unit fL => Femtoliters;

    public static readonly Unit Picoliters = new(Pico * Liters);
    public static Unit pL => Picoliters;

    public static readonly Unit Nanoliters = new(Nano * Liters);
    public static Unit nL => Nanoliters;

    public static readonly Unit Microliters = new(Micro * Liters);
    public static Unit μL => Microliters;
    public static Unit uL => Microliters;

    public static readonly Unit Milliliters = new(Milli * Liters);
    public static Unit mL => Milliliters;

    public static readonly Unit Centiliters = new(Centi * Liters);
    public static Unit cL => Centiliters;

    public static readonly Unit Deciliters = new(Deci * Liters);
    public static Unit dL => Deciliters;

    public static readonly Unit Decaliters = new(Deca * Liters);
    public static Unit daL => Decaliters;

    public static readonly Unit Hectoliters = new(Hecto * Liters);
    public static Unit hL => Hectoliters;

    public static readonly Unit Kiloliters = new(Kilo * Liters);
    public static Unit kL => Kiloliters;

    public static readonly Unit Megaliters = new(Mega * Liters);
    public static Unit ML => Megaliters;

    public static readonly Unit Gigaliters = new(Giga * Liters);
    public static Unit GL => Gigaliters;

    public static readonly Unit Teraliters = new(Tera * Liters);
    public static readonly Unit Petaliters = new(Peta * Liters);
    public static readonly Unit Exaliters = new(Exa * Liters);
    public static readonly Unit Zettaliters = new(Zetta * Liters);
    public static readonly Unit Yottaliters = new(Yotta * Liters);
    public static readonly Unit Ronnaliters = new(Ronna * Liters);
    public static readonly Unit Quettaliters = new(Quetta * Liters);

    public static readonly Unit FluidOuncesImperial = new("imperial fluid ounces", "fl oz (imp)", Dimensions.Volume)
    {
        Factor = 28.4130626, Base = Milliliters
    };

    public static Unit FlOzImp => FluidOuncesImperial;

    public static readonly Unit FluidOuncesUS = new("US customary fluid ounces", "fl oz", Dimensions.Volume)
    {
        Factor = 29.5735295625, Base = Milliliters
    };

    public static Unit FlOz => FluidOuncesUS;

    public static readonly Unit FluidOuncesUSFoodLabeling =
        new("US food labeling fluid ounces", "fl oz", Dimensions.Volume)
        {
            Factor = 30.0, Base = Milliliters
        };

    #endregion Volume Units

    #region Length_SI

    // The SI Unit
    public static readonly Unit Meters = new("meters", "m", Dimensions.Length);
    public static Unit m => Meters;

    // Derived units
    public static readonly Unit AstronomicalUnits = new("astronomical units", "au", Dimensions.Length)
    {
        Base = Meters, Factor = 149597870700
    };

    public static readonly Unit LightYears = new("light-years", "ly", Dimensions.Length)
    {
        Base = Kilometers, Factor = 9460730472580.8
    };

    public static readonly Unit Parsecs = new("parsecs", "pc", Dimensions.Length)
    {
        Base = AstronomicalUnits, Factor = 648000 / Math.PI
    };

    public static readonly Unit Kiloparsecs = new(Kilo * Parsecs);
    public static Unit kpc => Kiloparsecs;
    public static readonly Unit Megaparsecs = new(Mega * Parsecs);
    public static Unit Mpc => Megaparsecs;
    public static readonly Unit Gigaparsecs = new(Giga * Parsecs);
    public static Unit Gpc => Gigaparsecs;

    public static readonly Unit Quectometers = new(Quecto * Meters);
    public static readonly Unit Rontometers = new(Ronto * Meters);
    public static readonly Unit Yoctometers = new(Yocto * Meters);
    public static readonly Unit Zeptometers = new(Zepto * Meters);
    public static readonly Unit Attometers = new(Atto * Meters);
    public static Unit am => Attometers;

    public static readonly Unit Femtometers = new(Femto * Meters);
    public static Unit fm => Femtometers;

    public static readonly Unit Picometers = new(Pico * Meters);
    public static Unit pm => Picometers;
    public static readonly Unit Angstrom = new("ångströms", "Å", Dimensions.Length);

    public static readonly Unit Nanometers = new(Nano * Meters);
    public static Unit nm => Nanometers;

    public static readonly Unit Micrometers = new(Micro * Meters);
    public static Unit μm => Micrometers;
    public static Unit um => Micrometers;
    public static readonly Unit Micron = Micrometers.AddSynonym("microns");

    public static readonly Unit Millimeters = new(Milli * Meters);
    public static Unit mm => Millimeters;

    public static readonly Unit Centimeters = new(Centi * Meters);
    public static Unit cm => Centimeters;

    public static readonly Unit Decimeters = new(Deci * Meters);
    public static Unit dm => Decimeters;

    public static readonly Unit Decameters = new(Deca * Meters);
    public static Unit dam => Decameters;

    public static readonly Unit Hectometers = new(Hecto * Meters);
    public static Unit hm => Hectometers;

    public static readonly Unit Kilometers = new(Kilo * Meters);
    public static Unit km => Kilometers;

    public static readonly Unit Megameters = new(Mega * Meters);
    public static Unit Mm => Megameters;

    public static readonly Unit Gigameters = new(Giga * Meters);
    public static Unit Gm => Gigameters;

    public static readonly Unit Terameters = new(Tera * Meters);
    public static readonly Unit Petameters = new(Peta * Meters);
    public static readonly Unit Exameters = new(Exa * Meters);
    public static readonly Unit Zettameters = new(Zetta * Meters);
    public static readonly Unit Yottameters = new(Yotta * Meters);
    public static readonly Unit Ronnameters = new(Ronna * Meters);
    public static readonly Unit Quettameters = new(Quetta * Meters);

    public static readonly Unit BohrRadii = new("Bohr radii", "r_Bohr", Dimensions.Length)
    {
        Base = Meters,
        Factor = 5.2917721054482 * Math.Pow(10, -11)
    };

    #endregion Length_SI

    #region Length_Imperial

    // Technically, there are no "base" units in imperial, but we're picking them anyway. YOLO.
    // "Base" unit for imperial length:
    public static readonly Unit Inches = new("inch", "in", Dimensions.Length);

    public static readonly Unit Feet = new("feet", "ft", Dimensions.Length)
    {
        Factor = 12, Base = Inches
    };

    public static readonly Unit Yard = new("yard", "yd", Dimensions.Length)
    {
        Factor = 3, Base = Feet
    };

    public static readonly Unit Miles = new("miles", "mi", Dimensions.Length)
    {
        Factor = 5280, Base = Feet
    };

    public static readonly Unit League = new("league", "lea", Dimensions.Length)
    {
        Factor = 3, Base = Miles
    };

    #endregion Length_Imperial

    #endregion Base Units

    #region Physical Units

    // Velocity
    public static readonly Unit MetersPerSecond = new(m / s, "meters per second", "m/s", Dimensions.Velocity);
    public static readonly Unit FtPerSecond = new(Feet / s, "feet per second", "ft/s", Dimensions.Velocity);
    public static readonly Unit FtPerMinute = new(Feet / min, "feet per minute", "ft/min", Dimensions.Velocity);
    public static readonly Unit MilesPerHour = new(Miles / hr, "miles per hour", "mph", Dimensions.Velocity);

    // Acceleration
    public static readonly Unit MetersPerSecondSquared =
        new(m / s / s, "meters per second squared", "m/s²", Dimensions.Acceleration);

    public static readonly Unit Gees = new(MetersPerSecondSquared, "gees", "g", Dimensions.Acceleration)
    {
        Factor = 9.80665, Base = MetersPerSecondSquared
    };

    // Force
    public static readonly Unit Newtons = new(kg * m / s / s, "Newtons", "N", Dimensions.Force);
    public static Unit N => Newtons;

    #endregion Physical Units
    
    
    public static readonly Unit PoundForce = new("pound-force", "lbf", Dimensions.Weight)
    {
        Factor = 1, Base = Pounds * Gees
    };
}
