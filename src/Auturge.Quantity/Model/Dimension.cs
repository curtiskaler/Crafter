// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InconsistentNaming

using Auturge.Identifiers;

namespace Auturge.Quantity;

/// <summary>
/// A dimension is a mathematical expression identifying the
/// powers of the base quantities involved (such as length, mass, time, etc.).
/// </summary>
public class Dimension : DimensionVector,
    IHaveNameAndSymbol, IHaveSynonyms<Dimension>, IEquatable<Dimension>
{
    public long Id { get; }

    /// <summary>
    /// The name (or i18n key) to be resolved and displayed in the UI.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The symbol or abbreviation for the given entity.
    /// </summary>
    public string Symbol { get; }

    public List<Synonym> Synonyms { get; } = [];

    public override string ToString() => $@"{DisplayName} ({Symbol})";

    /// <summary>
    /// ctor for a new base dimension.
    /// </summary>
    /// <param name="displayName">The display name (or i18n key) of the dimension.</param>
    /// <param name="symbol">The symbol for the dimension.</param>
    /// <param name="T">The exponent for the TIME dimension.</param>
    /// <param name="L">The exponent for the LENGTH dimension.</param>
    /// <param name="M">The exponent for the MASS dimension.</param>
    /// <param name="I">The exponent for the ELECTRIC CURRENT dimension.</param>
    /// <param name="Θ">The exponent for the ABSOLUTE TEMPERATURE dimension.</param>
    /// <param name="N">The exponent for the AMOUNT OF SUBSTANCE dimension.</param>
    /// <param name="J">The exponent for the LUMINOUS INTENSITY dimension.</param>
    public Dimension(string displayName, string symbol, short T, short L, short M, short I, short Θ, short N,
        short J) : this(Flake.NewFlake(), displayName, symbol, T, L, M, I, Θ, N, J)
    {
    }

    /// <summary>
    /// ctor for a base, derived, or serialized dimension.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="displayName">The display name (or i18n key) of the dimension.</param>
    /// <param name="symbol">The symbol for the dimension.</param>
    /// <param name="T">The exponent for the TIME dimension.</param>
    /// <param name="L">The exponent for the LENGTH dimension.</param>
    /// <param name="M">The exponent for the MASS dimension.</param>
    /// <param name="I">The exponent for the ELECTRIC CURRENT dimension.</param>
    /// <param name="Θ">The exponent for the ABSOLUTE TEMPERATURE dimension.</param>
    /// <param name="N">The exponent for the AMOUNT OF SUBSTANCE dimension.</param>
    /// <param name="J">The exponent for the LUMINOUS INTENSITY dimension.</param>
    public Dimension(long? id, string displayName, string symbol, short T, short L, short M, short I, short Θ, short N,
        short J) : base(T, L, M, I, Θ, N, J)
    {
        Id = id ?? Flake.NewFlake();
        DisplayName = displayName;
        Symbol = symbol;
    }

    /// <summary>
    /// ctor for a derived dimension.
    /// </summary>
    /// <param name="displayName">The display name (or i18n key) of the dimension.</param>
    /// <param name="symbol">The symbol for the dimension.</param>
    /// <param name="details">A DimensionVector containing the properties of this dimension.</param>
    public Dimension(string displayName, string symbol, DimensionVector details)
        : this(null, displayName, symbol, details)
    {
    }

    /// <summary>
    /// ctor for a derived dimension.
    /// </summary>
    /// <param name="id">The ID of the dimension, if deserializing.</param>
    /// <param name="displayName">The display name (or i18n key) of the dimension.</param>
    /// <param name="symbol">The symbol for the dimension.</param>
    /// <param name="vector">A DimensionVector containing the properties of this dimension.</param>
    public Dimension(long? id, string displayName, string symbol, DimensionVector vector) : base(vector)
    {
        Id = id ?? Flake.NewFlake();
        DisplayName = displayName;
        Symbol = symbol;
    }

    public Dimension AddSynonym(IHaveNameAndSymbol nameAndSymbol)
    {
        Synonyms.Add(new Synonym(nameAndSymbol));
        return this;
    }

    public Dimension AddSynonym(string synonym, string symbol)
        => AddSynonym(new Synonym(synonym, symbol));

    #region Arithmetic Operators

    public static Dimension operator *(Dimension a, Dimension b)
    {
        // ReSharper disable RedundantCast
        var vector = ((DimensionVector)a) * ((DimensionVector)b);

        //does such a beast exist?
        var found = Dimensions.TryFind(vector, out var dimension);
        if (found && dimension != null)
        {
            return dimension;
        }

        //one doesn't exist, so let's wrap the vector up in a new Dimension
        var dim = new Dimension(vector.Analysis, vector.Analysis, vector);
        return dim;
    }

    public static Dimension operator /(Dimension a, Dimension b)
    {
        // ReSharper disable RedundantCast
        var vector = ((DimensionVector)a) / ((DimensionVector)b);

        // does such a beast exist?
        var found = Dimensions.TryFind(vector, out var dimension);
        if (found && dimension != null)
        {
            return dimension;
        }

        // one doesn't exist, so let's wrap the vector up in a new Dimension
        var dim = new Dimension(vector.Analysis, vector.Analysis, vector);
        return dim;
    }

    public static Dimension FindOrAdd(DimensionVector vector)
    {
        var found = Dimensions.TryFind(vector, out var dimension);
        if (found && dimension != null)
        {
            return dimension;
        }

        // one doesn't exist, so let's wrap the vector up in a new Dimension
        var dim = new Dimension(vector.Analysis, vector.Analysis, vector);
        Dimensions.Add(dim);
        return dim;
    }

    public new Dimension Reciprocal() => Reciprocal(this);

    public static Dimension Reciprocal(Dimension dim)
    {
        // Reciprocate the underlying exponent vector — NOT dim.Reciprocal(), which is this very
        // method and would recurse forever.
        DimensionVector reciprocalVector = ((DimensionVector)dim).Reciprocal();
        return new Dimension(null, "1/" + dim.DisplayName, "1/" + dim.Symbol, reciprocalVector);
    }

    #endregion Arithmetic Operators

    #region IEquatable<Dimension>

    public bool Equals(Dimension? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id || base.Equals(other);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Dimension)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Id, DisplayName, Symbol, Synonyms);
    }

    public static bool operator ==(Dimension? left, Dimension? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Dimension? left, Dimension? right)
    {
        return !(left == right);
    }

    #endregion IEquatable<Dimension>
}
