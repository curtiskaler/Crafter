using System.Globalization;
using System.Numerics;

namespace Auturge.Numerics;

internal enum ConversionResult
{
    Success,
    NumberIsFloat,
    NumberIsSigned,
    TargetTypeOverflow,
    UnsupportedType,
}

public partial struct Number : IConvertible
{
    public TypeCode GetTypeCode() => TypeCode.Object;

    public bool ToBoolean(IFormatProvider? provider = null) => (bool)ToType(typeof(bool), provider);

    public byte ToByte(IFormatProvider? provider = null) => (byte)ToType(typeof(byte), provider);

    public char ToChar(IFormatProvider? provider = null) => (char)ToType(typeof(char), provider);

    public DateTime ToDateTime(IFormatProvider? provider = null) => (DateTime)ToType(typeof(DateTime), provider);

    public decimal ToDecimal(IFormatProvider? provider = null) => (decimal)ToType(typeof(decimal), provider);

    public double ToDouble(IFormatProvider? provider = null) => (double)ToType(typeof(double), provider);

    public short ToInt16(IFormatProvider? provider = null) => (short)ToType(typeof(short), provider);

    public int ToInt32(IFormatProvider? provider = null) => (int)ToType(typeof(int), provider);

    public long ToInt64(IFormatProvider? provider = null) => (long)ToType(typeof(long), provider);

    public sbyte ToSByte(IFormatProvider? provider = null) => (sbyte)ToType(typeof(sbyte), provider);

    public float ToSingle(IFormatProvider? provider = null) => (float)ToType(typeof(float), provider);

    public T ToType<T>(IFormatProvider? provider = null)
    {
        return (T)ToType(typeof(T), provider);
    }

    public object ToType(Type conversionType, IFormatProvider? provider = null)
    {
        ConversionResult? conversionResult = TryConvert(this, conversionType, provider, out object? result);
        return conversionResult switch
        {
            ConversionResult.Success => result!,
            ConversionResult.NumberIsFloat => throw new InvalidCastException(
                $"Cannot convert Number [{ToString(NumberFormatInfo.InvariantInfo)}] to {conversionType.Name}. This number is a floating-point number, and the target type is an integral type."),
            ConversionResult.NumberIsSigned => throw new InvalidCastException(
                $"Cannot convert Number [{ToString(NumberFormatInfo.InvariantInfo)}] to {conversionType.Name}. This number is an signed, and the target type is not."),
            ConversionResult.TargetTypeOverflow => throw new InvalidCastException(
                $"Cannot convert Number [{ToString(NumberFormatInfo.InvariantInfo)}] to {conversionType.Name}. The smallest type that can fit this number without losing precision is {SmallestType.Name}."),
            ConversionResult.UnsupportedType => throw new InvalidCastException(
                $"Cannot convert Number [{ToString(NumberFormatInfo.InvariantInfo)}] to {conversionType.Name}: {conversionType.Name} is an unsupported type."),
            _ => throw new InvalidCastException($"Cannot convert from Number to {conversionType.Name}"),
        };
    }

    public ushort ToUInt16(IFormatProvider? provider = null) => (ushort)ToType(typeof(ushort), provider);

    public uint ToUInt32(IFormatProvider? provider = null) => (uint)ToType(typeof(uint), provider);

    public ulong ToUInt64(IFormatProvider? provider = null) => (ulong)ToType(typeof(ulong), provider);

    public string ToString(IFormatProvider? provider)
    {
        return ToString();

        // TODO: Fix this to use this guy:
        // return Formatting.FormatNumber(this, null, NumberFormatInfo.GetInstance(provider));
    }

    private static ConversionResult? TryConvert(Number number, Type conversionType, IFormatProvider? provider,
        out object? value)
    {
        value = null;

        if (conversionType == typeof(object) || conversionType == typeof(Number))
        {
            value = number;
            return ConversionResult.Success;
        }

        if (!IsSupportedType(conversionType))
            return ConversionResult.UnsupportedType;

        var isInteger = IsInteger(number);
        if (!isInteger && IsTypeIntegral(conversionType))
        {
            return ConversionResult.NumberIsFloat;
        }

        var isSigned = number.Sign == -1;
        if (isSigned && IsUnsigned(conversionType))
        {
            return ConversionResult.NumberIsSigned;
        }

        var bitSize = GetBitSize(number.SmallestType);
        var requiredBitSize = GetBitSize(conversionType);
        if (requiredBitSize < bitSize)
        {
            return ConversionResult.TargetTypeOverflow;
        }

        // Try to string parse the value. Any problems are also overflow.
        var str = number.ToString(NumberFormatInfo.InvariantInfo);

        try
        {
            if (conversionType == typeof(bool)) value = number != Zero;
            if (conversionType == typeof(byte)) value = Convert.ToByte(str);
            if (conversionType == typeof(sbyte)) value = Convert.ToSByte(str);
            if (conversionType == typeof(short)) value = Convert.ToInt16(str);
            if (conversionType == typeof(ushort)) value = Convert.ToUInt16(str);
            if (conversionType == typeof(char)) value = Convert.ToChar(str);
            if (conversionType == typeof(int)) value = Convert.ToInt32(str);
            if (conversionType == typeof(uint)) value = Convert.ToUInt32(str);
            if (conversionType == typeof(long)) value = Convert.ToInt64(str);
            if (conversionType == typeof(ulong)) value = Convert.ToUInt64(str);
            if (conversionType == typeof(Int128)) value = Int128.Parse(str);
            if (conversionType == typeof(UInt128)) value = UInt128.Parse(str);
            if (conversionType == typeof(BigInteger)) value = BigInteger.Parse(str);

            if (conversionType == typeof(decimal)) value = Convert.ToDecimal(str);
            if (conversionType == typeof(double)) value = Convert.ToDouble(str);
            if (conversionType == typeof(float)) value = Convert.ToSingle(str);
            if (conversionType == typeof(Half)) value = Half.Parse(str);

            if (value == null) return ConversionResult.UnsupportedType;
        }
        catch (Exception)
        {
            return ConversionResult.TargetTypeOverflow;
        }

        return ConversionResult.Success;
    }

    private static bool IsSupportedType(Type type)
    {
        return type == typeof(object) ||
               type == typeof(bool) ||
               type == typeof(byte) ||
               type == typeof(sbyte) ||
               type == typeof(short) ||
               type == typeof(ushort) ||
               type == typeof(int) ||
               type == typeof(uint) ||
               type == typeof(long) ||
               type == typeof(ulong) ||
               type == typeof(Half) ||
               type == typeof(Int128) ||
               type == typeof(UInt128) ||
               type == typeof(BigInteger) ||
               type == typeof(char) ||
               type == typeof(decimal) ||
               type == typeof(double) ||
               type == typeof(float);
    }

    private static bool IsTypeIntegral(Type type)
    {
        return type == typeof(byte) ||
               type == typeof(sbyte) ||
               type == typeof(short) ||
               type == typeof(ushort) ||
               type == typeof(int) ||
               type == typeof(uint) ||
               type == typeof(long) ||
               type == typeof(ulong) ||
               type == typeof(Int128) ||
               type == typeof(UInt128) ||
               type == typeof(BigInteger) ||
               type == typeof(char); // Char is an integral type in C#
    }

    private static bool IsUnsigned(Type type)
    {
        return type == typeof(byte) ||
               type == typeof(char) ||
               type == typeof(ushort) ||
               type == typeof(uint) ||
               type == typeof(ulong) ||
               type == typeof(UInt128); // Char is an integral type in C#
    }
}
