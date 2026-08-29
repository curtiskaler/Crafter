// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Local

using System.Diagnostics;

#pragma warning disable CS0660, CS0661
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

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

        foreach (var kvp in this)
        {
            if (!other.TryGetValue(kvp.Key, out var value))
            {
                return false;
            }

            if (kvp.Value != value) return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((UnitDefinition)obj);
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
