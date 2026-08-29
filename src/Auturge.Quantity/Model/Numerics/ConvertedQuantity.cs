using Auturge.Numerics;

namespace Auturge.Quantity;

internal class ConvertedQuantity(Quantity<Number> qty, Quantity<Number> original)
    : ConvertedQuantity<Number>(qty, original);
