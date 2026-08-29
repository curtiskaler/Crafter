using System.Reflection.Emit;

namespace Auturge.Identifiers.Internal;

internal static class TypeSizer
{
    public static int GetByteSize<T>() => GetByteSize(typeof(T));

    public static int GetByteSize(Type type)
    {
        var dm = new DynamicMethod("SizeOfType", typeof(int), []);
        ILGenerator il = dm.GetILGenerator();
        il.Emit(OpCodes.Sizeof, type);
        il.Emit(OpCodes.Ret);
        int size = (int)(dm.Invoke(null, null) ?? -1);
        return size == -1 ? throw new ArgumentException("Could not get size of " + type.Name) : size;
    }

    public static int GetBitSize<T>() => GetBitSize(typeof(T));

    public static int GetBitSize(Type type) => GetByteSize(type) * 8;
}
