using System.Globalization;
using System.Numerics;

namespace Auturge.Numerics;

public partial struct Number : IFloatingPoint<Number>
{
    // https://en.wikipedia.org/wiki/E_(mathematical_constant)
    public static Number E => Parse("2.718281828459045235360287471352", NumberFormatInfo.InvariantInfo);

    // https://en.wikipedia.org/wiki/Pi
    public static Number Pi =>
        Parse("3.14159265358979323846264338327950288419716939937510", NumberFormatInfo.InvariantInfo);

    // https://en.wikipedia.org/wiki/Tau_(mathematics) = 2*pi
    public static Number Tau =>
        Parse("6.28318530717958647692528676655900576839433879875021", NumberFormatInfo.InvariantInfo);

    // DecimalOffset is a plain int, but these members are specified in terms of BigInteger-style
    // byte/bit counts, so wrap it briefly to reuse that math.
    public int GetExponentByteCount() => new BigInteger(DecimalOffset).GetByteCount();

    public int GetExponentShortestBitLength() => Convert.ToInt32(new BigInteger(DecimalOffset).GetBitLength());

    public int GetSignificandBitLength() => Convert.ToInt32(RawValue.GetBitLength());

    public int GetSignificandByteCount() => RawValue.GetByteCount(!IsNegative);

    public bool TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten)
    {
        return TryWriteExponent(destination, true, out bytesWritten);
    }

    public bool TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten)
    {
        return TryWriteExponent(destination, false, out bytesWritten);
    }

    private bool TryWriteExponent(Span<byte> destination, bool bigEndian, out int bytesWritten)
    {
        // The significand is the part of a floating-point number
        // or a number in scientific notation that contains its
        // significant digits and represents the number's precision.

        // In the case of a "Number", the significand is the RawValue.
        // When we say Value = Significand * 10^Exponent, then
        //   the DecimalOffset is the negative of the Exponent.

        var exponent = new BigInteger(DecimalOffset);
        var sizeInBytes = exponent.GetByteCount();

        // Check if the destination span is large enough.
        if (destination.Length < sizeInBytes)
        {
            bytesWritten = 0;
            return false;
        }

        var bytes = exponent.ToByteArray(true, bigEndian).AsSpan();

        // Copy the significand to the destination.
        bytes.Slice(0, sizeInBytes).CopyTo(destination);

        bytesWritten = sizeInBytes;
        return true;
    }

    public bool TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten)
    {
        return TryWriteSignificand(destination, true, out bytesWritten);
    }

    public bool TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten)
    {
        return TryWriteSignificand(destination, false, out bytesWritten);
    }

    private bool TryWriteSignificand(Span<byte> destination, bool bigEndian, out int bytesWritten)
    {
        // The significand is the part of a floating-point number
        // or a number in scientific notation that contains its
        // significant digits and represents the number's precision.

        // In the case of a "Number", the significand is the RawValue.
        // When we say Value = Significand * 10^Exponent, then
        //   the DecimalOffset is the negative of the Exponent.

        var significand = RawValue;
        var sizeInBytes = GetSignificandByteCount();

        // Check if the destination span is large enough.
        if (destination.Length < sizeInBytes)
        {
            bytesWritten = 0;
            return false;
        }

        var bytes = significand.ToByteArray(true, bigEndian).AsSpan();

        // Copy the significand to the destination.
        bytes.Slice(0, sizeInBytes).CopyTo(destination);

        bytesWritten = sizeInBytes;
        return true;
    }

    public static Number Round(Number n, int digits, MidpointRounding mode)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(digits, BigInteger.Zero);
        if (n.DecimalOffset <= digits)
            return n; // Already at or below the requested precision. Nothing to round off.

        var diff = n.DecimalOffset - digits;
        var multiplier = new BigInteger(1);
        for (var i = 0; i < diff; i++)
            multiplier *= 10;

        var intPart = n.RawValue / multiplier;

        // grab the digit in question
        // TODO: how can we fuck this up?
        var semi = multiplier / 10;
        var semiPart = n.RawValue / semi;
        var outerDigit = semiPart - intPart * 10;
        var innerDigit = intPart - (intPart / 10) * 10;

        if (mode == MidpointRounding.ToEven)
        {
            // This strategy involves rounding to the nearest number,
            // and when a number is halfway between two others, it is rounded towards the nearest even number.
            if (outerDigit > 5 || (outerDigit == 5 && innerDigit % 2 == 1))
            {
                intPart += 1;
            }
        }
        else if (mode == MidpointRounding.AwayFromZero)
        {
            // The AwayFromZero rounding strategy rounds to the nearest number,
            // rounding a number halfway between two others away from zero.
            if (outerDigit >= 5)
            {
                intPart += 1;
            }
        }
        else if (mode == MidpointRounding.ToNegativeInfinity)
        {
            // This strategy entails rounding in a downward direction,
            // with the result being the closest to and no greater than the infinitely precise result.
            // To round to a specific decimal place towards negative infinity, you multiply the number
            // by a power of 10 to shift the decimal, apply Math.Floor(), and then divide by the same
            // power of 10 to shift the decimal back.
            // This effectively rounds down to the desired precision.
            // 3.7 -> 3
            // -3.2 -> -4
            // 5.0 -> 5
            if (outerDigit > 0)
            {
                intPart += n.IsNegative ? 1 : 0;
            }
        }
        else if (mode == MidpointRounding.ToPositiveInfinity)
        {
            // This strategy involves rounding in an upward direction,
            // with the result being the closest to and no less than the infinitely precise result.
            // 2.336 -> 2.34
            // -2.336 -> -2.33
            if (outerDigit > 0)
            {
                intPart += n.IsNegative ? 0 : 1;
            }
        }
        // MidpointRounding.ToZero is basically the same as truncating.

        if (n.IsNegative)
            intPart *= -1;

        return new Number(intPart, digits);
    }

    public static Number Truncate(Number n) => Round(n, 0, MidpointRounding.ToZero);

    public static Number TruncateTo(Number n, int numFractionalDigits)
        => Round(n, numFractionalDigits, MidpointRounding.ToZero);
}
