using System.Numerics;

namespace Auturge.Numerics;

public partial struct Number
{
    // The integer part as a SIGNED BigInteger. RawValue on its own is the magnitude, so casting
    // a negative Number straight from RawValue would drop the sign — a negative value must still
    // wrap (unchecked) or throw (checked) the same way (byte)(-1) does for the BCL integers.
    private BigInteger FlooredValue()
    {
        Number floored = this.Floor();
        return floored.IsNegative ? -floored.RawValue : floored.RawValue;
    }

    public static explicit operator byte(Number number) => (byte)number.FlooredValue();

    public static explicit operator checked byte(Number number) => checked((byte)number.FlooredValue());

    public static explicit operator uint(Number number) => (uint)number.FlooredValue();

    public static explicit operator checked uint(Number number) => checked((uint)number.FlooredValue());

    public static explicit operator ulong(Number number) => (ulong)number.FlooredValue();

    public static explicit operator checked ulong(Number number) => checked((ulong)number.FlooredValue());

    public static explicit operator ushort(Number number) => (ushort)number.FlooredValue();

    public static explicit operator checked ushort(Number number) => checked((ushort)number.FlooredValue());

    public static explicit operator UInt128(Number number) => (UInt128)number.FlooredValue();

    public static explicit operator checked UInt128(Number number) => checked((UInt128)number.FlooredValue());
}
