using System.Diagnostics;
using System.Globalization;

namespace Auturge.Numerics;

// "B" or "b" 	Binary 	                    255 ("b16") -> 0000000011111111
//      A binary string. (integral types only)      

// "X" or "x" 	Hexadecimal                 255 ("x4") -> 00ff
//      A hexadecimal string.       (by Integral types only)

// "D" or "d" 	Decimal                     -1234 ("D6") -> -001234                         
//      Integer digits with optional negative sign. (Integral types only.)

// "R" or "r" 	Round-trip                  -1234567890.12345678 ("R")
//      A string that can round-trip to an identical number. (Single, Double, and BigInteger)

// "C" or "c" 	Currency                    -123.456 ("C3", fr-FR) -> -123,456 €
//      A currency value. (all types)

// "E" or "e" 	Exponential (scientific)    -1052.0329112756 ("e2", en-US) -> -1.05e+003
//      Exponential notation. (all types)

// "F" or "f" 	Fixed-point                 -1234.56 ("F4", en-US) -> -1234.5600
//      Integral and decimal digits with optional negative sign. (all types)

// "G" or "g" 	General                     123.4546 ("G4", sv-SE) -> 123,5
//      The more compact of either fixed-point or scientific notation (all types)

// "N" or "n" 	Number                      -1234.56 ("N3", en-US) -> -1,234.560
//      Integral and decimal digits, group separators, and a decimal separator with optional negative sign. (all types)

// "P" or "p" 	Percent                     -0.39678 ("P1", en-US) -> -39.7 %
//      Number multiplied by 100 and displayed with a percent symbol. (all types)

internal static class Formatting
{
    // "B" or "b" 	Binary 	                    255 ("b16") -> 0000000011111111
    // "X" or "x" 	Hexadecimal                 255 ("x4") -> 00ff
    // "D" or "d" 	Decimal                     -1234 ("D6") -> -001234                         
    private static readonly List<char> _integralOnlyFormats = ['B', 'b', 'X', 'x', 'D', 'd'];
    private static bool IsIntegralFormat(char fmt) => _integralOnlyFormats.Contains(fmt);

    // this is the guy that matters for IFormattable:
    internal static string FormatNumber(Number value, string? format, NumberFormatInfo info)
        => FormatNumber(targetSpan: false, value, format, format, info, default, out _, out _)!;

    // TODO: IMPLEMENT
    private static unsafe string? FormatNumber(
        bool targetSpan, Number value, string? formatString, ReadOnlySpan<char> formatSpan,
        NumberFormatInfo info, Span<char> destination, out int charsWritten, out bool spanSuccess)
    {
        Debug.Assert(formatString == null || formatString.Length == formatSpan.Length);

        var fmt = ParseFormatSpecifier(formatSpan, out var digits);

        var isInteger = Number.IsInteger(value);
        var isIntegerFormat = IsIntegralFormat(fmt);
        if (isIntegerFormat && !isInteger)
            throw new FormatException($"Format {fmt} requires an integer, but the given number is not an integer");

        // format to hex
        if (fmt is 'x' or 'X')
        {
            return ToHex(targetSpan, value, fmt, digits, info, destination, out charsWritten, out spanSuccess);
        }

        // format to binary
        if (fmt is 'b' or 'B')
        {
            return ToBinary(targetSpan, value, fmt, digits, info, destination, out charsWritten, out spanSuccess);
        }

        // if it's an int32, then the most compact form is Decimal.
        
        // TODO: what the actual fuck, bro?
        const bool isInt32 = false;
        if (isInt32)
#pragma warning disable CS0162 // Unreachable code detected
        {
            if (fmt == 'g' || fmt == 'G' || fmt == 'r' || fmt == 'R')
            {
                formatSpan = formatString = digits > 0 ? $"D{digits}" : "D";
            }

            if (targetSpan)
            {
                spanSuccess = value.Sign.TryFormat(destination, out charsWritten, formatSpan, info);
                return null;
            }
            else
            {
                Debug.Assert(formatString != null);
                charsWritten = 0;
                spanSuccess = false;
                return value.Sign.ToString(formatString, info);
            }
        }
#pragma warning restore CS0162 // Unreachable code detected

        charsWritten = 0;
        spanSuccess = true;
        return string.Empty;
    }


    internal static char ParseFormatSpecifier(ReadOnlySpan<char> format, out int digits)
    {
        digits = 0;
        return 'G';
    }


    private static string? ToHex(bool targetSpan, Number value, char format, int digits,
        NumberFormatInfo info, Span<char> destination, out int charsWritten, out bool spanSuccess)
    {
        // "X" or "x" 	Hexadecimal                 255 ("x4") -> 00ff
        //      A hexadecimal string.       (by Integral types only)
        charsWritten = 0;
        spanSuccess = true;
        return null;
    }

    private static string? ToBinary(bool targetSpan, Number value, char format, int digits,
        NumberFormatInfo info, Span<char> destination, out int charsWritten, out bool spanSuccess)
    {
        // "B" or "b" 	Binary 	                    255 ("b16") -> 0000000011111111
        //      A binary string.           (integral types only)
        charsWritten = 0;
        spanSuccess = true;
        return null;
    }


    private const int _charStackBufferSize = 2000;

    // TODO: Are these helpful?  or UN-helpful?
    //  The whole purpose of Number is to have arbitrary length and precision.
    public static int NumberBufferLength => 1000;
    public static int MaxPrecisionCustomFormat => 1000;

    internal static bool TryFormatNumber(Number value, ReadOnlySpan<char> format, NumberFormatInfo info,
        Span<char> destination, out int charsWritten)
    {
        FormatNumber(targetSpan: true, value, null, format, info, destination, out charsWritten, out bool spanSuccess);
        return spanSuccess;
    }


    // var vlb = new ValueListBuilder<char>(stackalloc char[CharStackBufferSize]);
    // var result = FormatNumber(ref vlb, value, format, info) ?? vlb.AsSpan().ToString();
    // vlb.Dispose();
    // return result;


    // TODO: IMPLEMENT
    private static unsafe string? FormatNumber(ref ValueListBuilder<char> vlb, Number value, ReadOnlySpan<char> format,
        NumberFormatInfo info)
    {
        // TODO: IMPLEMENT
        // char fmt = ParseFormatSpecifier(format, out int precision);
        // byte* pDigits = stackalloc byte[NumberBufferLength];
        //
        // if (fmt == '\0')
        // {
        //     precision = Number.MaxPrecisionCustomFormat;
        // }
        //
        // // NumberBuffer number = new NumberBuffer(NumberBufferKind.FloatingPoint, pDigits, TNumber.NumberBufferLength);
        // number.IsNegative = TNumber.IsNegative(value);


        return string.Empty;

        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        // format to currency ('c' or 'C')
        // format to Exponential ('e' or 'E') 
        // format to Fixed-point ('f' or 'F')
        // format to General ('g' or 'G')
        // format to Number ('n' or 'N')
        // format to Percent ('p' or 'P')
        // otherwise, throw a FormatException at runtime    
        // other formats are integral-type ONLY
    }
    
    
    
    
    
}
