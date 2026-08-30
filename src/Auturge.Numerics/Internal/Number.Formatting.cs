using System.Globalization;
using System.Numerics;
using System.Text;

namespace Auturge.Numerics;

/// <summary>
/// Renders a <see cref="Number"/> according to the .NET standard numeric format strings
/// (see https://learn.microsoft.com/dotnet/standard/base-types/standard-numeric-format-strings).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Number"/> is an exact arbitrary-precision decimal, so every rounding step here is
/// round-half-away-from-zero on the exact value — there is no representation error to reason about,
/// and that matches the long-standing behaviour of the framework's fixed-precision specifiers.
/// </para>
/// <para>
/// The compact/scientific switch of "G" is deliberately not implemented: "G" without a precision is
/// the round-trip form (identical to the parameterless <see cref="Number.ToString()"/>), and "G" with
/// a precision rounds to that many significant digits but always stays in positional notation.
/// </para>
/// </remarks>
internal static class Formatting
{
    /// <summary>Sentinel for "no precision specifier was supplied".</summary>
    private const int NoPrecision = -1;

    private const int MaxPrecisionDigits = 9;

    // Standard negative/positive layout tables, indexed by the matching NumberFormatInfo pattern
    // property. Tokens: n = the formatted digits, $ / % = the currency/percent symbol,
    // - = NegativeSign, everything else is literal.
    private static readonly string[] _numberNegativePatterns =
        ["(n)", "-n", "- n", "n-", "n -"];

    private static readonly string[] _currencyPositivePatterns =
        ["$n", "n$", "$ n", "n $"];

    private static readonly string[] _currencyNegativePatterns =
    [
        "($n)", "-$n", "$-n", "$n-", "(n$)", "-n$", "n-$", "n$-",
        "-n $", "-$ n", "n $-", "$ n-", "$ -n", "n- $", "($ n)", "(n $)", "$- n",
    ];

    private static readonly string[] _percentPositivePatterns =
        ["n %", "n%", "%n", "% n"];

    private static readonly string[] _percentNegativePatterns =
    [
        "-n %", "-n%", "-%n", "%-n", "%n-", "n-%", "n%-", "-% n", "n %-", "% n-", "% -n", "n- %",
    ];

    /// <summary>Formats <paramref name="value"/> using an <see cref="IFormattable"/>-style format string.</summary>
    internal static string FormatNumber(Number value, string? format, NumberFormatInfo info)
    {
        char specifier = ParseFormatSpecifier(format.AsSpan(), out int precision);
        return Format(value, specifier, precision, info);
    }

    /// <summary>Formats into <paramref name="destination"/>; returns <see langword="false"/> if it does not fit.</summary>
    internal static bool TryFormatNumber(Number value, ReadOnlySpan<char> format, NumberFormatInfo info,
        Span<char> destination, out int charsWritten)
    {
        char specifier = ParseFormatSpecifier(format, out int precision);
        string result = Format(value, specifier, precision, info);

        if (result.Length > destination.Length)
        {
            charsWritten = 0;
            return false;
        }

        result.CopyTo(destination);
        charsWritten = result.Length;
        return true;
    }

    /// <summary>
    /// Splits a standard numeric format string into its specifier letter and optional precision.
    /// An empty string maps to the round-trip <c>'G'</c> with <see cref="NoPrecision"/>. Anything that
    /// is not <c>letter[digits]</c> is a custom format string, which this type does not support.
    /// </summary>
    internal static char ParseFormatSpecifier(ReadOnlySpan<char> format, out int digits)
    {
        digits = NoPrecision;

        if (format.IsEmpty)
            return 'G';

        char specifier = format[0];
        if (!char.IsAsciiLetter(specifier))
            throw new FormatException($"'{format.ToString()}' is not a supported numeric format string.");

        if (format.Length == 1)
            return specifier;

        ReadOnlySpan<char> precisionText = format[1..];
        if (precisionText.Length > MaxPrecisionDigits)
            throw new FormatException($"'{format.ToString()}' is not a supported numeric format string.");

        int value = 0;
        foreach (char c in precisionText)
        {
            if (!char.IsAsciiDigit(c))
                throw new FormatException($"'{format.ToString()}' is not a supported numeric format string.");

            value = value * 10 + (c - '0');
        }

        digits = value;
        return specifier;
    }

    private static string Format(Number value, char specifier, int precision, NumberFormatInfo info)
    {
        switch (char.ToUpperInvariant(specifier))
        {
            case 'R':
                return RoundTrip(value, info);

            case 'G':
                return precision > 0
                    ? SignificantDigits(value, precision, info)
                    : RoundTrip(value, info);

            case 'F':
                return FixedPoint(value, precision < 0 ? info.NumberDecimalDigits : precision, info,
                    groupSizes: null, groupSeparator: "", info.NumberDecimalSeparator,
                    _numberNegativePatterns[1], symbol: "");

            case 'N':
                return FixedPoint(value, precision < 0 ? info.NumberDecimalDigits : precision, info,
                    info.NumberGroupSizes, info.NumberGroupSeparator, info.NumberDecimalSeparator,
                    Pattern(_numberNegativePatterns, info.NumberNegativePattern), symbol: "");

            case 'C':
                return FixedPoint(value, precision < 0 ? info.CurrencyDecimalDigits : precision, info,
                    info.CurrencyGroupSizes, info.CurrencyGroupSeparator, info.CurrencyDecimalSeparator,
                    Pattern(_currencyNegativePatterns, info.CurrencyNegativePattern),
                    info.CurrencySymbol, Pattern(_currencyPositivePatterns, info.CurrencyPositivePattern));

            case 'P':
                return FixedPoint(value * OneHundred, precision < 0 ? info.PercentDecimalDigits : precision, info,
                    info.PercentGroupSizes, info.PercentGroupSeparator, info.PercentDecimalSeparator,
                    Pattern(_percentNegativePatterns, info.PercentNegativePattern),
                    info.PercentSymbol, Pattern(_percentPositivePatterns, info.PercentPositivePattern));

            case 'E':
                return Exponential(value, precision < 0 ? 6 : precision, char.IsUpper(specifier) ? 'E' : 'e', info);

            case 'D':
            case 'X':
            case 'B':
                return Integral(value, specifier, precision, info);

            default:
                throw new FormatException($"The '{specifier}' format specifier is not supported for {nameof(Number)}.");
        }
    }

    private static readonly Number OneHundred = new(100L);

    /// <summary>The round-trip form: every significant digit, no rounding. Mirrors <see cref="Number.ToString()"/>.</summary>
    private static string RoundTrip(Number value, NumberFormatInfo info)
    {
        Decompose(value, out bool isNegative, out string integerDigits, out string fractionDigits);
        string body = fractionDigits.Length > 0
            ? integerDigits + info.NumberDecimalSeparator + fractionDigits
            : integerDigits;
        return isNegative ? info.NegativeSign + body : body;
    }

    private static string SignificantDigits(Number value, int significantDigits, NumberFormatInfo info)
    {
        Decompose(value, out bool isNegative, out string integerDigits, out string fractionDigits);

        if (IsZero(integerDigits, fractionDigits))
            return "0";

        int exponent = integerDigits != "0"
            ? integerDigits.Length - 1
            : -(LeadingZeroCount(fractionDigits) + 1);

        RoundToScale(ref integerDigits, ref fractionDigits, significantDigits - 1 - exponent);
        fractionDigits = fractionDigits.TrimEnd('0');

        bool roundedToZero = IsZero(integerDigits, fractionDigits);
        string body = fractionDigits.Length > 0
            ? integerDigits + info.NumberDecimalSeparator + fractionDigits
            : integerDigits;
        return isNegative && !roundedToZero ? info.NegativeSign + body : body;
    }

    private static string FixedPoint(Number value, int decimalPlaces, NumberFormatInfo info,
        int[]? groupSizes, string groupSeparator, string decimalSeparator,
        string negativePattern, string symbol, string? positivePattern = null)
    {
        Decompose(value, out bool isNegative, out string integerDigits, out string fractionDigits);
        RoundToScale(ref integerDigits, ref fractionDigits, decimalPlaces);

        bool roundedToZero = IsZero(integerDigits, fractionDigits);

        if (groupSizes is { Length: > 0 } && groupSeparator.Length > 0)
            integerDigits = ApplyGrouping(integerDigits, groupSizes, groupSeparator);

        string digits = decimalPlaces > 0
            ? integerDigits + decimalSeparator + fractionDigits
            : integerDigits;

        string pattern = isNegative && !roundedToZero ? negativePattern : positivePattern ?? "n";
        return ApplyPattern(pattern, digits, symbol, info.NegativeSign);
    }

    private static string Exponential(Number value, int decimalPlaces, char exponentChar, NumberFormatInfo info)
    {
        Decompose(value, out bool isNegative, out string integerDigits, out string fractionDigits);

        string allDigits = integerDigits + fractionDigits;
        int pointIndex = integerDigits.Length;
        int firstSignificant = -1;
        for (int i = 0; i < allDigits.Length; i++)
        {
            if (allDigits[i] != '0')
            {
                firstSignificant = i;
                break;
            }
        }

        if (firstSignificant < 0)
        {
            string zeroMantissa = decimalPlaces > 0
                ? "0" + info.NumberDecimalSeparator + new string('0', decimalPlaces)
                : "0";
            return zeroMantissa + exponentChar + info.PositiveSign + "000";
        }

        int exponent = pointIndex - firstSignificant - 1;
        string significant = allDigits[firstSignificant..];
        int keep = decimalPlaces + 1;

        char[] mantissa;
        if (significant.Length <= keep)
        {
            mantissa = significant.PadRight(keep, '0').ToCharArray();
        }
        else
        {
            bool roundUp = significant[keep] >= '5';
            mantissa = significant[..keep].ToCharArray();
            if (roundUp)
            {
                int idx = keep - 1;
                while (idx >= 0 && mantissa[idx] == '9')
                {
                    mantissa[idx] = '0';
                    idx--;
                }

                if (idx < 0)
                {
                    mantissa = ('1' + new string(mantissa, 0, keep - 1)).ToCharArray();
                    exponent++;
                }
                else
                {
                    mantissa[idx]++;
                }
            }
        }

        var builder = new StringBuilder();
        builder.Append(mantissa[0]);
        if (decimalPlaces > 0)
        {
            builder.Append(info.NumberDecimalSeparator);
            builder.Append(mantissa, 1, decimalPlaces);
        }

        builder.Append(exponentChar);
        builder.Append(exponent < 0 ? info.NegativeSign : info.PositiveSign);
        builder.Append(Math.Abs(exponent).ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'));

        string body = builder.ToString();
        return isNegative ? info.NegativeSign + body : body;
    }

    private static string Integral(Number value, char specifier, int precision, NumberFormatInfo info)
    {
        if (value.DecimalOffset != 0)
            throw new FormatException(
                $"The '{specifier}' format specifier requires an integral value, but {nameof(Number)} has a fractional part.");

        BigInteger signed = value.IsNegative ? -value.RawValue : value.RawValue;
        string bigIntegerFormat = precision < 0
            ? specifier.ToString()
            : specifier + precision.ToString(CultureInfo.InvariantCulture);
        return signed.ToString(bigIntegerFormat, info);
    }

    /// <summary>
    /// Breaks a value into its sign and its magnitude's integer / fraction digit strings.
    /// The integer string never carries leading zeros beyond a single <c>"0"</c>; the fraction
    /// string has no trailing zeros (the significand is already normalised that way).
    /// </summary>
    private static void Decompose(Number value, out bool isNegative, out string integerDigits, out string fractionDigits)
    {
        isNegative = value.IsNegative;
        string magnitude = value.RawValue.ToString(CultureInfo.InvariantCulture);
        int offset = value.DecimalOffset;

        if (offset <= 0)
        {
            integerDigits = magnitude;
            fractionDigits = "";
        }
        else if (offset >= magnitude.Length)
        {
            integerDigits = "0";
            fractionDigits = new string('0', offset - magnitude.Length) + magnitude;
        }
        else
        {
            integerDigits = magnitude[..(magnitude.Length - offset)];
            fractionDigits = magnitude[(magnitude.Length - offset)..];
        }
    }

    /// <summary>
    /// Rounds the magnitude in <paramref name="integerDigits"/> / <paramref name="fractionDigits"/> so
    /// that it keeps exactly <paramref name="scale"/> fractional digits, half-away-from-zero.
    /// <paramref name="scale"/> may be negative, meaning digits are dropped from the integer part.
    /// </summary>
    private static void RoundToScale(ref string integerDigits, ref string fractionDigits, int scale)
    {
        string combined = integerDigits + fractionDigits;
        int pointIndex = integerDigits.Length;
        int keepLength = pointIndex + scale;

        if (keepLength >= combined.Length)
        {
            fractionDigits = scale > 0 ? fractionDigits.PadRight(scale, '0') : "";
            NormalizeInteger(ref integerDigits);
            return;
        }

        if (keepLength < 1)
        {
            int pad = 1 - keepLength;
            combined = new string('0', pad) + combined;
            pointIndex += pad;
            keepLength = 1;
        }

        bool roundUp = combined[keepLength] >= '5';
        char[] kept = combined[..keepLength].ToCharArray();

        if (roundUp)
        {
            int idx = keepLength - 1;
            while (idx >= 0 && kept[idx] == '9')
            {
                kept[idx] = '0';
                idx--;
            }

            if (idx < 0)
            {
                kept = ('1' + new string(kept)).ToCharArray();
                pointIndex++;
            }
            else
            {
                kept[idx]++;
            }
        }

        string rounded = new(kept);
        if (pointIndex >= rounded.Length)
        {
            integerDigits = rounded + new string('0', pointIndex - rounded.Length);
            fractionDigits = "";
        }
        else
        {
            integerDigits = rounded[..pointIndex];
            fractionDigits = rounded[pointIndex..];
        }

        if (scale > 0)
        {
            fractionDigits = fractionDigits.Length >= scale
                ? fractionDigits[..scale]
                : fractionDigits.PadRight(scale, '0');
        }
        else
        {
            fractionDigits = "";
        }

        NormalizeInteger(ref integerDigits);
    }

    private static void NormalizeInteger(ref string integerDigits)
    {
        integerDigits = integerDigits.TrimStart('0');
        if (integerDigits.Length == 0)
            integerDigits = "0";
    }

    private static string ApplyGrouping(string digits, int[] groupSizes, string separator)
    {
        var groups = new List<string>();
        int position = digits.Length;
        int sizeIndex = 0;

        while (position > 0)
        {
            int size = groupSizes[sizeIndex];
            if (size <= 0)
            {
                groups.Insert(0, digits[..position]);
                break;
            }

            int start = Math.Max(0, position - size);
            groups.Insert(0, digits[start..position]);
            position = start;

            if (sizeIndex < groupSizes.Length - 1)
                sizeIndex++;
        }

        return string.Join(separator, groups);
    }

    private static string ApplyPattern(string pattern, string digits, string symbol, string negativeSign)
    {
        var builder = new StringBuilder(pattern.Length + digits.Length + symbol.Length);
        foreach (char token in pattern)
        {
            switch (token)
            {
                case 'n':
                    builder.Append(digits);
                    break;
                case '$':
                case '%':
                    builder.Append(symbol);
                    break;
                case '-':
                    builder.Append(negativeSign);
                    break;
                default:
                    builder.Append(token);
                    break;
            }
        }

        return builder.ToString();
    }

    private static string Pattern(string[] patterns, int index)
        => (uint)index < (uint)patterns.Length ? patterns[index] : patterns[0];

    private static bool IsZero(string integerDigits, string fractionDigits)
    {
        foreach (char c in integerDigits)
        {
            if (c != '0')
                return false;
        }

        foreach (char c in fractionDigits)
        {
            if (c != '0')
                return false;
        }

        return true;
    }

    private static int LeadingZeroCount(string digits)
    {
        int count = 0;
        while (count < digits.Length && digits[count] == '0')
            count++;
        return count;
    }
}
