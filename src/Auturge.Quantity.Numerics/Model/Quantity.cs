using Auturge.Numerics;
using Auturge.Quantity.Exceptions;

namespace Auturge.Quantity;

public class Quantity(Number amount, Unit unit) : Quantity<Number>(amount, unit), IEquatable<Quantity>
{
    public Quantity(Quantity<Number> q) : this(q.Amount, q.Unit)
    {
    }

    public override Quantity<Number> ConvertTo(Unit targetUnit)
    {
        Quantity<Number> converted = base.ConvertTo(targetUnit);
        return converted;
    }

    #region IEquatable

    public bool Equals(Quantity? other)
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
        return ReferenceEquals(this, obj) || obj is Quantity other && Equals(other);
    }

    public override int GetHashCode() => HashCode.Combine(Amount, Unit);

    public static bool operator ==(Quantity? lhs, Quantity? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Quantity? lhs, Quantity? rhs)
        => !(lhs == rhs);

    #endregion IEquatable

    #region Arithmetic Operators

    public static Quantity operator +(Quantity lhs, Quantity rhs)
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
        return new Quantity(lhs.Amount + rhs.Amount, lhs.Unit);
    }

    public static Quantity operator +(Quantity lhs, Number amt)
    {
        return new Quantity(lhs.Amount + amt, lhs.Unit);
    }

    public static Quantity operator -(Quantity lhs, Quantity rhs)
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
        return new Quantity(lhs.Amount - rhs.Amount, lhs.Unit);
    }

    public static Quantity operator -(Quantity lhs, Number amt)
    {
        return new Quantity(lhs.Amount - amt, lhs.Unit);
    }

    public static Quantity operator *(Quantity lhs, Quantity rhs)
    {
        return new Quantity(lhs.Amount * rhs.Amount, lhs.Unit * rhs.Unit);
    }

    public static Quantity operator *(Quantity qty, Number factor)
    {
        return new Quantity(qty.Amount * factor, qty.Unit);
    }

    public static Quantity operator /(Quantity lhs, Quantity rhs)
    {
        return new Quantity(lhs.Amount / rhs.Amount, lhs.Unit / rhs.Unit);
    }

    public static Quantity operator /(Quantity qty, Number divisor)
    {
        return new Quantity(qty.Amount / divisor, qty.Unit);
    }

    #endregion Arithmetic Operators

    #region Implicit Conversion

    // When I say "make 5", the unit is "each".
    public static implicit operator Quantity(int n) => new Quantity(n, Units.Each);

    #endregion Implicit Conversion
}
