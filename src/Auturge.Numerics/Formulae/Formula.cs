namespace Auturge.Numerics;

public abstract class Formula<TValue, T1>(params Func<TValue, T1, TValue>[] operations) : IFormula
{
    internal List<Func<TValue, T1, TValue>> Operations { get; } = [.. operations];

    public TValue Apply(TValue value, T1 arg)
    {
        TValue obj = value;
        foreach (Func<TValue, T1, TValue> operation in Operations)
            obj = (TValue)operation.Method.Invoke(value, [value, arg])!;
        return obj;
    }
}
