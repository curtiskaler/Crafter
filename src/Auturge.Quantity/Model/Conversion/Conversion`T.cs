namespace Auturge.Quantity;

/// <summary>
/// A bijection that changes the value of one form to another. 
/// </summary>
public class Conversion<T> : Conversion, IBijection<T>
{
    /// <summary>
    /// An equation that remains true for all possible values of its variables.
    /// </summary>
    public new static readonly Conversion<T> Identity = new(x => x, x => x);

    internal Conversion(Func<T, T> converter, Func<T, T> inverter) : base(converter.Box(), inverter.Box())
    {
    }

    internal Conversion(List<Conversion> chain) : base(chain)
    {
    }

    internal Conversion(Conversion conv) : base(conv.Functions, conv.Inversions)
    {
    }
    
    internal Conversion(List<Conversion<T>> chain) : base(chain.Box())
    {
    }

    internal Conversion(List<Func<object, object>> converters, List<Func<object, object>> inverters)
        : base(converters, inverters)
    {
    }

    public T Execute(T term) => Functions.Aggregate(term, (current, operation) => (T)operation((T)current!)!);

    IBijection<T> IBijection<T>.Invert()
    {
        ArgumentNullException.ThrowIfNull(Inversions);
        return new Conversion<T>(Inversions, Functions);
    }
}

public static class FunctionExtensions
{
    public static Func<object, object> Box<T>(this Func<T, T> func) => (o) => func((T)o)!;
    public static List<Func<object, object>> Box<T>(this List<Func<T, T>> list) => [.. list.Select(Box)];

    public static Func<T, T> Unbox<T>(this Func<object, object> func) => o => (T)func(o!);
    public static List<Func<T, T>> Unbox<T>(this List<Func<object, object>> list) => [.. list.Select(Unbox<T>)];

    public static List<Conversion> Box<T>(this List<Conversion<T>> list) => new(list);

    public static Conversion<T> Unbox<T>(this Conversion conv) 
        => new(conv.Functions, conv.Inversions);
}


//
// public class Conversion : Conversion<object>
// {
//     internal Conversion(Func<object, object> converter, Func<object, object> inversion) : base(converter, inversion)
//     {
//     }
//
//     internal Conversion(List<Conversion<object>> chain) : base(chain)
//     {
//     }
// }


// public class Conversion<T> : Bijection<Func<T, T>, T> //where T : IEquatable<T>
// {
//     /// <summary>
//     /// An equation that remains true for all possible values of its variables.
//     /// </summary>
//     public static readonly Conversion<T> Identity = new(x => x, x => x);
//
//     /// <summary>
//     /// Conversion that takes a converter, and an inversion function.
//     /// </summary>
//     /// <param name="converter"></param>
//     /// <param name="inversion"></param>
//     internal Conversion(Func<T, T> converter, Func<T, T> inversion) : base(converter, inversion)
//     {
//     }
//
//     /// <summary>
//     /// Instantiate a conversion that represents a chain of conversions.
//     /// For example, ( m/s -> mi/hr ) from the chain [ m/s -> ft/s -> mi/s -> mi/hr ]
//     /// </summary>
//     /// <param name="chain">A list of conversions to be successively applied.</param>
//     /// <exception cref="ArgumentException">the provided chain is an empty list</exception>
//     internal Conversion(List<Conversion<T>> chain)
//     {
//         if (chain.Count < 1) throw new ArgumentException("Cannot create a conversion chain from an empty list");
//         var conv = chain[0];
//         for (var index = 1; index < chain.Count; index++)
//         {
//             conv *= chain[index];
//         }
//
//         Functions.AddRange(conv.Functions);
//         Inversions.AddRange(conv.Inversions);
//     }
//
//     private Conversion(List<Func<T, T>> func, List<Func<T, T>> inv) : base(func, inv)
//     {
//     }
//
//     /// <summary>
//     /// Conversion that takes lists of chained functions.
//     /// </summary>
//     /// <param name="chainedFunctions"></param>
//     /// <param name="chainedInverters"></param>
//     private Conversion(List<List<Func<T, T>>> chainedFunctions,
//         List<List<Func<T, T>>> chainedInverters)
//         : base(
//             chainedFunctions.SelectMany(f => f).ToList(),
//             chainedInverters.SelectMany(i => i).ToList()
//         )
//     {
//     }
//
//     public T Execute(T term)
//         => Functions.Aggregate(term, (current, operation) => operation(current));
//
//     /// <summary>
//     /// Return the <see cref="Conversion&lt;T&gt;"/> that reverses this conversion.
//     /// </summary>
//     public override Conversion<T> Invert()
//     {
//         ArgumentNullException.ThrowIfNull(Inversions);
//         return new Conversion<T>(Inversions, Functions);
//     }
//
//     #region Arithmetic Operators
//
//     public static Conversion<T> operator *(Conversion<T> lhs, Conversion<T> rhs) =>
//         // this means: do conversion 1, then do conversion 2
//         new([lhs.Functions, rhs.Functions],
//             [rhs.Inversions, lhs.Inversions]);
//
//     public static Conversion<T> operator /(Conversion<T> lhs, Conversion<T> rhs) =>
//         // this means: do conversion 1, then do conversion inversion 2
//         // TODO: ...is that mathematically true?  Ensure that this operator is not nonsense.
//         new([lhs.Functions, rhs.Inversions],
//             [rhs.Functions, lhs.Inversions]);
//
//     #endregion Arithmetic Operators
// }
