using System.Diagnostics;
using System.Numerics;
using Auturge.Numerics;

namespace Auturge.Quantity;

[DebuggerDisplay("{SourceUnit} -> {TargetUnit}")]
public class UnitConversion<T>(Unit sourceUnit, Unit targetUnit)
    : IUnitConversion, IEquatable<UnitConversion<T>> where T : IEquatable<T>, INumber<T>, IConvertible
{
    public Unit SourceUnit { get; } = sourceUnit;
    public Unit TargetUnit { get; } = targetUnit;
    public Conversion<T> Conversion { get; } = Conversion<T>.Identity;

    public UnitConversion(Unit sourceUnit, Unit targetUnit, Conversion<T> conversion) : this(sourceUnit, targetUnit)
    {
        Conversion = conversion;
    }

    internal UnitConversion(Unit sourceUnit, Unit targetUnit,
        Func<T, T> conversion,
        Func<T, T> inversion)
        : this(sourceUnit, targetUnit, new Conversion<T>(conversion, inversion))
    {
    }

    internal UnitConversion(List<UnitConversion<T>> list) : this(list.First().SourceUnit, list.Last().TargetUnit)
    {
        var convList = list.Select(it => it.Conversion).ToList();
        Conversion = new Conversion<T>(convList);
    }

    internal UnitConversion(Unit sourceUnit, Unit targetUnit, T factor)
        : this(sourceUnit, targetUnit, new Conversion<T>(v => v * factor, v => v / factor))
    {
    }

    internal UnitConversion(Unit sourceUnit, Unit targetUnit, Number factor)
        : this(sourceUnit, targetUnit, new Conversion<T>(
            v => (new Number(v) * factor).ToType<T>(),
            v => (new Number(v) / factor).ToType<T>()))
    {
    }

    internal UnitConversion(Unit sourceUnit, Unit targetUnit, Conversion conversion) : this(sourceUnit, targetUnit,
        conversion.Unbox<T>())
    {
    }
    
    /// <summary>
    /// Determines whether this converts from one unit to the other.
    /// </summary>
    public bool CanHandle(Unit a, Unit b)
    {
        // We can convert A down to bases_A, then convert that to bases_B, then convert it up to B.

        // 1) Look at definitions.
        //      Check numerator: Do we need a converter?
        //          If so, then figure out the "base" definitions for the definitions.
        //      Check denominator: Do we need a converter?
        //          If so, then figure out the "base" definitions for the definitions.
        // 2) Look for converters between bases
        // 3) Validate
        //      Do we have converters for all the bases?
        //      Are there any extras that can't be converted?
        // 4) Convert

        // Ex: 15 km/hr to x mph
        // 1) km/hr -> m/s   and   mi/hr -> ft/s  (ft is a base)
        //      1 km/hr = 1000m/hr      (don't need this = 1000m/3600s)
        //      1 mi/hr = 5280ft/hr     (don't need this = 5280ft/3600s)
        // 2) Need converter for m -> ft.  s->s is identity.
        // 3) IF we have m->ft and identity, then we have everything we need. No extras.
        // 4) 15000m/hr ->  m->ft = (1 ft/ 0.3048 m) 
        //      ( 15000m ) ( 1 ft/ 0.3048 m ) (1 mi / 5280 ft) = 15*(3048/5280) ft/hr

        // This method handles 1-3.

        // Can we simply calculate a single factor to multiply?
        // 1 km/hr = 3048/5280 mi/hr

        // it should be from 'meters' to 'feet'

        return a == SourceUnit && b == TargetUnit
               || a == TargetUnit && b == SourceUnit;
    }

    public T Convert(T amount) => Conversion.Execute(amount);

    // public Number Convert(Number amount) => Conversion.Execute(amount);

    #region Arithmetic Operators

    public static UnitConversion<T> operator *(UnitConversion<T> lhs, UnitConversion<T> rhs)
    {
        // this means: do conversion 1, then do conversion 2
        var x = lhs.Conversion * rhs.Conversion;
        return new UnitConversion<T>(lhs.SourceUnit, rhs.TargetUnit, lhs.Conversion * rhs.Conversion);
    }

    public static UnitConversion<T> operator /(UnitConversion<T> lhs, UnitConversion<T> rhs)
    {
        // this means: do conversion 1, then do conversion inversion 2
        return new UnitConversion<T>(lhs.SourceUnit, rhs.TargetUnit, lhs.Conversion / rhs.Conversion);
    }

    #endregion Arithmetic Operators

    public static implicit operator UnitConversion<T>(List<UnitConversion<T>> list) => new(list);

    public UnitConversion<T> Invert() => Invert(this);

    public static UnitConversion<T> Invert(UnitConversion<T> toInvert)
        => new(toInvert.TargetUnit, toInvert.SourceUnit, toInvert.Conversion.Invert());


    public static UnitConversion<T> Create(Unit source, Unit target, Func<T, T> conversion, Func<T, T> inversion) 
        => new(source, target, conversion, inversion);

    public bool Equals(UnitConversion<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        bool sameSource = SourceUnit.Equals(other.SourceUnit);
        bool sameTarget = TargetUnit.Equals(other.TargetUnit);
        // because the conversion functions are dynamic lambdas,
        // we can't really care about the conversions.
        // Also, we shouldn't HAVE multiple conversions from, e.g., meters to feet.
        bool equal = sameSource && sameTarget;
        return equal;
    }

    public override bool Equals(object? obj) => Equals(obj as UnitConversion<T>);

    public override int GetHashCode() => HashCode.Combine(SourceUnit, TargetUnit);
}
