// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Auturge.Numerics;
using Auturge.Quantity.Exceptions;
using static Auturge.Quantity.Units;

namespace Auturge.Quantity;

public class UnitConversions<T> : MemberCache<UnitConversions<T>, UnitConversion<T>> where T : INumber<T>, IConvertible
{
    #region List Methods

    public static UnitConversion<T> Find(Unit source, Unit target)
    {
        bool found = TryFind(source, target, out UnitConversion<T>? converter);
        if (found && converter != null)
        {
            return converter;
        }

        throw new ConverterNotFoundException(source, target);
    }

    public static bool TryFind(Unit source, Unit target, [MaybeNullWhen(false)] out UnitConversion<T> converter)
    {
        converter = null;

        if (source == target)
        {
            converter = One;
            return true;
        }

        bool found = TryFind(x => x.CanHandle(source, target), out converter);
        if (found && converter != null)
        {
            return true;
        }

        // we didn't find one... can we MAKE one?
        // Finding a chain to convert from unit a to unit b using a list of converters
        // is best solved by treating the units as a graph and finding the shortest path 
        // between them, using an algorithm like Breadth-First Search (BFS) or Dijkstra's
        // algorithm.

        // this is wrong. It initializes the graph with everything, every unit.
        var graph = new UnitConversionGraph<T>(Items);
        found = graph.TryFindPath(source, target, out UnitConversion<T>? tConverter);
        if (!found || tConverter == null) return false;

        converter = tConverter;
        return true;
    }

    #endregion List Methods

    public static readonly UnitConversion<T> One = new(Unit.One, Unit.One, x => x, x => x);
    public static readonly UnitConversion<T> Identity = new(Unit.One, Unit.One, x => x, x => x);

    // ========================================================================
    //   BASE CONVERSIONS (conversions from one base to another) 
    //  There are no real "bases" in imperial units, so we have to treat  
    //  them ALL as bases and write converters from every unit to every other
    //  OR we could arbitrarily decide on the base for, say, imperial units?
    // ========================================================================

    // distance
    public static readonly UnitConversion<T>
        cmPerInch = new(Inches, Centimeters, 2.54);

    public static readonly UnitConversion<T> ftPerMile = new(Miles, Feet, 5280.0);
    public static readonly UnitConversion<T> inPerFoot = new(Feet, Inches, 12);
    public static readonly UnitConversion<T> ftPerYard = new(Yard, Feet, 3);
    public static readonly UnitConversion<T> miPerLeague = new(League, Miles, 3);

    public static readonly UnitConversion<T> QmPerMeter = new(Quettameters, Meters, Math.Pow(10, 30));
    public static readonly UnitConversion<T> RmPerMeter = new(Ronnameters, Meters, Math.Pow(10, 27));
    public static readonly UnitConversion<T> YmPerMeter = new(Yottameters, Meters, Math.Pow(10, 24));
    public static readonly UnitConversion<T> ZmPerMeter = new(Zettameters, Meters, Math.Pow(10, 21));
    public static readonly UnitConversion<T> EmPerMeter = new(Exameters, Meters, Math.Pow(10, 18));
    public static readonly UnitConversion<T> PmPerMeter = new(Petameters, Meters, Math.Pow(10, 15));
    public static readonly UnitConversion<T> TmPerMeter = new(Terameters, Meters, Math.Pow(10, 12));
    public static readonly UnitConversion<T> GmPerMeter = new(Gigameters, Meters, Math.Pow(10, 9));
    public static readonly UnitConversion<T> MmPerMeter = new(Megameters, Meters, Math.Pow(10, 6));
    public static readonly UnitConversion<T> kmPerMeter = new(Kilometers, Meters, 1000);
    public static readonly UnitConversion<T> hmPerMeter = new(Hectometers, Meters, 100);
    public static readonly UnitConversion<T> damPerMeter = new(Decameters, Meters, 10);
    public static readonly UnitConversion<T> mPerMeter = new(Meters, Meters, 1);
    public static readonly UnitConversion<T> dmPerMeter = new(Meters, Decimeters, 10);
    public static readonly UnitConversion<T> cmPerMeter = new(Meters, Centimeters, 100);
    public static readonly UnitConversion<T> mmPerMeter = new(Meters, Millimeters, 1000);
    public static readonly UnitConversion<T> umPerMeter = new(Meters, Micrometers, Math.Pow(10, 6));
    public static readonly UnitConversion<T> nmPerMeter = new(Meters, Nanometers, Math.Pow(10, 9));
    public static readonly UnitConversion<T> pmPerMeter = new(Meters, Picometers, Math.Pow(10, 12));
    public static readonly UnitConversion<T> fmPerMeter = new(Meters, Femtometers, Math.Pow(10, 15));
    public static readonly UnitConversion<T> amPerMeter = new(Meters, Attometers, Math.Pow(10, 18));
    public static readonly UnitConversion<T> zmPerMeter = new(Meters, Zeptometers, Math.Pow(10, 21));
    public static readonly UnitConversion<T> ymPerMeter = new(Meters, Yoctometers, Math.Pow(10, 24));
    public static readonly UnitConversion<T> rmPerMeter = new(Meters, Rontometers, Math.Pow(10, 27));
    public static readonly UnitConversion<T> qmPerMeter = new(Meters, Quectometers, Math.Pow(10, 30));

    public static readonly UnitConversion<T> pmPerAngstrom = new(Angstrom, Picometers, 100);
    public static readonly UnitConversion<T> auPerMeter = new(AstronomicalUnits, Meters, 149597870700);
    public static readonly UnitConversion<T> lyPerKm = new(LightYears, Kilometers, 9460730472580.8);
    public static readonly UnitConversion<T> pcPerAu = new(Parsecs, AstronomicalUnits, 648000 / Math.PI);
    public static readonly UnitConversion<T> bohrPerMeter = new(BohrRadii, Meters, 5.2917721054482 * Math.Pow(10, -11));

    // time
    public static readonly UnitConversion<T> QyPerYear = new(Quettayears, Years, Math.Pow(10, 30));
    public static readonly UnitConversion<T> RyPerYear = new(Ronnayears, Years, Math.Pow(10, 27));
    public static readonly UnitConversion<T> YyPerYear = new(Yottayears, Years, Math.Pow(10, 24));
    public static readonly UnitConversion<T> ZyPerYear = new(Zettayears, Years, Math.Pow(10, 21));
    public static readonly UnitConversion<T> EyPerYear = new(Exayears, Years, Math.Pow(10, 18));
    public static readonly UnitConversion<T> PyPerYear = new(Petayears, Years, Math.Pow(10, 15));
    public static readonly UnitConversion<T> TyPerYear = new(Terayears, Years, Math.Pow(10, 12));
    public static readonly UnitConversion<T> GyPerYear = new(Gigayears, Years, Math.Pow(10, 9));
    public static readonly UnitConversion<T> MyPerYear = new(Megayears, Years, Math.Pow(10, 6));
    public static readonly UnitConversion<T> millenniaPerYear = new(Millennia, Years, 1000);
    public static readonly UnitConversion<T> centuriesPerYear = new(Centuries, Years, 100);
    public static readonly UnitConversion<T> decadesPerYear = new(Decades, Years, 10);
    public static readonly UnitConversion<T> yPerYear = new(Years, Years, 1);

    public static readonly UnitConversion<T> WeeksPerYear = new(Years, Weeks, 52);
    public static readonly UnitConversion<T> DaysPerWeek = new(Weeks, Days, 7);
    public static readonly UnitConversion<T> DaysPerFortnight = new(Fortnights, Days, 14);

    public static readonly UnitConversion<T> HrsPerDay = new(Days, Hours, 24);
    public static readonly UnitConversion<T> minPerHour = new(Hours, Minutes, 60);
    public static readonly UnitConversion<T> secPerMin = new(Minutes, Seconds, 60);

    public static readonly UnitConversion<T> sPerSecond = new(Seconds, Seconds, 1);
    public static readonly UnitConversion<T> dsPerSecond = new(Seconds, Deciseconds, 10);
    public static readonly UnitConversion<T> csPerSecond = new(Seconds, Centiseconds, 100);
    public static readonly UnitConversion<T> msPerSecond = new(Seconds, Milliseconds, 1000);
    public static readonly UnitConversion<T> usPerSecond = new(Seconds, Microseconds, Math.Pow(10, 6));
    public static readonly UnitConversion<T> nsPerSecond = new(Seconds, Nanoseconds, Math.Pow(10, 9));
    public static readonly UnitConversion<T> psPerSecond = new(Seconds, Picoseconds, Math.Pow(10, 12));
    public static readonly UnitConversion<T> fsPerSecond = new(Seconds, Femtoseconds, Math.Pow(10, 15));
    public static readonly UnitConversion<T> asPerSecond = new(Seconds, Attoseconds, Math.Pow(10, 18));
    public static readonly UnitConversion<T> zsPerSecond = new(Seconds, Zeptoseconds, Math.Pow(10, 21));
    public static readonly UnitConversion<T> ysPerSecond = new(Seconds, Yoctoseconds, Math.Pow(10, 24));
    public static readonly UnitConversion<T> rsPerSecond = new(Seconds, Rontoseconds, Math.Pow(10, 27));
    public static readonly UnitConversion<T> qsPerSecond = new(Seconds, Quectoseconds, Math.Pow(10, 30));


    // public static readonly UnitConversion<T> ftPerMeter = inPerFoot * cmPerInch * (One / cmPerMeter);
}

public class UnitConversions : UnitConversions<Number>;
