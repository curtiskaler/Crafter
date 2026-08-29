namespace Auturge.Numerics;

internal struct NumberParseBuffer
{
    public NumberParseResult Validity { get; }
    public string DecimalSeparator { get; }
    public bool? Sign { get; }
    public string Value { get; }

    public NumberParseBuffer(NumberParseResult error)
    {
        Validity = error;
        DecimalSeparator = string.Empty;
        Sign = false;
        Value = string.Empty;
    }

    public NumberParseBuffer(string decimalSeparator, bool? sign, string value)
    {
        Validity = NumberParseResult.Success;
        DecimalSeparator = decimalSeparator;
        Value = value;
        Sign = sign;
    }
}
