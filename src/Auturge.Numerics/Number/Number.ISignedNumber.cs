using System.Numerics;

namespace Auturge.Numerics;

public partial struct Number : ISignedNumber<Number>, IAdditiveIdentity<Number, Number>
{
    /// <summary> Gets the value <c>-1</c> for the type. </summary>
    static Number ISignedNumber<Number>.NegativeOne => new(-1L);
    
    public static Number AdditiveIdentity => new(BigInteger.Zero);
}
