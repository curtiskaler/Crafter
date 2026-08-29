using System.Globalization;
using System.Numerics;
using System.Text;

namespace Auturge.Numerics;

public partial struct Number
{
    // TODO: change this over to the formatting thing
    private static StringBuilder ToString(Number number, StringBuilder stringBuilder, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(stringBuilder);
        NumberFormatInfo info = NumberFormatInfo.GetInstance(provider);

        if (number.RawValue.IsZero)
        {
            stringBuilder.Append('0');
            return stringBuilder;
        }

        if (number.IsNegative)
            stringBuilder.Append(info.NegativeSign);

        // RawValue's decimal digits ARE the significand's digit string, so ask
        // BigInteger for them directly instead of peeling them off one at a time
        // via repeated division/modulus (which was O(digits) BigInteger divisions
        // per digit produced, i.e. O(digits^2) overall for the whole number).
        string digits = number.RawValue.ToString(CultureInfo.InvariantCulture);
        int digitCount = number.DigitCount;
        int decimalOffset = number.DecimalOffset;

        if (decimalOffset >= digitCount)
        {
            stringBuilder.Append('0');
            stringBuilder.Append(info.NumberDecimalSeparator);

            for (int i = decimalOffset - digitCount; i > 0; i--)
                stringBuilder.Append('0');

            stringBuilder.Append(digits);
        }
        else
        {
            var splitIndex = digitCount - decimalOffset;
            stringBuilder.Append(digits, 0, splitIndex);

            if (decimalOffset > 0)
            {
                stringBuilder.Append(info.NumberDecimalSeparator);
                stringBuilder.Append(digits, splitIndex, digits.Length - splitIndex);
            }
        }

        return stringBuilder;
    }

    private static int CountDigits(BigInteger value)
    {
        int count = 0;

        while (value > 0)
        {
            count++;
            value /= 10;
        }

        return count;
    }

    private static void TrimRight(ref int decimalOffset, ref BigInteger value, ref int digitCount)
    {
        while (decimalOffset > 0 && (value % 10) == 0)
        {
            decimalOffset--;
            value /= 10;
            digitCount--;
        }
    }

    private static BigInteger MakeItHaveThisManyDigits(Number number, int numDigits)
    {
        if (number.DecimalOffset == numDigits)
            return number.RawValue; // Already right size. Do nothing.

        var diff = numDigits - number.DecimalOffset;
        var multiplier = new BigInteger(1);
        for (var i = 0; i < diff; i++)
            multiplier *= 10;

        var intPart = number.RawValue * multiplier;
        return intPart;
    }
}
