// ReSharper disable MemberCanBePrivate.Global

using Auturge.Identifiers;
using Auturge.Numerics;

// using Auturge.Numerics;

namespace Auturge.Quantity;

public sealed class Unit : IEquatable<Unit>, IHaveNameAndSymbol, IHaveSynonyms<Unit>
{
    public static readonly Unit One = new(0, "1", "1", Dimensions.One);

    public long Id { get; }

    // TODO: change the conversion to use this
    // public Conversion ToBase { get; internal set; }
    
    /// <summary>
    /// The unit base.
    /// </summary>
    public Unit? Base { get; internal init; }
    
    /// <summary>
    /// The factor multiplied into the base to get this unit.
    /// </summary>
    public Number Factor { get; internal init; } = new(1.0);
    
    /// <summary>
    /// The factor the base unit should be divided by to get one of this unit.
    /// </summary>
    public Number Divisor { get; internal init; } = new(1.0);

    /// <summary>
    /// The definition of the unit, in terms of the exponents of its base units.
    /// </summary>
    public UnitDefinition Definition { get; }

    /// <summary>
    /// Name of the unit. For example,
    ///     "units.mgPerKg" (or "milligrams per kilogram", if hard-coded), or
    ///     "units.m3perMinute" (or "cubic meters per minute", if hard-coded).
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Symbol of the unit (for example, mg/kg, m³/min).
    /// </summary>
    public string Symbol { get; init; }

    /// <summary>
    /// The UnitType (for example, Length) that describes this unit.
    /// </summary>
    public Dimension Dimension { get; }

    public List<Synonym> Synonyms { get; } = [];

    public bool IsBase => Base == null;

    public override string ToString() => $@"{DisplayName} ({Symbol})";

    #region Constructors

    /// <summary>
    /// ctor for new Base units.
    /// </summary>
    public Unit(string displayName, string symbol, Dimension dimension)
        : this(Flake.NewFlake(), displayName, symbol, dimension)
    {
    }

    /// <summary>
    /// Called by prefix operator.
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="displayName"></param>
    /// <param name="symbol"></param>
    /// <param name="dimension"></param>
    /// <param name="definition"></param>
    public Unit(Unit unit, string? displayName = null, string? symbol = null, Dimension? dimension = null,
        UnitDefinition? definition = null)
        : this(unit.Id,
            displayName ?? unit.DisplayName,
            symbol ?? unit.Symbol,
            dimension ?? unit.Dimension,
            definition ?? unit.Definition)
    {
        // Don't want to assign Newtons as the base unit of Newtons:
        if (unit.Base != null && unit.Base.IsBase)
        {
            Base = unit.Base ?? unit;
            Factor = unit.Factor != 0 ? unit.Factor : 1;
            Divisor = unit.Divisor != 0 ? unit.Divisor : 1;
        }
    }


    /// <summary>
    /// General ctor.
    /// </summary>
    internal Unit(long? id, string displayName, string symbol, Dimension dimension, UnitDefinition? definition = null)
    {
        Id = id ?? Flake.NewFlake();
        DisplayName = displayName;
        Symbol = symbol;
        Dimension = dimension;
        Definition = definition ?? new UnitDefinition();
        if (definition == null)
        {
            Definition.Add(this, 1);
        }
    }

    /// <summary>
    /// ctor for intermediary units
    /// </summary>
    private Unit(Dimension dimension, UnitDefinition? definition)
        : this(Flake.NewFlake(), string.Empty, string.Empty, dimension, definition)
    {
    }

    #endregion Constructors

    #region IEquality

    public bool Equals(Unit? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Id == other.Id) return true;
        return Dimension == other.Dimension && Definition == other.Definition;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != GetType()) return false;
        return Equals((Unit)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, DisplayName, Symbol, Dimension, Definition);
    }

    public static bool operator ==(Unit? lhs, Unit? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Unit? lhs, Unit? rhs)
    {
        return !(lhs == rhs);
    }

    #endregion IEquality

    #region IHaveSynonyms

    public Unit AddSynonym(IHaveNameAndSymbol nameAndSymbol)
    {
        Synonyms.Add(new Synonym(nameAndSymbol));
        return this;
    }

    public Unit AddSynonym(string displayName, string symbol)
    {
        Synonyms.Add(new Synonym(displayName, symbol));
        return this;
    }

    public Unit AddSynonym(string symbol)
        => AddSynonym(new Synonym(DisplayName, symbol));

    #endregion IHaveSynonyms

    #region Arithmetic Operators

    public static Quantity operator *(Number factor, Unit baseUnit)
    {
        var found = Units.TryFind(
            x => x.Dimension == baseUnit.Dimension &&
                 (Math.Abs(x.Factor - factor) < .000001 || Math.Abs((double)x.Divisor - (1 / factor)) < .000001), out var unit);
        if (!found || unit == null)
        {
            unit = baseUnit;
        }

        return new Quantity(factor, unit);
    }

    public static Unit operator *(SIPrefix prefix, Unit baseUnit)
    {
        var displayName = prefix.DisplayName + baseUnit.DisplayName;
        var symbol = prefix.Symbol + baseUnit.Symbol;
        Number factor = prefix.Factor * baseUnit.Factor;
        Number divisor = prefix.Divisor * baseUnit.Divisor;

        // Do we already have such a beast in the cache?
        var found = Units.TryFind(
            x => x.Dimension == baseUnit.Dimension && x.DisplayName == displayName && x.Symbol == symbol, out var unit);
        if (found && unit != null)
        {
            return unit;
        }

        // It's not in the cache. Not sure why... Build it and return it.
        return new Unit(Flake.NewFlake(), displayName, symbol, baseUnit.Dimension)
        {
            Base = baseUnit,
            Factor = factor,
            Divisor = divisor
        };
    }

    /// <summary>
    /// Generate a unit that is the product of two units.
    /// <para />
    /// Used for, e.g., mm^2 or N. 
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    public static Unit operator *(Unit? lhs, Unit? rhs)
    {
        ArgumentNullException.ThrowIfNull(lhs);
        ArgumentNullException.ThrowIfNull(rhs);

        // Do we need to do this up-front?
        var leftDef = lhs.Definition.IncludeBaseUnits(lhs);
        var rightDef = rhs.Definition.IncludeBaseUnits(rhs);
        var definition = leftDef * rightDef;

        // can we find a dimension for this?  Pry not for something like kg * m / s
        var dim = lhs.Dimension * rhs.Dimension;
        var dimension = Dimensions.FindOrAdd(dim);

        // is there such a unit?
        var existing = Units.TryFind(dimension, definition, out var unit);
        if (existing && unit != null)
        {
            return unit;
        }

        return new Unit(dimension, definition);
    }

    /// <summary>
    /// Generate a unit that is the quotient of two units.
    /// <para />
    /// Used for, e.g., m/s or N. 
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    public static Unit operator /(Unit lhs, Unit rhs)
    {
        ArgumentNullException.ThrowIfNull(lhs);
        ArgumentNullException.ThrowIfNull(rhs);

        // Do we need to do this up-front?
        var leftDef = lhs.Definition.IncludeBaseUnits(lhs);
        var rightDef = rhs.Definition.IncludeBaseUnits(rhs);
        var definition = leftDef / rightDef;

        var dim = lhs.Dimension / rhs.Dimension;
        var dimension = Dimensions.FindOrAdd(dim);

        // is there such a unit?
        var existing = Units.TryFind(dimension, definition, out var unit);
        if (existing && unit != null)
        {
            return unit;
        }

        return new Unit(dimension, definition);
    }

    /// <summary>
    /// Generate a unit that is the reciprocal of another Unit.
    /// <para />
    /// Used for, e.g., 1/s or Hz. 
    /// </summary>
    /// <param name="one"></param>
    /// <param name="unit"></param>
    public static Unit operator /(double one, Unit unit)
    {
        one.ExpectOne();
        return Reciprocal(unit);
    }

    /// <summary>
    /// Generate the reciprocal of this Unit.
    /// <para />
    /// Used, e.g., to convert seconds to Hz. 
    /// </summary>
    public Unit Reciprocal() => Reciprocal(this);

    /// <summary>
    /// Generate the reciprocal of the specified Unit.
    /// <para />
    /// Used, e.g., to convert seconds to Hz. 
    /// </summary>
    public static Unit Reciprocal(Unit unit)
    {
        var definition = unit.Definition.Reciprocal();
        var dimension = Dimensions.Find(unit.Dimension.Reciprocal());

        // is there such a unit?
        var existing = Units.TryFind(dimension, definition, out var res);
        if (existing && res != null)
        {
            return res;
        }

        return new Unit(dimension, definition) { Base = unit.Base };
    }

    #endregion Arithmetic Operators
}
