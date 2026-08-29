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
        var info = (NumberFormatInfo)NumberFormatInfo.GetInstance(provider).Clone();

        if (number.RawValue.IsZero)
        {
            stringBuilder.Append('0');
            return stringBuilder;
        }

        if (number.IsNegative)
            stringBuilder.Append(info.NegativeSign);

        var divisor = new BigInteger(1);
        for (BigInteger i = 1; i < number.DigitCount; i++)
            divisor *= 10;

        var dotWritten = false;
        if (number.DecimalOffset >= number.DigitCount)
        {
            stringBuilder.Append('0');
            stringBuilder.Append(info.NumberDecimalSeparator);
            dotWritten = true;

            var offset = number.DecimalOffset - 1;
            while (offset >= number.DigitCount)
            {
                stringBuilder.Append('0');
                offset--;
            }
        }

        var divisorIndex = number.DigitCount - number.DecimalOffset;
        for (BigInteger i = 0; i < number.DigitCount; i++)
        {
            if (!dotWritten && i == divisorIndex)
                stringBuilder.Append(info.NumberDecimalSeparator);

            var digitValue = (number.RawValue / divisor) % 10;
            stringBuilder.Append((char)(digitValue + '0'));
            divisor /= 10;
        }

        return stringBuilder;
    }

    private static BigInteger CountDigits(BigInteger value)
    {
        BigInteger count = 0;

        while (value > 0)
        {
            count++;
            value /= 10;
        }

        return count;
    }

    private static void TrimRight(ref BigInteger decimalOffset, ref BigInteger value, ref BigInteger digitCount)
    {
        while (decimalOffset > 0 && (value % 10) == 0)
        {
            decimalOffset--;
            value /= 10;
            digitCount--;
        }
    }
    
    private static BigInteger MakeItHaveThisManyDigits(Number number, BigInteger numDigits)
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
