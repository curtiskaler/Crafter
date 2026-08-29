using System.Globalization;
using System.Text;

namespace Auturge.Numerics;

internal static class ParsingExtensions
{
    internal static bool IsWhite(this uint ch) => (ch == 0x20) || ((ch - 0x09) <= (0x0D - 0x09));

    internal static bool IsDigit(this uint ch) => (ch - '0') <= 9;

    internal static bool? TrimSign(this string strValue, NumberFormatInfo info, out string unsigned)
    {
        string pos = info.PositiveSign;
        string neg = info.NegativeSign;

        string noPos = strValue.Contains(pos)
            ? strValue.Replace(pos, string.Empty)
            : strValue;

        unsigned = noPos.Contains(neg)
            ? strValue.Replace(neg, string.Empty)
            : noPos;

        return strValue.Contains(pos) ? true : strValue.Contains(neg) ? false : null;
    }

    internal static bool TrimCurrency(this string value, NumberFormatInfo info, out string trimmed)
    {
        var isCurrency = false;
        // it's currency if either:
        //  it starts with a sign+currency symbol, or
        //  it starts with a currency symbol

        //... 
        var symbol = info.CurrencySymbol;
        if (value.Contains(symbol))
        {
            isCurrency = true;
        }

        var noSymbol = value.Replace(symbol, "");

        // trim any whiteSpaces
        var builder = new StringBuilder(noSymbol.Length);
        foreach (var c in noSymbol.Where(c => !char.IsWhiteSpace(c)))
        {
            builder.Append(c);
        }

        trimmed = builder.ToString();
        return isCurrency;
    }
}
