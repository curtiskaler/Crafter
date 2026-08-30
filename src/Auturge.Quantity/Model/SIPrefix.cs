// ReSharper disable InconsistentNaming

using System.Numerics;

namespace Auturge.Quantity;

// T is constrained to just the arithmetic SIPrefix<T> actually needs (multiply, divide, and a
// multiplicative identity for the no-divisor overload below), not the full generic-math surface —
// any numeric type implementing this small subset can be used, not only the built-in ones.
public class SIPrefix<T> : IHaveNameAndSymbol
    where T : IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IMultiplicativeIdentity<T, T>
{
    // Two overloads rather than a `T? divisor = default` parameter: for an interface-only T,
    // `T?` is a plain non-nullable `T`, so an omitted divisor would silently become `default(T)`
    // (all-zero-bits) instead of "no divisor effect".
    public SIPrefix(string displayName, string symbol, T factor, T divisor)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        ArgumentNullException.ThrowIfNull(symbol);

        DisplayName = displayName;
        Symbol = symbol;
        Factor = factor;
        Divisor = divisor;
    }

    public SIPrefix(string displayName, string symbol, T factor)
        : this(displayName, symbol, factor, T.MultiplicativeIdentity)
    {
    }

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
    public T Divisor { get; }
}
