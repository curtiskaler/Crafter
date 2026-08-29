namespace Auturge.Quantity;

public abstract class Operation<TDelegate> : Operation
{
    protected new readonly List<TDelegate> Functions = [];
}

public interface IBijection<T>
{
    public T Execute(T term);
    IBijection<T> Invert();
}

/// <summary>
/// A bijection is a perfect one-to-one pairing between elements of two sets,
/// meaning every element in the first set maps to exactly one unique element
/// in the second set, and vice versa, making the function both
/// injective (one-to-one) and surjective (onto).  This makes it invertible.
/// </summary>
/// <typeparam name="TDelegate"></typeparam>
/// <typeparam name="T"></typeparam>
public abstract class Bijection<TDelegate, T> : Operation<TDelegate>, IBijection<T>
{
    protected Bijection(TDelegate f, TDelegate i) : this([f], [i])
    {
    }

    protected Bijection()
    {
    }

    protected Bijection(List<TDelegate> f, List<TDelegate> i)
    {
        if (f.Count == 0)
        {
            throw new ArgumentException("f must have at least one function");
        }

        if (i.Count == 0)
        {
            throw new ArgumentException("i must have at least one function");
        }

        Functions.AddRange(f);
        Inversions.AddRange(i);
    }

    /// <summary>
    /// The list of functions required to invert the executed operation.
    /// </summary>
    protected readonly List<TDelegate> Inversions = [];

    public abstract T Execute(T term);

    /// <summary>
    /// Invert the operation.
    /// </summary>
    public abstract IBijection<T> Invert();
}
