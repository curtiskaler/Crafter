namespace Auturge.Numerics;

internal struct NumberParseBuffer
{
    public NumberParseResult Validity { get; }
    public string DecimalSeparator { get; }
    public bool? Sign { get; }
    public string Value { get; }

    /// <summary>
    /// The base-10 exponent from scientific notation (e.g. <c>+10</c> for <c>"1.5E+10"</c>),
    /// or <c>0</c> when the source had no exponent.
    /// </summary>
    public int Exponent { get; }

    public NumberParseBuffer(NumberParseResult error)
    {
        Validity = error;
        DecimalSeparator = string.Empty;
        Sign = false;
        Value = string.Empty;
        Exponent = 0;
    }

    public NumberParseBuffer(string decimalSeparator, bool? sign, string value, int exponent = 0)
    {
        Validity = NumberParseResult.Success;
        DecimalSeparator = decimalSeparator;
        Value = value;
        Sign = sign;
        Exponent = exponent;
    }
}
