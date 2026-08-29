using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Auturge.Identifiers;

[DebuggerDisplay("{DisplayName} : {Resource}")]
public class Reference<T>(string displayName, [NotNull] T resource) : IEquatable<Reference<T>> 
{
    public string DisplayName { get; } = displayName ?? throw new ArgumentNullException(nameof(displayName));
    public T Resource { get; } = resource ?? throw new ArgumentNullException(nameof(resource));

    public Reference(Reference<T> original) : this(original.DisplayName, original.Resource)
    {
    }

    #region Equality
    
    public bool Equals(Reference<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return DisplayName == other.DisplayName && Resource!.Equals(other.Resource);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Reference<T>)obj);
    }

    public override int GetHashCode() => HashCode.Combine(DisplayName, Resource);
    
    public static bool operator ==(Reference<T>? lhs, Reference<T>? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Reference<T>? lhs, Reference<T>? rhs) => !(lhs == rhs);
    
    #endregion Equality
}
