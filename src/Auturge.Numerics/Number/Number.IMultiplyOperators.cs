using System.Numerics;

namespace Auturge.Numerics;

public partial struct Number : IMultiplicativeIdentity<Number, Number>,
    IDivisionOperators<Number, Number, Number>,
    IMultiplyOperators<Number, Number, Number>,
    IModulusOperators<Number, Number, Number>
{
    public static Number MultiplicativeIdentity => One;
    static Number IMultiplicativeIdentity<Number, Number>.MultiplicativeIdentity => MultiplicativeIdentity;

    internal static readonly BigInteger DefaultFractionalDigitCount = new(8);

    public static Number operator *(Number left, Number right)
    {
        // It doesn't matter who is negative... as long as one is negative and the
        // other is not, we must make the result negative.
        var isNegative = (left.IsNegative != right.IsNegative);
        var product = left.RawValue * right.RawValue;
        if (isNegative)
            product = -product;

        var offset = left.DecimalOffset + right.DecimalOffset;

        return new Number(product, offset);
    }

    public static Number operator /(Number dividend, Number divisor)
    {
        return Divide(dividend, divisor, DefaultFractionalDigitCount);
    }

    public static Number Divide(Number dividend, Number divisor, BigInteger fractionalDigits)
    {
        if (fractionalDigits < 0)
            throw new ArgumentOutOfRangeException(nameof(fractionalDigits));

        if (divisor.RawValue == 0)
        {
            // This rule might look odd, but when simplifying expressions, x/x (x divided by x) is 1.
            // So, to keep the rule true, 0 divided by 0 is also 1.
            return dividend.RawValue == 0
                ? One
                : throw new DivideByZeroException($"{nameof(divisor)} can only be zero if {nameof(dividend)} is zero.");
        }

        var maxDigitCount = BigInteger.Max(dividend.DecimalOffset, divisor.DecimalOffset);
        var finalFloatCount = maxDigitCount + fractionalDigits;
        var intDividend = MakeItHaveThisManyDigits(dividend, finalFloatCount);
        var intDivisor = MakeItHaveThisManyDigits(divisor, maxDigitCount);

        // It doesn't matter who is negative... as long as one is negative and the
        // other is not, we must make the result negative.
        var isResultNegative = (dividend.IsNegative != divisor.IsNegative);
        var result = intDividend / intDivisor;
        if (isResultNegative)
            result = -result;

        return new Number(result, finalFloatCount - maxDigitCount);
    }

    static Number IModulusOperators<Number, Number, Number>.operator %(Number dividend, Number divisor)
        => Remainder(dividend, divisor, 0);

    public static Number operator %(Number dividend, Number divisor) 
        => Remainder(dividend, divisor, 0);
    
    private static Number Remainder(Number dividend, Number divisor, BigInteger fractionalDigits)
    {
        var divisionResult = Divide(dividend, divisor, fractionalDigits);
        var result = dividend - divisionResult * divisor;
        return result;
    }
}
