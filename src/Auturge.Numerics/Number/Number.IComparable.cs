using System.Numerics;

namespace Auturge.Numerics;

public partial struct Number : IComparable, IComparable<Number>
{
    public int CompareTo(object? value)
    {
        return value switch
        {
            null => 1,
            Number n when this < n => -1,
            Number n when this > n => 1,
            Number => 0,
            _ => throw new ArgumentException("Argument must be a Number.")
        };
    }

    public int CompareTo(Number other)
    {
        if (IsNegative != other.IsNegative)
            return IsNegative ? -1 : 1;

        var offsetL = this.DecimalOffset;
        var offsetR = other.DecimalOffset;
        var maxIndex = BigInteger.Max(offsetL, offsetR);
        var intL = MakeItHaveThisManyDigits(this, maxIndex);
        var intR = MakeItHaveThisManyDigits(other, maxIndex);

        if (IsNegative) return intR.CompareTo(intL);
        return IsNegative ? intR.CompareTo(intL) : intL.CompareTo(intR);
    }
}

public partial struct Number : IComparisonOperators<Number, Number, bool>
{
    public static bool operator >(Number a, Number b)
    {
        return a.CompareTo(b) > 0;
    }

    public static bool operator >=(Number a, Number b)
    {
        return a.CompareTo(b) >= 0;
    }

    public static bool operator <(Number a, Number b)
    {
        return a.CompareTo(b) < 0;
    }

    public static bool operator <=(Number a, Number b)
    {
        return a.CompareTo(b) <= 0;
    }
}

public partial struct Number : IEqualityOperators<Number, Number, bool>
{
    public static bool operator ==(Number a, Number b)
    {
        return
            a.RawValue == b.RawValue &&
            a.DecimalOffset == b.DecimalOffset &&
            a.IsNegative == b.IsNegative;
    }

    public static bool operator !=(Number a, Number b) => !(a == b);

    public static bool operator ==(Number a, double b)
    {
        return a == new Number(b);
    }

    public static bool operator !=(Number a, double b)
    {
        return !(a == b);
    }

    public static bool operator ==(Number a, int b)
    {
        return a == new Number(b);
    }

    public static bool operator !=(Number a, int b)
    {
        return !(a == b);
    }
}
