// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Local

using System.Diagnostics;

namespace Auturge.Quantity;

/// <summary>
/// A dictionary of &lt;base Unit, Exponent&gt;.
/// <para/>
/// Useful for defining units like m^2, or Newtons (kg m / s^2).
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class UnitDefinition : Dictionary<Unit, short>, IEquatable<IDictionary<Unit, short>>
{
    public UnitDefinition(IDictionary<Unit, short>? definition = null)
    {
        if (definition is null) return;
        foreach (var kvp in definition)
        {
            Add(kvp.Key, kvp.Value);
        }
    }

    public UnitDefinition IncludeBaseUnits(params Unit[] units)
    {
        foreach (var unit in units)
        {
            IncludeBaseUnit(this, unit);
        }

        return this;
    }

    public override string ToString()
    {
        var list = this
            .Select(kvp => kvp.Key.Symbol + (kvp.Value != 1 ? "^" + kvp.Value : ""))
            .ToList();
        return string.Join(' ', list);
    }

    #region IEquatable<UnitDefinition>

    public bool Equals(UnitDefinition other)
        => Equals((Dictionary<Unit, short>)other);

    public bool Equals(IDictionary<Unit, short>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        if (Count != other.Count) return false;

        // Match keys by base-unit Id, not by Unit equality: Unit.Equals defers back here for its own
        // definition check and a base unit's definition contains itself, so a structural key match
        // would recurse with no base case.
        Dictionary<long, short> otherById = other.ToDictionary(static kvp => kvp.Key.Id, static kvp => kvp.Value);
        foreach (KeyValuePair<Unit, short> kvp in this)
        {
            if (!otherById.TryGetValue(kvp.Key.Id, out short value) || value != kvp.Value)
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((UnitDefinition)obj);
    }

    // Order-independent over the (base-unit Id, exponent) entries — the same pairing Equals compares.
    public override int GetHashCode()
    {
        int hash = Count;
        foreach (KeyValuePair<Unit, short> kvp in this)
        {
            hash ^= HashCode.Combine(kvp.Key.Id, kvp.Value);
        }

        return hash;
    }

    public static bool operator ==(UnitDefinition? lhs, UnitDefinition? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(UnitDefinition? lhs, UnitDefinition? rhs)
    {
        return !(lhs == rhs);
    }

    #endregion IEquatable<UnitDefinition>

    #region Arithmetic Operators

    public static UnitDefinition operator *(UnitDefinition lhs, UnitDefinition rhs)
    {
        var definition = new UnitDefinition(lhs);

        foreach (var (key, value) in rhs)
        {
            if (!definition.TryAdd(key, value))
            {
                definition[key] += value;
            }
        }

        return definition;
    }

    public static UnitDefinition operator /(UnitDefinition lhs, UnitDefinition rhs)
    {
        var definition = new UnitDefinition(lhs);

        foreach (var (key, value) in rhs)
        {
            if (!definition.TryAdd(key, (short)-value))
            {
                definition[key] -= value;
            }
        }

        return definition;
    }

    public UnitDefinition Reciprocal() => Reciprocal(this);

    public static UnitDefinition Reciprocal(UnitDefinition definition)
    {
        var result = new UnitDefinition();
        foreach (var (key, value) in definition)
        {
            result.Add(key, (short)-value);
        }

        return result;
    }

    #endregion Arithmetic Operators

    /// <summary>
    /// Given a definition, if a unit is a base unit, include it in the definition; otherwise, return the original definition.
    /// <para/>
    /// This is because a "base unit" has an empty definition, so copying it won't add the unit itself.
    /// </summary>
    private static UnitDefinition IncludeBaseUnit(UnitDefinition definition, Unit unit)
    {
        if (unit.Definition.Count != 0) return definition;

        if (!definition.TryAdd(unit, 1))
        {
            definition[unit] += 1;
        }

        return definition;
    }
}
