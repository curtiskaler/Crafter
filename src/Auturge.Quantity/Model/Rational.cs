using System.Globalization;
using System.Numerics;

namespace Auturge.Quantity;

/// <summary>
/// An exact rational number (<see cref="BigInteger"/> numerator / <see cref="BigInteger"/> denominator),
/// always stored fully reduced with a positive denominator.
/// <para/>
/// Used where "arbitrary precision" is wanted but a generic <c>INumber&lt;T&gt;</c> parameter isn't
/// possible (e.g. a stored field on a shared, non-generic type like <see cref="Unit"/>). Unlike
/// <see cref="decimal"/>, it has no range ceiling; unlike <see cref="double"/>, it has no rounding error.
/// </summary>
public readonly struct Rational : IEquatable<Rational>,
    IMultiplyOperators<Rational, Rational, Rational>,
    IDivisionOperators<Rational, Rational, Rational>,
    IMultiplicativeIdentity<Rational, Rational>
{
    public BigInteger Numerator { get; }
    public BigInteger Denominator { get; }

    public Rational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator == 0)
            throw new DivideByZeroException("A Rational cannot have a zero denominator.");

        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        if (numerator == 0)
        {
            denominator = 1;
        }
        else
        {
            BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
            if (gcd > 1)
            {
                numerator /= gcd;
                denominator /= gcd;
            }
        }

        Numerator = numerator;
        Denominator = denominator;
    }

    public static readonly Rational Zero = new(0, 1);
    public static Rational One => new(1, 1);
    public static Rational MultiplicativeIdentity => One;

    public static implicit operator Rational(int value) => new(value, 1);
    public static implicit operator Rational(long value) => new(value, 1);
    public static implicit operator Rational(BigInteger value) => new(value, 1);
    public static implicit operator Rational(double value) => FromDouble(value);

    public Rational Reciprocal() => new(Denominator, Numerator);

    /// <summary> Narrowing, lossy conversion — for display/debugging/interop only. </summary>
    public static explicit operator double(Rational r) => (double)r.Numerator / (double)r.Denominator;

    #region Arithmetic Operators

    public static Rational operator *(Rational a, Rational b)
        => new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

    public static Rational operator /(Rational a, Rational b)
        => new(a.Numerator * b.Denominator, a.Denominator * b.Numerator);

    public static Rational operator +(Rational a, Rational b)
        => new(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);

    public static Rational operator -(Rational a, Rational b)
        => new(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);

    public static Rational operator -(Rational a) => new(-a.Numerator, a.Denominator);

    #endregion Arithmetic Operators

    #region Equality / Comparison

    public static bool operator ==(Rational a, Rational b) => a.Numerator == b.Numerator && a.Denominator == b.Denominator;
    public static bool operator !=(Rational a, Rational b) => !(a == b);
    public static bool operator <(Rational a, Rational b) => a.Numerator * b.Denominator < b.Numerator * a.Denominator;
    public static bool operator >(Rational a, Rational b) => a.Numerator * b.Denominator > b.Numerator * a.Denominator;
    public static bool operator <=(Rational a, Rational b) => !(a > b);
    public static bool operator >=(Rational a, Rational b) => !(a < b);

    public bool Equals(Rational other) => this == other;
    public override bool Equals(object? obj) => obj is Rational other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    #endregion Equality / Comparison

    public override string ToString()
        => Denominator == 1
            ? Numerator.ToString(CultureInfo.InvariantCulture)
            : $"{Numerator.ToString(CultureInfo.InvariantCulture)}/{Denominator.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>
    /// Converts this rational into any numeric type supporting .NET generic math, via
    /// <see cref="INumberBase{TSelf}.CreateChecked{TOther}"/> on both the numerator and denominator.
    /// </summary>
    public T To<T>() where T : INumberBase<T> => T.CreateChecked(Numerator) / T.CreateChecked(Denominator);

    /// <summary>
    /// Builds an exact <see cref="Rational"/> from a <see cref="decimal"/>'s own internal
    /// (unscaled value, scale) representation, via <see cref="decimal.GetBits"/> — exact, and
    /// simpler than approximating via a Stern-Brocot search since decimal already IS a scaled integer.
    /// </summary>
    public static Rational FromDecimal(decimal value)
    {
        int[] bits = decimal.GetBits(value);
        BigInteger unscaled = (BigInteger)(uint)bits[0]
                               | ((BigInteger)(uint)bits[1] << 32)
                               | ((BigInteger)(uint)bits[2] << 64);
        if ((bits[3] & 0x80000000) != 0) unscaled = -unscaled;

        int scale = (bits[3] >> 16) & 0x7F;
        return new Rational(unscaled, BigInteger.Pow(10, scale));
    }

    /// <summary>
    /// Builds an exact <see cref="Rational"/> from a double's shortest round-trip decimal string,
    /// not its binary bit pattern — a literal like 0.45359237 is an exact decimal definition and
    /// should be captured as exactly that, not as the nearby binary fraction the bits actually hold.
    /// </summary>
    private static Rational FromDouble(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Cannot represent NaN or Infinity as a Rational.", nameof(value));

        return ParseDecimalString(value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Parses a plain-or-scientific decimal string (as produced by <see cref="double.ToString()"/>
    /// under <see cref="CultureInfo.InvariantCulture"/>) into an exact <see cref="Rational"/>,
    /// including exponent signs (e.g. "1E+30", "5.29E-11").
    /// </summary>
    private static Rational ParseDecimalString(string text)
    {
        int i = 0;
        bool negative = false;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            negative = text[i] == '-';
            i++;
        }

        BigInteger mantissa = 0;
        int fractionalDigits = 0;
        bool sawDot = false;
        for (; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '.')
            {
                sawDot = true;
                continue;
            }

            if (c is 'e' or 'E') break;

            if (c is < '0' or > '9')
                throw new FormatException($"Unexpected character '{c}' in numeric string \"{text}\".");

            mantissa = mantissa * 10 + (c - '0');
            if (sawDot) fractionalDigits++;
        }

        int exponent = 0;
        if (i < text.Length && (text[i] == 'e' || text[i] == 'E'))
        {
            i++;
            bool exponentNegative = false;
            if (i < text.Length && (text[i] == '+' || text[i] == '-'))
            {
                exponentNegative = text[i] == '-';
                i++;
            }

            int exponentValue = 0;
            for (; i < text.Length; i++)
            {
                char c = text[i];
                if (c is < '0' or > '9')
                    throw new FormatException($"Unexpected character '{c}' in exponent of \"{text}\".");
                exponentValue = exponentValue * 10 + (c - '0');
            }

            exponent = exponentNegative ? -exponentValue : exponentValue;
        }

        if (negative) mantissa = -mantissa;

        // The full value is mantissa * 10^(exponent - fractionalDigits).
        int netExponent = exponent - fractionalDigits;
        return netExponent >= 0
            ? new Rational(mantissa * BigInteger.Pow(10, netExponent), 1)
            : new Rational(mantissa, BigInteger.Pow(10, -netExponent));
    }
}
