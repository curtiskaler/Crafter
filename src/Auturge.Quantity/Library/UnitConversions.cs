// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
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

    public static readonly UnitConversion<T> One = UnitConversion<T>.Create(Unit.One, Unit.One, x => x, x => x);
    public static readonly UnitConversion<T> Identity = UnitConversion<T>.Create(Unit.One, Unit.One, x => x, x => x);

    // ========================================================================
    //   BASE CONVERSIONS (conversions from one base to another) 
    //  There are no real "bases" in imperial units, so we have to treat  
    //  them ALL as bases and write converters from every unit to every other
    //  OR we could arbitrarily decide on the base for, say, imperial units?
    // ========================================================================

    // distance
    public static readonly UnitConversion<T>
        cmPerInch = UnitConversion<T>.Create(Inches, Centimeters, 2.54);

    public static readonly UnitConversion<T> ftPerMile = UnitConversion<T>.Create(Miles, Feet, 5280.0);
    public static readonly UnitConversion<T> inPerFoot = UnitConversion<T>.Create(Feet, Inches, 12);
    public static readonly UnitConversion<T> ftPerYard = UnitConversion<T>.Create(Yard, Feet, 3);
    public static readonly UnitConversion<T> miPerLeague = UnitConversion<T>.Create(League, Miles, 3);

    public static readonly UnitConversion<T> QmPerMeter = UnitConversion<T>.Create(Quettameters, Meters, Math.Pow(10, 30));
    public static readonly UnitConversion<T> RmPerMeter = UnitConversion<T>.Create(Ronnameters, Meters, Math.Pow(10, 27));
    public static readonly UnitConversion<T> YmPerMeter = UnitConversion<T>.Create(Yottameters, Meters, Math.Pow(10, 24));
    public static readonly UnitConversion<T> ZmPerMeter = UnitConversion<T>.Create(Zettameters, Meters, Math.Pow(10, 21));
    public static readonly UnitConversion<T> EmPerMeter = UnitConversion<T>.Create(Exameters, Meters, Math.Pow(10, 18));
    public static readonly UnitConversion<T> PmPerMeter = UnitConversion<T>.Create(Petameters, Meters, Math.Pow(10, 15));
    public static readonly UnitConversion<T> TmPerMeter = UnitConversion<T>.Create(Terameters, Meters, Math.Pow(10, 12));
    public static readonly UnitConversion<T> GmPerMeter = UnitConversion<T>.Create(Gigameters, Meters, Math.Pow(10, 9));
    public static readonly UnitConversion<T> MmPerMeter = UnitConversion<T>.Create(Megameters, Meters, Math.Pow(10, 6));
    public static readonly UnitConversion<T> kmPerMeter = UnitConversion<T>.Create(Kilometers, Meters, 1000);
    public static readonly UnitConversion<T> hmPerMeter = UnitConversion<T>.Create(Hectometers, Meters, 100);
    public static readonly UnitConversion<T> damPerMeter = UnitConversion<T>.Create(Decameters, Meters, 10);
    public static readonly UnitConversion<T> mPerMeter = UnitConversion<T>.Create(Meters, Meters, 1);
    public static readonly UnitConversion<T> dmPerMeter = UnitConversion<T>.Create(Meters, Decimeters, 10);
    public static readonly UnitConversion<T> cmPerMeter = UnitConversion<T>.Create(Meters, Centimeters, 100);
    public static readonly UnitConversion<T> mmPerMeter = UnitConversion<T>.Create(Meters, Millimeters, 1000);
    public static readonly UnitConversion<T> umPerMeter = UnitConversion<T>.Create(Meters, Micrometers, Math.Pow(10, 6));
    public static readonly UnitConversion<T> nmPerMeter = UnitConversion<T>.Create(Meters, Nanometers, Math.Pow(10, 9));
    public static readonly UnitConversion<T> pmPerMeter = UnitConversion<T>.Create(Meters, Picometers, Math.Pow(10, 12));
    public static readonly UnitConversion<T> fmPerMeter = UnitConversion<T>.Create(Meters, Femtometers, Math.Pow(10, 15));
    public static readonly UnitConversion<T> amPerMeter = UnitConversion<T>.Create(Meters, Attometers, Math.Pow(10, 18));
    public static readonly UnitConversion<T> zmPerMeter = UnitConversion<T>.Create(Meters, Zeptometers, Math.Pow(10, 21));
    public static readonly UnitConversion<T> ymPerMeter = UnitConversion<T>.Create(Meters, Yoctometers, Math.Pow(10, 24));
    public static readonly UnitConversion<T> rmPerMeter = UnitConversion<T>.Create(Meters, Rontometers, Math.Pow(10, 27));
    public static readonly UnitConversion<T> qmPerMeter = UnitConversion<T>.Create(Meters, Quectometers, Math.Pow(10, 30));

    public static readonly UnitConversion<T> pmPerAngstrom = UnitConversion<T>.Create(Angstrom, Picometers, 100);
    public static readonly UnitConversion<T> auPerMeter = UnitConversion<T>.Create(AstronomicalUnits, Meters, 149597870700);
    public static readonly UnitConversion<T> lyPerKm = UnitConversion<T>.Create(LightYears, Kilometers, 9460730472580.8);
    public static readonly UnitConversion<T> pcPerAu = UnitConversion<T>.Create(Parsecs, AstronomicalUnits, 648000 / Math.PI);
    public static readonly UnitConversion<T> bohrPerMeter = UnitConversion<T>.Create(BohrRadii, Meters, 5.2917721054482 * Math.Pow(10, -11));

    // time
    public static readonly UnitConversion<T> QyPerYear = UnitConversion<T>.Create(Quettayears, Years, Math.Pow(10, 30));
    public static readonly UnitConversion<T> RyPerYear = UnitConversion<T>.Create(Ronnayears, Years, Math.Pow(10, 27));
    public static readonly UnitConversion<T> YyPerYear = UnitConversion<T>.Create(Yottayears, Years, Math.Pow(10, 24));
    public static readonly UnitConversion<T> ZyPerYear = UnitConversion<T>.Create(Zettayears, Years, Math.Pow(10, 21));
    public static readonly UnitConversion<T> EyPerYear = UnitConversion<T>.Create(Exayears, Years, Math.Pow(10, 18));
    public static readonly UnitConversion<T> PyPerYear = UnitConversion<T>.Create(Petayears, Years, Math.Pow(10, 15));
    public static readonly UnitConversion<T> TyPerYear = UnitConversion<T>.Create(Terayears, Years, Math.Pow(10, 12));
    public static readonly UnitConversion<T> GyPerYear = UnitConversion<T>.Create(Gigayears, Years, Math.Pow(10, 9));
    public static readonly UnitConversion<T> MyPerYear = UnitConversion<T>.Create(Megayears, Years, Math.Pow(10, 6));
    public static readonly UnitConversion<T> millenniaPerYear = UnitConversion<T>.Create(Millennia, Years, 1000);
    public static readonly UnitConversion<T> centuriesPerYear = UnitConversion<T>.Create(Centuries, Years, 100);
    public static readonly UnitConversion<T> decadesPerYear = UnitConversion<T>.Create(Decades, Years, 10);
    public static readonly UnitConversion<T> yPerYear = UnitConversion<T>.Create(Years, Years, 1);

    public static readonly UnitConversion<T> WeeksPerYear = UnitConversion<T>.Create(Years, Weeks, 52);
    public static readonly UnitConversion<T> DaysPerWeek = UnitConversion<T>.Create(Weeks, Days, 7);
    public static readonly UnitConversion<T> DaysPerFortnight = UnitConversion<T>.Create(Fortnights, Days, 14);

    public static readonly UnitConversion<T> HrsPerDay = UnitConversion<T>.Create(Days, Hours, 24);
    public static readonly UnitConversion<T> minPerHour = UnitConversion<T>.Create(Hours, Minutes, 60);
    public static readonly UnitConversion<T> secPerMin = UnitConversion<T>.Create(Minutes, Seconds, 60);

    public static readonly UnitConversion<T> sPerSecond = UnitConversion<T>.Create(Seconds, Seconds, 1);
    public static readonly UnitConversion<T> dsPerSecond = UnitConversion<T>.Create(Seconds, Deciseconds, 10);
    public static readonly UnitConversion<T> csPerSecond = UnitConversion<T>.Create(Seconds, Centiseconds, 100);
    public static readonly UnitConversion<T> msPerSecond = UnitConversion<T>.Create(Seconds, Milliseconds, 1000);
    public static readonly UnitConversion<T> usPerSecond = UnitConversion<T>.Create(Seconds, Microseconds, Math.Pow(10, 6));
    public static readonly UnitConversion<T> nsPerSecond = UnitConversion<T>.Create(Seconds, Nanoseconds, Math.Pow(10, 9));
    public static readonly UnitConversion<T> psPerSecond = UnitConversion<T>.Create(Seconds, Picoseconds, Math.Pow(10, 12));
    public static readonly UnitConversion<T> fsPerSecond = UnitConversion<T>.Create(Seconds, Femtoseconds, Math.Pow(10, 15));
    public static readonly UnitConversion<T> asPerSecond = UnitConversion<T>.Create(Seconds, Attoseconds, Math.Pow(10, 18));
    public static readonly UnitConversion<T> zsPerSecond = UnitConversion<T>.Create(Seconds, Zeptoseconds, Math.Pow(10, 21));
    public static readonly UnitConversion<T> ysPerSecond = UnitConversion<T>.Create(Seconds, Yoctoseconds, Math.Pow(10, 24));
    public static readonly UnitConversion<T> rsPerSecond = UnitConversion<T>.Create(Seconds, Rontoseconds, Math.Pow(10, 27));
    public static readonly UnitConversion<T> qsPerSecond = UnitConversion<T>.Create(Seconds, Quectoseconds, Math.Pow(10, 30));


    // public static readonly UnitConversion<T> ftPerMeter = inPerFoot * cmPerInch * (One / cmPerMeter);
}
