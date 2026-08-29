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

    private readonly BigInteger _digitCount;

    /// <summary>
    /// The total number of digits. Does not include the sign or decimal separator.
    /// </summary>
    public BigInteger DigitCount => _digitCount;

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
    internal BigInteger DecimalOffset { get; }

    /// <summary> The smallest numeric type that can hold this Number. </summary>
    [IgnoreDataMember]
    public Type SmallestType { get; }

    /// <summary> The type of the number used to create this Number. </summary>
    [IgnoreDataMember]
    public Type ConstructorType { get; }

    public Number(BigInteger value) : this(value, 0, typeof(BigInteger))
    {
    }

    public Number(BigInteger significand, BigInteger exponent, Type? type = null)
    {
        ConstructorType = type ?? typeof(Number);
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

        SmallestType = GetBestType(this);
    }

    public Number(IConvertible value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), value.GetType())
    {
    }

    public Number(byte value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(byte))
    {
    }

    public Number(sbyte value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(sbyte))
    {
    }

    public Number(short value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(short))
    {
    }

    public Number(ushort value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(ushort))
    {
    }

    public Number(int value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(int))
    {
    }

    public Number(uint value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(uint))
    {
    }

    public Number(char value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(char))
    {
    }
    
    public Number(long value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(long))
    {
    }

    public Number(ulong value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(ulong))
    {
    }

    public Number(Int128 value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(Int128))
    {
    }

    public Number(UInt128 value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(UInt128))
    {
    }

    public Number(Half value) : this(Parse(value.ToString(CultureInfo.CurrentCulture)), typeof(Half))
    {
    }

    public Number(Number number, Type? type = null)
    {
        ConstructorType = type ?? typeof(Number);
        DecimalOffset = number.DecimalOffset;
        IsNegative = number.IsNegative;
        RawValue = number.RawValue;
        Sign = number.Sign;
        SmallestType = number.SmallestType;
        _digitCount = number.DigitCount;
    }

    public override string ToString() => ToString(this, new StringBuilder(), NumberFormatInfo.CurrentInfo).ToString();

    public object ToSmallest() => ToType(SmallestType, NumberFormatInfo.InvariantInfo);
}
