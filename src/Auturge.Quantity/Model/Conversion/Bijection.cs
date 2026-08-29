namespace Auturge.Quantity;

public interface IBijection
{
    public object Execute(object term);
    IBijection Invert();
}

/// <summary>
/// A bijection is a perfect one-to-one pairing between elements of two sets,
/// meaning every element in the first set maps to exactly one unique element
/// in the second set, and vice versa, making the function both
/// injective ('one-to-one') and surjective ('onto').  This makes it invertible.
/// In practice, this means that the inputs and outputs cannot be null.
/// </summary>
public abstract class Bijection : Operation, IBijection
{
    /// <summary>
    /// The list of functions required to invert the executed operation.
    /// </summary>
    protected internal readonly List<Func<object, object>> Inversions = [];

    protected Bijection()
    {
    }

    protected Bijection(Func<object, object> f, Func<object, object> i) : this([f], [i])
    {
    }

    protected Bijection(List<Func<object, object>> f, List<Func<object, object>> i)
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
    /// Execute the function.
    /// </summary>
    /// <param name="term">The input term.</param>
    /// <returns>The result of the executed function.</returns>
    public abstract object Execute(object term);

    /// <summary>
    /// Invert the operation.
    /// </summary>
    public abstract IBijection Invert();
}
