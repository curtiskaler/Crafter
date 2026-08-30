using Auturge.Identifiers;

namespace Crafter.Model.Identifiers;

/// <summary>
/// A named reference to a Crafter <see cref="URN"/>. Equality is by display name and URN,
/// inherited from <see cref="Reference{T}"/>.
/// </summary>
public class URNReference : Reference<URN>
{
    /// <summary>The referenced URN.</summary>
    public URN URN => Resource;

    /// <summary>Creates a reference from a URN's <paramref name="entityType"/> and <paramref name="id"/>.</summary>
    public URNReference(string displayName, string entityType, string id)
        : base(displayName, new URN(entityType, id))
    {
    }

    /// <summary>Creates a reference from a full <c>urn:auturge-crafter:…</c> string.</summary>
    public URNReference(string displayName, string urnString)
        : this(displayName, new URN(urnString))
    {
    }

    /// <summary>Creates a reference to an existing <paramref name="urn"/>.</summary>
    public URNReference(string displayName, URN urn) : base(displayName, urn)
    {
    }

    /// <summary>Copy constructor.</summary>
    public URNReference(URNReference reference) : base(reference)
    {
    }
}
