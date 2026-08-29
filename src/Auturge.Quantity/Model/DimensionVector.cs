// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace Auturge.Quantity;

/**
 * A vector holding the SI standard dimensions.
 */
public class DimensionVector : IEquatable<DimensionVector>
{
    /// <summary>
    /// The power of the TIME exponent.
    /// </summary>
    public short Time { get; init; }

    /// <summary>
    /// The power of the LENGTH exponent.
    /// </summary>
    public short Length { get; init; }

    /// <summary>
    /// The power of the MASS exponent.
    /// </summary>
    public short Mass { get; init; }

    /// <summary>
    /// The power of the ELECTRIC CURRENT exponent.
    /// </summary>
    public short ElectricCurrent { get; init; }

    /// <summary>
    /// The power of the ABSOLUTE TEMPERATURE exponent.
    /// </summary>
    public short AbsoluteTemperature { get; init; }

    /// <summary>
    /// The power of the AMOUNT OF SUBSTANCE exponent.
    /// </summary>
    public short AmountOfSubstance { get; init; }

    /// <summary>
    /// The power of the LUMINOUS INTENSITY exponent.
    /// </summary>
    public short LuminousIntensity { get; init; }

    /// <summary>
    /// The symbols used to uniquely identify the dimensions of a quantity.
    /// </summary>
    public string Analysis
    {
        get
        {
            string T = GetPowerDescription(Time, "T");
            string L = GetPowerDescription(Length, "L");
            string M = GetPowerDescription(Mass, "M");
            string I = GetPowerDescription(ElectricCurrent, "I");
            string Θ = GetPowerDescription(AbsoluteTemperature, "Θ");
            string N = GetPowerDescription(AmountOfSubstance, "N");
            string J = GetPowerDescription(LuminousIntensity, "J");

            string result = T + L + M + I + Θ + N + J;
            return result.Trim().Replace("  ", " ");
        }
    }

    /// <summary>
    /// ctor for a base or derived dimension.
    /// </summary>
    /// <param name="T">The exponent for the TIME dimension.</param>
    /// <param name="L">The exponent for the LENGTH dimension.</param>
    /// <param name="M">The exponent for the MASS dimension.</param>
    /// <param name="I">The exponent for the ELECTRIC CURRENT dimension.</param>
    /// <param name="Θ">The exponent for the ABSOLUTE TEMPERATURE dimension.</param>
    /// <param name="N">The exponent for the AMOUNT OF SUBSTANCE dimension.</param>
    /// <param name="J">The exponent for the LUMINOUS INTENSITY dimension.</param>
    public DimensionVector(short T, short L, short M, short I, short Θ, short N, short J)
    {
        Time = T;
        Length = L;
        Mass = M;
        ElectricCurrent = I;
        AbsoluteTemperature = Θ;
        AmountOfSubstance = N;
        LuminousIntensity = J;
    }

    /// <summary>
    /// ctor for a derived dimension.
    /// </summary>
    /// <param name="Q">A BuckinghamVector containing the properties of this dimension.</param>
    public DimensionVector(DimensionVector Q)
    {
        Time = Q.Time;
        Length = Q.Length;
        Mass = Q.Mass;
        ElectricCurrent = Q.ElectricCurrent;
        AbsoluteTemperature = Q.AbsoluteTemperature;
        AmountOfSubstance = Q.AmountOfSubstance;
        LuminousIntensity = Q.LuminousIntensity;
    }

    /// <summary>
    /// internal ctor for arithmetic operators.
    /// </summary>
    private DimensionVector(List<DimensionVector>? numerator, List<DimensionVector>? denominator)
    {
        // add the values in the numerator, subtract the numbers in the denominator
        Time = Aggregate(numerator, denominator, d => d.Time);
        Length = Aggregate(numerator, denominator, d => d.Length);
        Mass = Aggregate(numerator, denominator, d => d.Mass);
        ElectricCurrent = Aggregate(numerator, denominator, d => d.ElectricCurrent);
        AbsoluteTemperature = Aggregate(numerator, denominator, d => d.AbsoluteTemperature);
        AmountOfSubstance = Aggregate(numerator, denominator, d => d.AmountOfSubstance);
        LuminousIntensity = Aggregate(numerator, denominator, d => d.LuminousIntensity);
    }

    public static readonly DimensionVector One = new(0, 0, 0, 0, 0, 0, 0);

    #region IEquatable<Dimension>

    public bool Equals(DimensionVector? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Time == other.Time && Length == other.Length && Mass == other.Mass &&
               ElectricCurrent == other.ElectricCurrent && AbsoluteTemperature == other.AbsoluteTemperature &&
               AmountOfSubstance == other.AmountOfSubstance && LuminousIntensity == other.LuminousIntensity;
    }

    public override bool Equals(object? obj) 
        => ReferenceEquals(this, obj) || obj is DimensionVector other && Equals(other);

    public override int GetHashCode() 
        => HashCode.Combine(Time, Length, Mass, ElectricCurrent, AbsoluteTemperature, AmountOfSubstance,
            LuminousIntensity);

    public static bool operator ==(DimensionVector? lhs, DimensionVector? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(DimensionVector? left, DimensionVector? right) => !(left == right);

    #endregion IEquatable<Dimension>

    #region Operators

    // return a new UnitType that represents the combination(multiplication) of the two unit types
    // For example, "Area" = Length * Length
    public static DimensionVector operator *(DimensionVector a, DimensionVector b) 
        => new([a, b], null);

    // return a new UnitType that represents the combination(division) of the two unit types
    // For example, "Velocity" = Length * Time
    public static DimensionVector operator /(DimensionVector dividend, DimensionVector divisor) 
        => new([dividend], [divisor]);

    public DimensionVector Reciprocal() => Reciprocal(this);

    public static DimensionVector Reciprocal(DimensionVector v) 
        => new((short)-v.Time, (short)-v.Length, (short)-v.Mass, (short)-v.ElectricCurrent,
            (short)-v.AbsoluteTemperature, (short)-v.AmountOfSubstance, (short)-v.LuminousIntensity);

    #endregion Operators

    private static string GetPowerDescription(short v, string s)
        => v switch
        {
            0 => "",
            1 => $" {s} ",
            _ => $" {s}^{v} "
        };

    private static short Aggregate(List<DimensionVector>? numerator, List<DimensionVector>? denominator,
        Func<DimensionVector, short> selector)
        => (short)(AddValues(numerator, selector) - AddValues(denominator, selector));

    private static short AddValues(List<DimensionVector>? list, Func<DimensionVector, short> selector) 
        => list is { Count: > 0 }
            ? list.Select(selector).Aggregate<short, short>(0, (current, b) => (short)(current + b))
            : (short)0;
}
