using System.Numerics;

namespace Auturge.Quantity;

public interface IQuantity
{
    Unit Unit { get; }
}

public interface IQuantity<out T> : IQuantity where T : INumber<T>, IConvertible
{
    T Amount { get; }
}
