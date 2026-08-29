using System.Numerics;

namespace Auturge.Numerics;

public partial struct Number : IAdditionOperators<Number, Number, Number>
{
    public static Number operator +(Number left, Number right)
    {
        var offsetL = left.DecimalOffset;
        var offsetR = right.DecimalOffset;
        var maxIndex = BigInteger.Max(offsetL, offsetR);
        var intL = MakeItHaveThisManyDigits(left, maxIndex);
        var intR = MakeItHaveThisManyDigits(right, maxIndex);

        if (left.IsNegative)
            intL = -intL;

        if (right.IsNegative)
            intR = -intR;

        var sum = intL + intR;
        return new Number(sum, maxIndex);
    }
}

public partial struct Number : IIncrementOperators<Number>
{
    static Number IIncrementOperators<Number>.operator ++(Number value) => value + One;
}

public partial struct Number : IDecrementOperators<Number>
{
    static Number IDecrementOperators<Number>.operator --(Number value) => value - One;
}

public partial struct Number : IUnaryPlusOperators<Number, Number>
{

    public static Number operator +(Number value) => value;
}

public partial struct Number : IUnaryNegationOperators<Number, Number>
{

    public static Number operator -(Number value) => new(-value.RawValue, value.DecimalOffset);
}

public partial struct Number : ISubtractionOperators<Number, Number, Number>
{
    public static Number operator -(Number left, Number right)
    {
        var offsetL = left.DecimalOffset;
        var offsetR = right.DecimalOffset;
        var maxIndex = BigInteger.Max(offsetL, offsetR);
        var intL = MakeItHaveThisManyDigits(left, maxIndex);
        var intR = MakeItHaveThisManyDigits(right, maxIndex);

        if (left.IsNegative)
            intL = -intL;

        if (right.IsNegative)
            intR = -intR;

        var subtraction = intL - intR;
        return new Number(subtraction, maxIndex);
    }
}
