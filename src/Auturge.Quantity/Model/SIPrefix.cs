// ReSharper disable InconsistentNaming

using System.Numerics;

namespace Auturge.Quantity;

public class SIPrefix<T> : IHaveNameAndSymbol where T : INumber<T>
{
    public SIPrefix(string displayName, string symbol, T factor, T? divisor = default)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        ArgumentNullException.ThrowIfNull(symbol);

        DisplayName = displayName;
        Symbol = symbol;
        Factor = factor;
        Divisor = divisor;
    }

    // public static SIPrefix<T> CreateInstance(string displayName, string symbol, T factor, T? divisor)
    // {
    //     return new SIPrefix<T>(displayName, symbol, factor, divisor);
    // }

    /// <summary>
    /// The SI prefix name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The SI prefix symbol.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// Factor by which this prefix adjusts the base unit.
    /// </summary>
    public T Factor { get; }

    /// <summary>
    /// Factor by which the base unit adjusts this prefix.
    /// </summary>
    public T? Divisor { get; }
}

