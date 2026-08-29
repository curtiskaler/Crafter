using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Auturge.Numerics;

public partial struct Number : ISpanFormattable
{
    public bool TryFormat(Span<char> destination, out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        => Formatting.TryFormatNumber(this, format, NumberFormatInfo.GetInstance(provider), destination,
            out charsWritten);
}

public partial struct Number : IFormattable
{
    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
        => Formatting.FormatNumber(this, format, NumberFormatInfo.CurrentInfo); // not sure where this override comes from
    
    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format,
        IFormatProvider? provider)
        => Formatting.FormatNumber(this, format, NumberFormatInfo.GetInstance(provider));
}
