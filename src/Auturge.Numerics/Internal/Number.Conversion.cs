using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Auturge.Numerics;

public partial struct Number // Conversion
{
    /// <summary>
    /// Gets the smallest numeric type that can hold this number without loss of precision.
    /// If it cannot be reduced to a smaller data type, returns <see cref="Number"/>.
    /// </summary>
    public static Type GetBestType(Number number)
    {
        var isInteger = IsInteger(number);
        var type = isInteger
            ? GetSmallestIntegerType(number)
            : GetSmallestFloatType(number);
        return type;
    }

    internal static int GetBitSize(Type type) => Marshal.SizeOf(type) * 8;

    private static Type GetSmallestIntegerType(Number number)
    {
        if (!IsInteger(number))
            throw new ArgumentException("Number is not an integer.", nameof(number));

        // RawValue is the magnitude; the smallest type has to account for the sign too
        // (e.g. -40000 fits in int, not in the unsigned ushort its magnitude would suggest).
        var value = number.IsNegative ? -number.RawValue : number.RawValue;
        if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
        {
            return typeof(sbyte);
        }

        if
            (value >= byte.MinValue && value <= byte.MaxValue) // Must be checked after sbyte if 'value' can be negative
        {
            return typeof(byte);
        }

        if (value >= short.MinValue && value <= short.MaxValue)
        {
            return typeof(short);
        }

        if (value >= ushort.MinValue && value <= ushort.MaxValue) // Must be checked after short
        {
            return typeof(ushort);
        }

        if (value >= int.MinValue && value <= int.MaxValue)
        {
            return typeof(int);
        }

        if (value >= uint.MinValue && value <= uint.MaxValue) // Must be checked after int
        {
            return typeof(uint);
        }

        if (value >= long.MinValue && value <= long.MaxValue)
        {
            return typeof(long);
        }

        if (value >= ulong.MinValue && value <= ulong.MaxValue) // Must be checked after long
        {
            return typeof(ulong);
        }

        if (value >= Int128.MinValue && value <= Int128.MaxValue)
        {
            return typeof(Int128);
        }

        if (value >= UInt128.MinValue && value <= UInt128.MaxValue) // Must be checked after Int128
        {
            return typeof(UInt128);
        }

        return typeof(BigInteger);
    }

    private static Type GetSmallestFloatType(Number number)
    {
        // TODO: switch to formatting
        var info = NumberFormatInfo.InvariantInfo;
        var str = number.ToString(info);

        var isDecimal = decimal.TryParse(str, info, out var dec);
        if (isDecimal && dec.ToString(info) == str)
        {
            if (CanHandle<float>(str, info)) return typeof(float);
            if (CanHandle<double>(str, info)) return typeof(double);
            return typeof(decimal);
        }

        return typeof(Number);
    }

    public static bool ConvertsTo(Number number, Type type)
    {
        if (!IsSupportedType(type)) return false;
        if (IsTypeIntegral(type) != IsInteger(number)) return false;

        var smallestType = GetBestType(number);
        var requiredBits = GetBitSize(smallestType);
        var allowedBits = GetBitSize(type);

        return requiredBits <= allowedBits;
    }

    // "Fits in T without loss" == T's shortest round-trippable rendering of the parsed value is
    // byte-for-byte the original text. The previous check compared the round-trip delta against
    // T.Epsilon.ToDecimal(), but every IEEE Epsilon underflows decimal to 0, so it never matched.
    private static bool CanHandle<T>(string str, NumberFormatInfo info)
        where T : IFloatingPointIeee754<T>, IConvertible
    {
        if (!T.TryParse(str, NumberStyles.Float, info, out T? parsed) || parsed is null)
            return false;

        return string.Equals(parsed.ToString(null, info), str, StringComparison.Ordinal);
    }

    private static bool TryParseFloat(Number number, NumberFormatInfo info, out float result)
    {
        // float 	±1.5 x 10^−45 to ±3.4 x 10^38 	    ~6-9 digits 	4 bytes 	System.Single
        // TODO switch to using the formatter
        var str = number.ToString(NumberFormatInfo.InvariantInfo);
        return float.TryParse(str, NumberStyles.Float, info, out result);
    }

    private static bool TryParseDouble(Number number, NumberFormatInfo info, out double result)
    {
        // double 	±5.0 × 10^−324 to ±1.7 × 10^308 	~15-17 digits 	8 bytes 	System.Double
        // TODO switch to using the formatter
        var str = number.ToString(NumberFormatInfo.InvariantInfo);
        return double.TryParse(str, NumberStyles.Float, info, out result);
    }

    private static bool TryParseDecimal(Number number, NumberFormatInfo info, out decimal result)
    {
        // decimal 	±1.0 x 10^-28 to ±7.9228 x 10^28 	28-29 digits 	16 bytes 	System.Decimal
        // TODO switch to using the formatter
        var str = number.ToString(NumberFormatInfo.InvariantInfo);
        return decimal.TryParse(str, NumberStyles.Float, info, out result);
    }


    public static implicit operator decimal(Number number) => number.ToDecimal();
    public static implicit operator double(Number number) => number.ToDouble();
    public static implicit operator int(Number number) => number.ToInt32();
    
    public static implicit operator Number(int v) => new(v.ToString(NumberFormatInfo.InvariantInfo));
    public static implicit operator Number(double v) => new(v.ToString(NumberFormatInfo.InvariantInfo));
}
