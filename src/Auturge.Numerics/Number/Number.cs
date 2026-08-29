using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Auturge.Numerics;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public partial struct Number
{
    /// <summary> Gets the value <c>0</c> for the type. </summary>
    public static readonly Number Zero = new(0L);

    static Number INumberBase<Number>.Zero => Zero;

    /// <summary> Gets the value <c>1</c> for the type. </summary>
    public static readonly Number One = new(1L);

    static Number INumberBase<Number>.One => One;

    /// <summary> Gets the value <c>2</c> for the type. </summary>
    public static readonly Number Two = new(2L);

    private readonly int _digitCount;

    /// <summary>
    /// The total number of digits. Does not include the sign or decimal separator.
    /// </summary>
    public int DigitCount => _digitCount;

    /// <summary>
    /// -1 if negative, 0 if zero, +1 if positive.
    /// </summary>
    public readonly int Sign { get; }

    /// <summary> Returns whether the value is an integer or not. </summary>
    public readonly bool IsIntegral { get; }

    /// <summary> Returns whether the value is negative or not. </summary>
    public bool IsNegative { get; }

    /// <summary> The significand. </summary>
    internal BigInteger RawValue { get; }

    /// <summary> The negative exponent. </summary>
    internal int DecimalOffset { get; }

    private Type? _smallestType;

    /// <summary>
    /// The smallest numeric type that can hold this Number.
    /// Computed lazily on first access rather than eagerly in the constructor,
    /// since most Numbers (e.g. every intermediate arithmetic result) never need it.
    /// </summary>
    [IgnoreDataMember]
    public Type SmallestType => _smallestType ??= GetBestType(this);

    public Number(BigInteger value) : this(value, 0)
    {
    }

    public Number(BigInteger significand, int exponent)
    {
        IsNegative = significand < 0;
        if (IsNegative)
        {
            significand = -significand;
        }

        Sign = IsNegative ? -1 : this == Zero ? 0 : 1;

        _digitCount = CountDigits(significand);
        TrimRight(ref exponent, ref significand, ref _digitCount);
        RawValue = significand;
        DecimalOffset = exponent;
        IsIntegral = IsInteger(this);
    }

    public Number(IConvertible value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)))
    {
    }

    // Integral primitives are constructed straight from their bits via BigInteger,
    // rather than round-tripping through ToString()/Parse() — they have no fractional
    // part, so the string round-trip was pure overhead (an allocation plus the full
    // sign/currency/group-separator parsing pipeline for what is always just digits).
    public Number(byte value) : this(new BigInteger((int)value), 0)
    {
    }

    public Number(sbyte value) : this(new BigInteger((int)value), 0)
    {
    }

    public Number(short value) : this(new BigInteger((int)value), 0)
    {
    }

    public Number(ushort value) : this(new BigInteger((int)value), 0)
    {
    }

    public Number(int value) : this(new BigInteger(value), 0)
    {
    }

    public Number(uint value) : this(new BigInteger(value), 0)
    {
    }

    public Number(char value) : this(new BigInteger((int)value), 0)
    {
    }

    public Number(long value) : this(new BigInteger(value), 0)
    {
    }

    public Number(ulong value) : this(new BigInteger(value), 0)
    {
    }

    public Number(Int128 value) : this(BigInteger.CreateChecked(value), 0)
    {
    }

    public Number(UInt128 value) : this(BigInteger.CreateChecked(value), 0)
    {
    }

    // Half (like decimal/double/float via the IConvertible ctor above) keeps the
    // string round-trip: it can hold a fractional value, and formatting is the
    // simplest way to capture its exact decimal representation without loss.
    public Number(Half value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)))
    {
    }

    public Number(Number number)
    {
        DecimalOffset = number.DecimalOffset;
        IsNegative = number.IsNegative;
        RawValue = number.RawValue;
        Sign = number.Sign;
        IsIntegral = number.IsIntegral;
        _smallestType = number._smallestType; // preserve laziness; don't force computation
        _digitCount = number.DigitCount;
    }

    public override string ToString() => ToString(this, new StringBuilder(), NumberFormatInfo.CurrentInfo).ToString();

    public object ToSmallest() => ToType(SmallestType, NumberFormatInfo.InvariantInfo);
}
