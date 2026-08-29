using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Auturge.Numerics;

internal ref struct ValueListBuilder<T> : IDisposable
{
    private Span<T> _span;
    private T[]? _arrayFromPool;
    private int _pos;

    public ValueListBuilder(Span<T?> scratchBuffer)
    {
        _span = scratchBuffer!;
    }

    public ValueListBuilder(int capacity)
    {
        Grow(capacity);
    }

    public int Length
    {
        get => _pos;
        set
        {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= _span.Length);
            _pos = value;
        }
    }

    public ref T this[int index]
    {
        get
        {
            Debug.Assert(index < _pos);
            return ref _span[index];
        }
    }

    public ReadOnlySpan<T> AsSpan()
    {
        return _span.Slice(0, _pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        var toReturn = _arrayFromPool;
        if (toReturn == null) return;

        _arrayFromPool = null;

#if SYSTEM_PRIVATE_CORELIB
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    ArrayPool<T>.Shared.Return(toReturn, _pos);
                }
                else
                {
                    ArrayPool<T>.Shared.Return(toReturn);
                }
#else
        if (!typeof(T).IsPrimitive)
        {
            Array.Clear(toReturn, 0, _pos);
        }

        ArrayPool<T>.Shared.Return(toReturn);
#endif
    }

    // Note that consuming implementations depend on the list only growing if it's absolutely
    // required.  If the list is already large enough to hold the additional items be added,
    // it must not grow. The list is used in a number of places where the reference is checked,
    // and it's expected to match the initial reference provided to the constructor if that
    // span was sufficiently large.
    private void Grow(int additionalCapacityRequired = 1)
    {
        const int arrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        // Double the size of the span.  If it's currently empty, default to size 4,
        // although it'll be increased in Rent to the pool's minimum bucket size.
        int nextCapacity = Math.Max(_span.Length != 0 ? _span.Length * 2 : 4,
            _span.Length + additionalCapacityRequired);

        // If the computed doubled capacity exceeds the possible length of an array, then we
        // want to downgrade to either the maximum array length if that's large enough to hold
        // an additional item, or the current length + 1 if it's larger than the max length, in
        // which case it'll result in an OOM when calling Rent below.  In the exceedingly rare
        // case where _span.Length is already int.MaxValue (in which case it couldn't be a managed
        // array), just use that same value again and let it OOM in Rent as well.
        if ((uint)nextCapacity > arrayMaxLength)
        {
            nextCapacity = Math.Max(Math.Max(_span.Length + 1, arrayMaxLength), _span.Length);
        }

        T[] array = ArrayPool<T>.Shared.Rent(nextCapacity);
        _span.CopyTo(array);

        T[]? toReturn = _arrayFromPool;
        _span = _arrayFromPool = array;
        if (toReturn == null) return;
        
        if (!typeof(T).IsPrimitive)
        {
            Array.Clear(toReturn, 0, _pos);
        }

        ArrayPool<T>.Shared.Return(toReturn);
    }
}
