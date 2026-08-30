namespace Auturge.Numerics;

public abstract class Formula<TValue, T1>(params Func<TValue, T1, TValue>[] operations) : IFormula
{
    internal List<Func<TValue, T1, TValue>> Operations { get; } = [.. operations];

    public TValue Apply(TValue value, T1 arg)
    {
        TValue accumulator = value;
        foreach (Func<TValue, T1, TValue> operation in Operations)
            accumulator = operation(accumulator, arg);
        return accumulator;
    }
}
