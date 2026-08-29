// ReSharper disable ReplaceWithPrimaryConstructorParameter
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToAutoProperty

using System.Diagnostics;
using System.Numerics;
using Auturge.Quantity.Exceptions;

namespace Auturge.Quantity;

[DebuggerDisplay("{Amount} {Unit.Symbol}")]
public class Quantity<T>(T amount, Unit unit)
    : IQuantity<T>, IEquatable<Quantity<T>> where T : IEquatable<T>, INumber<T>, IConvertible
{
    public Unit Unit { get; } = unit;
    public T Amount { get; } = amount;

    public override string ToString() => $@"{Amount} {Unit.Symbol}";


    public virtual Quantity<T> ConvertTo(Unit targetUnit)
    {
        if (Unit.Dimension != targetUnit.Dimension)
        {
            throw new ArgumentException(RS.EX_MismatchedDimensions);
        }

        UnitConversion<T> conversion = UnitConversions<T>.Find(Unit, targetUnit);

        // Now make sure we're going the right direction
        UnitConversion<T> converter = conversion.TargetUnit == targetUnit ? conversion : conversion.Invert();

        T newAmount = converter.Convert(Amount);

        return new ConvertedQuantity<T>(newAmount, targetUnit, this);
    }

    #region IEquatable

    public bool Equals(Quantity<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        bool amountEqual = Amount.Equals(other.Amount);
        bool unitEqual = Unit.Equals(other.Unit);
        return amountEqual && unitEqual;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != GetType()) return false;
        return ReferenceEquals(this, obj) || obj is Quantity<T> other && Equals(other);
    }

    public override int GetHashCode() => HashCode.Combine(Amount, Unit);

    public static bool operator ==(Quantity<T>? lhs, Quantity<T>? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Quantity<T>? lhs, Quantity<T>? rhs)
        => !(lhs == rhs);

    #endregion IEquatable

    #region Arithmetic Operators

    public static Quantity<T> operator +(Quantity<T> lhs, Quantity<T> rhs)
    {
        // make sure they're compatible.
        if (lhs.Unit.Dimension != rhs.Unit.Dimension)
        {
            throw new IncompatibleUnitTypeException("+", lhs, rhs);
        }

        if (lhs.Unit != rhs.Unit)
        {
            throw new IncompatibleUnitException("+", lhs, rhs);
        }

        // var (left, right) = QuantityConverter.FindDimensionalMatches(lhs, rhs);
        return new Quantity<T>(lhs.Amount + rhs.Amount, lhs.Unit);
    }

    public static Quantity<T> operator +(Quantity<T> lhs, T amt)
        => new(lhs.Amount + amt, lhs.Unit);

    public static Quantity<T> operator -(Quantity<T> lhs, Quantity<T> rhs)
    {
        // make sure they're compatible.
        if (lhs.Unit.Dimension != rhs.Unit.Dimension)
        {
            throw new IncompatibleUnitTypeException("-", lhs, rhs);
        }

        if (lhs.Unit != rhs.Unit)
        {
            throw new IncompatibleUnitException("-", lhs, rhs);
        }

        // var (left, right) = QuantityConverter.FindDimensionalMatches(lhs, rhs);
        return new Quantity<T>(lhs.Amount - rhs.Amount, lhs.Unit);
    }

    public static Quantity<T> operator -(Quantity<T> lhs, T amt)
    {
        return new Quantity<T>(lhs.Amount - amt, lhs.Unit);
    }

    public static Quantity<T> operator *(Quantity<T> lhs, Quantity<T> rhs)
    {
        return new Quantity<T>(lhs.Amount * rhs.Amount, lhs.Unit * rhs.Unit);
    }

    public static Quantity<T> operator *(Quantity<T> qty, T factor)
    {
        return new Quantity<T>(qty.Amount * factor, qty.Unit);
    }

    public static Quantity<T> operator /(Quantity<T> lhs, Quantity<T> rhs)
    {
        return new Quantity<T>(lhs.Amount / rhs.Amount, lhs.Unit / rhs.Unit);
    }

    public static Quantity<T> operator /(Quantity<T> qty, T divisor)
    {
        return new Quantity<T>(qty.Amount / divisor, qty.Unit);
    }

    #endregion Arithmetic Operators
}
