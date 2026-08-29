namespace Auturge.Numerics;

public static class NumberExtensions
{
    /// <summary>
    /// Chops off all decimal places, returning the integer part.
    /// Similar to Math.Floor.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static Number Floor(this Number number) => Number.Round(number, 0, MidpointRounding.ToNegativeInfinity);

    public static Number Truncate(this Number x) => Number.Round(x, digits: 0, MidpointRounding.ToZero);
    public static Number TruncateTo(this Number number, int fractionalDigits)
        => Number.TruncateTo(number, fractionalDigits);

    public static Number Round(this Number number, int digits, MidpointRounding mode)
        => Number.Round(number, digits, mode);
    
    /// <summary>
    /// Returns a value indicating whether a <see cref="Number"/> can be converted to a given <paramref name="type" />.
    /// </summary>
    /// <param name="number">The number to check.</param>
    /// <param name="type">The type to try to stuff the number into.</param>
    /// <returns></returns>
    public static bool ConvertsTo(this Number number, Type type) 
        => Number.ConvertsTo(number, type);
}
