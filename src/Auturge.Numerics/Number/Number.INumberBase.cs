using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Auturge.Numerics;

public partial struct Number : INumberBase<Number>
{
    public static int Radix => 10;
    public static Number Abs(Number value) => value.IsNegative ? -value : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsCanonical(Number value) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsComplexNumber(Number value) => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEvenInteger(Number value) => IsInteger(value) && (Abs(value % Two) == Zero);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(Number value) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsImaginaryNumber(Number value) => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsInfinity(Number value) => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger(Number value) => IsFinite(value) && value.DecimalOffset == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsNaN(Number value) => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsNegative(Number value) => value.IsNegative;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsNegativeInfinity(Number value) => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsNormal(Number value) => value != Zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOddInteger(Number value) => IsInteger(value) && value % Two == One;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositive(Number value) => !value.IsNegative;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsPositiveInfinity(Number value) => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsRealNumber(Number value) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.IsSubnormal(Number value) => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(Number value) => value.RawValue == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Number MaxMagnitude(Number x, Number y)
    {
        var ax = Abs(x);
        var ay = Abs(y);

        if (ax > ay)
        {
            return x;
        }

        if (ax == ay)
        {
            return x.IsNegative ? y : x;
        }

        return y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Number INumberBase<Number>.MaxMagnitudeNumber(Number x, Number y) => MaxMagnitude(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Number MinMagnitude(Number x, Number y)
    {
        var ax = Abs(x);
        var ay = Abs(y);

        if (ax < ay)
        {
            return x;
        }

        if (ax == ay)
        {
            return x.IsNegative ? x : y;
        }

        return y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Number INumberBase<Number>.MinMagnitudeNumber(Number x, Number y) => MinMagnitude(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.TryConvertFromChecked<TOther>(TOther value, out Number result)
        => TryConvertFrom(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.TryConvertFromSaturating<TOther>(TOther value, out Number result)
        => TryConvertFrom(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertFrom<TOther>(TOther value, out Number result) where TOther : INumberBase<TOther>
    {
        if (typeof(TOther) == typeof(byte))
        {
            byte actualValue = (byte)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(BigInteger))
        {
            BigInteger actualValue = (BigInteger)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(char))
        {
            char actualValue = (char)(object)value;
            result = new Number(actualValue);
            return true;
        }

        else if (typeof(TOther) == typeof(short))
        {
            short actualValue = (short)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(int))
        {
            int actualValue = (int)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(long))
        {
            long actualValue = (long)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(Int128))
        {
            Int128 actualValue = (Int128)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(nint))
        {
            // Number has no nint constructor; nint always fits in long.
            nint actualValue = (nint)(object)value;
            result = new Number((long)actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(sbyte))
        {
            sbyte actualValue = (sbyte)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(ushort))
        {
            ushort actualValue = (ushort)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(uint))
        {
            uint actualValue = (uint)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(ulong))
        {
            ulong actualValue = (ulong)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(UInt128))
        {
            UInt128 actualValue = (UInt128)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(nuint))
        {
            // Number has no nuint constructor; nuint always fits in ulong.
            nuint actualValue = (nuint)(object)value;
            result = new Number((ulong)actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(decimal))
        {
            decimal actualValue = (decimal)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(double))
        {
            double actualValue = (double)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(Half))
        {
            Half actualValue = (Half)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else if (typeof(TOther) == typeof(float))
        {
            float actualValue = (float)(object)value;
            result = new Number(actualValue);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.TryConvertFromTruncating<TOther>(TOther value, out Number result)
        => TryConvertFrom(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.TryConvertToChecked<TOther>(Number value, [MaybeNullWhen(false)] out TOther result)
    {
        if (typeof(TOther) == typeof(byte))
        {
            byte actualResult = checked((byte)value);
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(char))
        {
            char actualResult = checked((char)value);
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(decimal))
        {
            decimal actualResult = checked((decimal)value);
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ushort))
        {
            ushort actualResult = checked((ushort)value);
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(uint))
        {
            uint actualResult = checked((uint)value);
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ulong))
        {
            ulong actualResult = checked((ulong)value);
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(UInt128))
        {
            UInt128 actualResult = checked((UInt128)value);
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(nuint))
        {
            nuint actualResult = checked((nuint)value);
            result = (TOther)(object)actualResult;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.TryConvertToSaturating<TOther>(Number value,
        [MaybeNullWhen(false)] out TOther result)
        => TryConvertTo(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Number>.TryConvertToTruncating<TOther>(Number value,
        [MaybeNullWhen(false)] out TOther result)
        => TryConvertTo(value, out result);

    // The truncated value WITH its sign restored. RawValue alone is the magnitude, so the
    // saturating conversions below would otherwise clamp a negative value against the wrong bound.
    private static BigInteger TruncatedInteger(Number value)
    {
        Number truncated = value.Truncate();
        return truncated.IsNegative ? -truncated.RawValue : truncated.RawValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertTo<TOther>(Number value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        if (typeof(TOther) == typeof(byte))
        {
            var number = TruncatedInteger(value);
            var actualResult =
                (number >= byte.MaxValue) ? byte.MaxValue :
                (number <= byte.MinValue) ? byte.MinValue : (byte)number;
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(char))
        {
            var number = TruncatedInteger(value);
            var actualResult =
                (number >= char.MaxValue) ? char.MaxValue :
                (number <= char.MinValue) ? char.MinValue : (char)number;
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(decimal))
        {
            var actualResult =
                (value >= new Number(+79228162514264337593543950336.0)) ? decimal.MaxValue :
                (value <= new Number(-79228162514264337593543950336.0)) ? decimal.MinValue :
                (decimal)value;
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ushort))
        {
            var number = TruncatedInteger(value);
            var actualResult =
                (number >= ushort.MaxValue) ? ushort.MaxValue :
                (number <= ushort.MinValue) ? ushort.MinValue : (ushort)number;
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(uint))
        {
            var number = TruncatedInteger(value);
            var actualResult =
                (number >= uint.MaxValue) ? uint.MaxValue :
                (number <= uint.MinValue) ? uint.MinValue : (uint)number;
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(ulong))
        {
            var number = TruncatedInteger(value);
            var actualResult =
                (number >= ulong.MaxValue) ? ulong.MaxValue :
                (number <= ulong.MinValue) ? ulong.MinValue : (ulong)number;
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(UInt128))
        {
            var number = TruncatedInteger(value);
            var actualResult =
                (number >= UInt128.MaxValue) ? UInt128.MaxValue :
                (number <= UInt128.MinValue) ? UInt128.MinValue : (UInt128)number;
            result = (TOther)(object)actualResult;
            return true;
        }
        else if (typeof(TOther) == typeof(nuint))
        {
            var number = TruncatedInteger(value);
            var actualResult =
                (number >= nuint.MaxValue) ? nuint.MaxValue :
                (number <= nuint.MinValue) ? nuint.MinValue : (nuint)number;
            result = (TOther)(object)actualResult;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
}
