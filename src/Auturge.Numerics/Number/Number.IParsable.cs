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
        int decimalOffset = 0;
        while (i < length)
        {
            var c = unsigned[i];
            rawValue *= 10;
            rawValue += c - '0';

            decimalOffset++;
            i++;
        }

        // Fold in any scientific-notation exponent. The digits so far represent
        // rawValue * 10^-decimalOffset; multiplying by 10^exponent shifts the
        // decimal point, i.e. leaves a net negative exponent of (decimalOffset - exponent).
        int netOffset = decimalOffset - simplified.Exponent;
        if (netOffset < 0)
        {
            // A positive net exponent has no place in Number's (significand, negative-exponent)
            // shape, so bake it into the significand instead.
            rawValue *= BigInteger.Pow(10, -netOffset);
            netOffset = 0;
        }

        // Now assign the value
        if (sign == false)
            rawValue = -rawValue;

        number = new Number(rawValue, netOffset);
        return NumberParseResult.Success;
    }

    /// <summary>
    /// Gets the signed number with only sign, digits, and (number) decimal separator.
    /// </summary>
    private static bool TrySimplify(string strValue, NumberStyles style, NumberFormatInfo info,
        out NumberParseBuffer number)
    {
        // TODO: GAF about style? >.<

        // Split off any scientific-notation exponent BEFORE sign/currency trimming, so the
        // exponent's own '+'/'-' sign isn't mistaken for the number's sign. e.g. "1.5E+10"
        // becomes mantissa "1.5" with exponent +10; double.ToString() produces this form for
        // magnitudes like 1e30 ("1E+30") and 1e-30 ("1E-30").
        if (!TrySplitExponent(strValue, info, out string mantissa, out int exponent))
        {
            number = new NumberParseBuffer(NumberParseResult.InvalidCharacter);
            return false;
        }

        // TrimSign returns the boolean sign (true = +, false = -) of the number.
        bool? sign = mantissa.TrimSign(info, out string unsigned);

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
        
        // trim out group separator. (the exponent was already split off up front.)
        string noGroups = trimmed.Replace(groupSep, "");

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

            number = new NumberParseBuffer(NumberParseResult.InvalidCharacter);
            return false;
        }

        number = new NumberParseBuffer(info.NumberDecimalSeparator, sign, noGroups, exponent);
        return true;
    }

    /// <summary>
    /// Splits a scientific-notation string into its mantissa and base-10 exponent
    /// (e.g. <c>"1.5E+10"</c> -> <c>"1.5"</c>, <c>10</c>). Strings without an <c>e</c>/<c>E</c>
    /// pass through unchanged with a zero exponent. Returns <see langword="false"/> for a
    /// malformed exponent (missing digits, non-digit characters, or overflow).
    /// </summary>
    private static bool TrySplitExponent(string strValue, NumberFormatInfo info, out string mantissa,
        out int exponent)
    {
        exponent = 0;

        int eIndex = strValue.AsSpan().IndexOfAny('e', 'E');
        if (eIndex < 0)
        {
            mantissa = strValue;
            return true;
        }

        mantissa = strValue[..eIndex];
        ReadOnlySpan<char> expPart = strValue.AsSpan(eIndex + 1);

        // an 'e'/'E' with nothing before it isn't scientific notation we can use.
        if (mantissa.Length == 0)
            return false;

        bool expNegative = false;
        if (expPart.StartsWith(info.PositiveSign))
        {
            expPart = expPart[info.PositiveSign.Length..];
        }
        else if (expPart.StartsWith(info.NegativeSign))
        {
            expNegative = true;
            expPart = expPart[info.NegativeSign.Length..];
        }

        if (expPart.IsEmpty
            || !int.TryParse(expPart, NumberStyles.None, CultureInfo.InvariantCulture, out int magnitude))
        {
            return false;
        }

        exponent = expNegative ? -magnitude : magnitude;
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
