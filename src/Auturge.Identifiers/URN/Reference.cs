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

    // Every comparison path (==, Equals(object), IEquatable<Reference<T>>) funnels
    // through Equals(object) -> EqualsCore so a subclass that widens the comparison
    // (e.g. ResourceLink adding Link) stays consistent across all of them.
    public bool Equals(Reference<T>? other) => Equals((object?)other);

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return EqualsCore((Reference<T>)obj);
    }

    protected virtual bool EqualsCore(Reference<T> other)
        => DisplayName == other.DisplayName
           && EqualityComparer<T>.Default.Equals(Resource, other.Resource);

    public override int GetHashCode() => HashCode.Combine(DisplayName, Resource);

    public static bool operator ==(Reference<T>? lhs, Reference<T>? rhs)
        => lhs is null ? rhs is null : lhs.Equals((object?)rhs);

    public static bool operator !=(Reference<T>? lhs, Reference<T>? rhs) => !(lhs == rhs);

    #endregion Equality
}
