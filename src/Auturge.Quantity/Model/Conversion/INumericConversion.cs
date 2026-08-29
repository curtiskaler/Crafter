using System.Numerics;

namespace Auturge.Quantity;

public interface INumericConversion<T> : IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>
    where T : INumericConversion<T>
{
}
