using System.Diagnostics;

namespace Auturge.Identifiers;

/// <summary>
/// A display name paired with the resource it identifies. Equality is by
/// <see cref="DisplayName"/> and <see cref="Resource"/>.
/// </summary>
/// <typeparam name="T">The resource type; must be a non-nullable type.</typeparam>
/// <param name="displayName">Human-readable label for the resource.</param>
/// <param name="resource">The resource being referenced.</param>
[DebuggerDisplay("{DisplayName} : {Resource}")]
public class Reference<T>(string displayName, T resource) : IEquatable<Reference<T>>
    where T : notnull
{
    /// <summary>Human-readable label for the resource.</summary>
    public string DisplayName { get; } = displayName ?? throw new ArgumentNullException(nameof(displayName));

    /// <summary>The referenced resource.</summary>
    public T Resource { get; } = resource is null ? throw new ArgumentNullException(nameof(resource)) : resource;

    /// <summary>
    /// Reprojects <paramref name="original"/> as a plain <see cref="Reference{T}"/>: its
    /// <see cref="DisplayName"/> and <see cref="Resource"/> are copied, but subclass state
    /// (such as a <see cref="ResourceLink{T}"/>'s link) is not.
    /// </summary>
    public Reference(Reference<T> original) : this(original.DisplayName, original.Resource)
    {
    }

    #region Equality

    // Every comparison path (==, Equals(object), IEquatable<Reference<T>>) funnels through
    // Equals(object) -> EqualsCore so a subclass that widens the comparison (e.g. ResourceLink
    // adding Link) stays consistent across all of them.
    /// <inheritdoc/>
    public bool Equals(Reference<T>? other) => Equals((object?)other);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return EqualsCore((Reference<T>)obj);
    }

    /// <summary>
    /// The type-specific equality check, invoked only after the runtime types match. Override
    /// to fold additional state (as <see cref="ResourceLink{T}"/> does with its link) into
    /// equality; call <c>base.EqualsCore</c> for the display name and resource.
    /// </summary>
    protected virtual bool EqualsCore(Reference<T> other)
        => DisplayName == other.DisplayName
           && EqualityComparer<T>.Default.Equals(Resource, other.Resource);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(DisplayName, Resource);

    /// <summary>Compares two references by <see cref="DisplayName"/> and <see cref="Resource"/>.</summary>
    public static bool operator ==(Reference<T>? lhs, Reference<T>? rhs)
        => lhs is null ? rhs is null : lhs.Equals((object?)rhs);

    /// <summary>Compares two references by <see cref="DisplayName"/> and <see cref="Resource"/>.</summary>
    public static bool operator !=(Reference<T>? lhs, Reference<T>? rhs) => !(lhs == rhs);

    #endregion Equality
}
