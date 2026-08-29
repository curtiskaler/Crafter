// ReSharper disable InconsistentNaming

namespace Auturge.Quantity;

/// <summary>
/// A bijection that changes the value of one form to another. 
/// </summary>
public class Conversion : Bijection
{
    /// <summary>
    /// An equation that remains true for all possible values of its variables.
    /// </summary>
    public static readonly Conversion Identity = new(x => x, x => x);

    /// <summary>
    /// Conversion that takes a converter, and an inversion function.
    /// </summary>
    /// <param name="converter"></param>
    /// <param name="inversion"></param>
    public Conversion(Func<object, object> converter, Func<object, object> inversion) : base(converter, inversion)
    {
    }
    
    public Conversion(List<Func<object, object>> func, List<Func<object, object>> inv) : base(func, inv)
    {
    }
    
    /// <summary>
    /// Instantiate a conversion that represents a chain of conversions.
    /// For example, ( m/s -> mi/hr ) from the chain [ m/s -> ft/s -> mi/s -> mi/hr ]
    /// </summary>
    /// <param name="chain">A list of conversions to be successively applied.</param>
    /// <exception cref="ArgumentException">the provided chain is an empty list</exception>
    internal Conversion(List<Conversion> chain)
    {
        if (chain.Count < 1) throw new ArgumentException("Cannot create a conversion chain from an empty list");
        var conv = chain[0];
        for (var index = 1; index < chain.Count; index++)
        {
            conv *= chain[index];
        }

        Functions.AddRange(conv.Functions);
        Inversions.AddRange(conv.Inversions);
    }

    /// <summary>
    /// Conversion that takes lists of chained functions.
    /// </summary>
    /// <param name="chainedFunctions"></param>
    /// <param name="chainedInverters"></param>
    private Conversion(List<List<Func<object, object>>> chainedFunctions,
        List<List<Func<object, object>>> chainedInverters)
        : base(
            [.. chainedFunctions.SelectMany(f => f)],
            [.. chainedInverters.SelectMany(i => i)]
        )
    {
    }

    public override object Execute(object term)
        => Functions.Aggregate(term, (current, operation) => operation(current));

    /// <summary>
    /// Return the <see cref="Conversion"/> that reverses this conversion.
    /// </summary>
    public override Conversion Invert()
    {
        ArgumentNullException.ThrowIfNull(Inversions);
        return new Conversion(Inversions, Functions);
    }

    #region Arithmetic Operators

    public static Conversion operator *(Conversion lhs, Conversion rhs) =>
        // this means: do conversion 1, then do conversion 2
        new([lhs.Functions, rhs.Functions],
            [rhs.Inversions, lhs.Inversions]);

    public static Conversion operator /(Conversion lhs, Conversion rhs) =>
        // this means: do conversion 1, then do conversion inversion 2
        // TODO: ...is that mathematically true?  Ensure that this operator is not nonsense.
        new([lhs.Functions, rhs.Inversions],
            [rhs.Functions, lhs.Inversions]);

    #endregion Arithmetic Operators
}

// internal Conversion(Converter converter) : base(converter.Convert, converter.Invert)
// {
// }
