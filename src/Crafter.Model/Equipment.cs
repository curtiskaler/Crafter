using System.ComponentModel.DataAnnotations;
using Auturge.Identifiers;
using Auturge.Stores;

namespace Crafter.Model;

public class Equipment(long? id, string? displayName) : StoredEntity(id), IDisplayEntity, IEquatable<Equipment>
{
    public Equipment() : this(null, null)
    {
    }

    public Equipment(string displayName) : this(Flake.NewFlake(), displayName)
    {
    }

    public string DisplayName { get; } = displayName ?? string.Empty;
    
    public ValidationResult? ValidateBeforeSave(IState? state) => ValidationResult.Success;

    public static readonly Equipment None = new(-1, string.Empty);

    #region Equality

    public bool Equals(Equipment? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return DisplayName == other.DisplayName && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Equipment)obj);
    }

    public override int GetHashCode() => HashCode.Combine(DisplayName, Id);

    public static bool operator ==(Equipment? lhs, Equipment? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Equipment? lhs, Equipment? rhs) => !(lhs == rhs);

    #endregion Equality
}
