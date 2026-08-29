using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Auturge.Numerics;

public partial struct Number : IParsable<Number>
{
    public static Number Parse(string strValue, IFormatProvider? provider = null)
        => DoParse(strValue, NumberStyles.Float | NumberStyles.AllowThousands, provider); // IParsable<Number>

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Number result)
        => DoTryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result); // IParsable<Number>
}

public partial struct Number : ISpanParsable<Number>
{
    public static Number Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => DoParse(s.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, provider); // ISpanParsable<Number>

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Number result)
        => DoTryParse(s.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, provider,
            out result); // ISpanParsable<Number>
}

public partial struct Number //:INumberBase<Number> 
{
    public static Number Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider = null)
        => DoParse(s.ToString(), style, provider); // INumberBase<Number>

    public static Number Parse(string s, NumberStyles style, IFormatProvider? provider = null)
        => DoParse(s, style, provider); // INumberBase<Number>

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out Number result)
        => DoTryParse(s.ToString(), style, provider, out result); // INumberBase<Number>

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider,
        out Number result)
        => DoTryParse(s, style, provider, out result); // INumberBase<Number>
}

public partial struct Number // Parsing internals 
{
    public const NumberStyles DefaultStyle = NumberStyles.Float | NumberStyles.AllowThousands;

    private static Number DoParse(string strValue, NumberStyles? style, IFormatProvider? provider)
    {
        NumberParseResult status = GetParseResult(strValue, style, provider, out Number result);
        return status switch
        {
            NumberParseResult.Success => result,
            NumberParseResult.IsNullOrWhiteSpace => throw new NumberParseException(
                "Number to parse must not be null, empty, or whitespace", strValue),
            NumberParseResult.InvalidCharacter => throw new NumberParseException(
                "Number must be numeric", strValue),
            _ => throw new NumberParseException(strValue)
        };
    }

    private static bool DoTryParse(string? stringValue, NumberStyles? style, IFormatProvider? provider,
        out Number number)
        => GetParseResult(stringValue, style, provider, out number) == NumberParseResult.Success;


    private static NumberParseResult GetParseResult(string? stringValue, NumberStyles? style, IFormatProvider? provider,
        out Number number)
    {
        // TODO: GAF about style? >.<

        if (string.IsNullOrWhiteSpace(stringValue))
        {
            number = default;
            return NumberParseResult.IsNullOrWhiteSpace;
        }

        var info = (NumberFormatInfo)NumberFormatInfo.GetInstance(provider).Clone();
        var styles = style ?? DefaultStyle;
        var valid = TrySimplify(stringValue, styles, info, out var simplified);
        if (!valid)
        {
            number = default;
            return simplified.Validity;
        }

        // check the sign.
        var sign = simplified.Sign;
        var decSep = simplified.DecimalSeparator;
        var unsigned = simplified.Value;

        // skip all leading zeroes.
        var i = 0;
        var length = unsigned.Length;
        BigInteger rawValue = 0;
        while (i < length)
        {
            var c = unsigned[i];
            if (c != '0')
                break;
            i++;
        }

        // all that's left now is digits and the decimal separator.
        // process the integer part
        while (i < length)
        {
            if (unsigned[i..].StartsWith(decSep))
                break;

            var c = unsigned[i];
            rawValue *= 10;
            rawValue += c - '0';

            i++;
        }

        // if we're here, then we've either found a decimal separator, or we reached the end.
        i += decSep.Length;
        BigInteger decimalOffset = 0;
        while (i < length)
        {
            var c = unsigned[i];
            rawValue *= 10;
            rawValue += c - '0';

            decimalOffset++;
            i++;
        }

        // Now assign the value
        if (sign == false)
            rawValue = -rawValue;

        number = new Number(rawValue, decimalOffset);
        return NumberParseResult.Success;
    }

    /// <summary>
    /// Gets the signed number with only sign, digits, and (number) decimal separator.
    /// </summary>
    private static bool TrySimplify(string strValue, NumberStyles style, NumberFormatInfo info,
        out NumberParseBuffer number)
    {
        // TODO: GAF about style? >.<

        // TrimSign returns the boolean sign (true = +, false = -) of the number.
        bool? sign = strValue.TrimSign(info, out string unsigned);

        // if it has a currency symbol, then we're parsing currency; otherwise, we're not.
        string decSep;
        string groupSep;
        if (unsigned.TrimCurrency(info, out string trimmed))
        {
            decSep = info.CurrencyDecimalSeparator;
            groupSep = info.CurrencyGroupSeparator;
        }
        else
        {
            decSep = info.NumberDecimalSeparator;
            groupSep = info.NumberGroupSeparator;
        }
        
        // trim out exponential notation.
        string noExponential = trimmed.Replace("e", "").Replace("E", "");
        
        // trim out group separator.
        string noGroups = noExponential.Replace(groupSep, "");

        // return failure on anything that remains
        // which isn't a digit or decimal separator.
        int i = 0;
        while (i < noGroups.Length)
        {
            string str = noGroups[i..];
            uint c = noGroups[i];

            if (str.StartsWith(decSep))
            {
                i += decSep.Length;
                continue;
            }

            if (c.IsDigit())
            {
                i++;
                continue;
            }

            // if c==e or E, that's exponential notation
            if (c == 'E' || c == 'e')
            {
                // validate the sign
            }


            number = new NumberParseBuffer(NumberParseResult.InvalidCharacter);
            return false;
        }

        number = new NumberParseBuffer(info.NumberDecimalSeparator, sign, noGroups);
        return true;
    }
}


// private static Number Parse(string strValue) 
//     => Parse(strValue, CultureInfo.CurrentCulture);
//
// public static Number Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
//     => Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
//
// public static Number Parse(string strValue)
//     => Parse(strValue, CultureInfo.CurrentCulture);
//

//
// public static Number Parse(string strValue, IFormatProvider? provider)
//     => Parse<string>(strValue, provider);
//

//
//
// public static bool TryParse(string strValue, out Number result)
//     => TryParse(strValue, out result, CultureInfo.CurrentCulture);
//
//
// public static bool TryParse([NotNullWhen(true)] string? strValue, out Number result, IFormatProvider? provider)
// {
//     if (strValue == null)
//     {
//         result = Zero;
//         return false;
//     }
//
//     var info = (NumberFormatInfo)NumberFormatInfo.GetInstance(provider).Clone();
//     var status = DoTryParse<string>(strValue, out result, info);
//
//     return status switch
//     {
//         NumberParseResult.Success => true,
//         _ => false
//     };
// }
