using System.Diagnostics;

namespace Auturge.Identifiers;

[DebuggerDisplay("{DisplayName} : {Link}")]
public class ResourceLink<T>(Reference<T> reference, Uri link)
    : Reference<T>(reference), IEquatable<ResourceLink<T>>
{
    public Uri Link { get; } = link;

    public bool Equals(ResourceLink<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Link.Equals(other.Link);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ResourceLink<T>)obj);
    }

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Link);
}
