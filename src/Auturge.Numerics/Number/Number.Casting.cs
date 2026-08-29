namespace Auturge.Numerics;

public partial struct Number
{
    public static explicit operator byte(Number number) => (byte)number.Floor().RawValue;

    public static explicit operator checked byte(Number number) => (byte)number.Floor().RawValue;

    public static explicit operator uint(Number number) => (uint)number.Floor().RawValue;

    public static explicit operator checked uint(Number number) => (uint)number.Floor().RawValue;

    public static explicit operator ulong(Number number) => (ulong)number.Floor().RawValue;

    public static explicit operator checked ulong(Number number) => (ulong)number.Floor().RawValue;

    public static explicit operator ushort(Number number) => (ushort)number.Floor().RawValue;

    public static explicit operator checked ushort(Number number) => (ushort)number.Floor().RawValue;

    public static explicit operator UInt128(Number number) => (UInt128)number.Floor().RawValue;

    public static explicit operator checked UInt128(Number number) => (UInt128)number.Floor().RawValue;
}
