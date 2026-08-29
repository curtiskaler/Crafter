using Auturge.Numerics;

// ReSharper disable InconsistentNaming

namespace Auturge.Quantity;

public class SIPrefix(string displayName, string symbol, Number factor, Number? divisor = null)
    : SIPrefix<Number>(displayName, symbol, factor, divisor ?? 1);
