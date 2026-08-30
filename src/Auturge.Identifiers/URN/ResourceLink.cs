using System.Diagnostics;

namespace Auturge.Identifiers;

/// <summary>
/// A <see cref="Reference{T}"/> paired with a resolvable <see cref="Uri"/>. Equality also
/// considers <see cref="Link"/>.
/// </summary>
/// <typeparam name="T">The resource type.</typeparam>
/// <param name="reference">The underlying reference; its display name and resource are copied.</param>
/// <param name="link">Where the resource can be retrieved.</param>
[DebuggerDisplay("{DisplayName} : {Link}")]
public class ResourceLink<T>(Reference<T> reference, Uri link)
    : Reference<T>(reference), IEquatable<ResourceLink<T>>
    where T : notnull
{
    /// <summary>Where the resource can be retrieved.</summary>
    public Uri Link { get; } = link ?? throw new ArgumentNullException(nameof(link));

    /// <inheritdoc/>
    public bool Equals(ResourceLink<T>? other) => Equals((object?)other);

    // Reference<T>.Equals(object) has already confirmed the runtime types match before
    // dispatching here; the pattern guard keeps it correct if called directly.
    /// <inheritdoc/>
    protected override bool EqualsCore(Reference<T> other)
        => other is ResourceLink<T> link
           && base.EqualsCore(other)
           && Link.Equals(link.Link);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Link);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ResourceLink<T>);
}
