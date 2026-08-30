using System.Diagnostics;

namespace Auturge.Identifiers;

[DebuggerDisplay("{DisplayName} : {Link}")]
public class ResourceLink<T>(Reference<T> reference, Uri link)
    : Reference<T>(reference), IEquatable<ResourceLink<T>>
{
    public Uri Link { get; } = link ?? throw new ArgumentNullException(nameof(link));

    public bool Equals(ResourceLink<T>? other) => Equals((object?)other);

    // Reference<T>.Equals(object) has already confirmed the runtime types match
    // before dispatching here; the pattern guard keeps it correct if called directly.
    protected override bool EqualsCore(Reference<T> other)
        => other is ResourceLink<T> link
           && base.EqualsCore(other)
           && Link.Equals(link.Link);

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Link);
}
