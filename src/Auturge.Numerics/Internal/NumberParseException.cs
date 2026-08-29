namespace Auturge.Numerics;

internal class NumberParseException : ArgumentException
{
    public string Value { get; }

    public NumberParseException(ReadOnlySpan<char> value) : this(null, value)
    {
    }

    public NumberParseException(string value) : this(null, value)
    {
    }

    public NumberParseException(string? message, ReadOnlySpan<char> value) : this(message, value.ToString())
    {
    }

    public NumberParseException(string? message, string value) : base(message)
    {
        Value = value;
    }
}
