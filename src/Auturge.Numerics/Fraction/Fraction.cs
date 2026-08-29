// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Auturge.Numerics;

[DebuggerDisplay("{Numerator}/{Denominator}")]
public class Fraction<T> where T : INumber<T>, IConvertible
{
    /// <summary>
    /// The approximate value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// The top number in a fraction, indicating how many parts of a whole are being considered or counted.
    /// </summary>
    public BigInteger Numerator { get; }

    /// <summary>
    /// The bottom number in a fraction, showing the total number of equal parts a whole is divided into.
    /// </summary>
    public BigInteger Denominator { get; }

    // TODO: Make this work with Number and arbitrary precision, like the below comment block, but with 
    //  reasonable simplification.

    /// <summary>
    /// Instantiate a fraction from the given value. 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="error"></param>
    public Fraction(T value, decimal error = 0.0000000001m)
    {
        Value = value;
        decimal dec = value.ToDecimal(CultureInfo.InvariantCulture);

        // walk the Stern-Brocot tree
        decimal n = Math.Floor(dec);
        dec -= n;
        if (dec < error)
        {
            Numerator = new BigInteger(n);
            Denominator = 1;
            return;
        }
        else if (1 - error < dec)
        {
            Numerator = new BigInteger(n + 1);
            Denominator = 1;
            return;
        }

        // the lower fraction is 0/1
        int lowerN = 0;
        int lowerD = 1;


        // the upper fraction is 1/1
        int upperN = 1;
        int upperD = 1;

        while (true)
        {
            // The middle fraction is (lower_n + upper_n) / (lower_d + upper_d)
            int middleN = lowerN + upperN;
            int middleD = lowerD + upperD;

            // if x + error < middle
            if (middleD * (dec + error) < middleN)
            {
                // middle is our new upper
                upperN = middleN;
                upperD = middleD;
            }
            else if (middleN < (dec - error) * middleD)
            {
                // middle is out new lower
                lowerN = middleN;
                lowerD = middleD;
            }
            else
            {
                Numerator = new BigInteger(n * middleD + middleN);
                Denominator = middleD;
                return;
            }
        }
    }
}

// public Fraction(T value)
// {
//     // convert it to a Number
//     decimal x = value.ToDecimal(CultureInfo.InvariantCulture);
//
//     // FIXME add this method to Number
//     int[] bits = decimal.GetBits(x);
//
//     BigInteger numerator = (new BigInteger((uint)bits[0]) | ((BigInteger)(uint)bits[1] << 32) |
//                       ((BigInteger)(uint)bits[2] << 64));
//     if ((bits[3] & 0x80000000) != 0) numerator = -numerator;
//
//     int scale = (bits[3] >> 16) & 0x7F;
//     var denominator = BigInteger.Pow(10, scale);
//
//     // Reduce fraction to lowest terms
//     var gcd = BigInteger.GreatestCommonDivisor(num, denominator);
//     Numerator = numerator / gcd;
//     Denominator = denominator / gcd;
//     Value = value;
// }
