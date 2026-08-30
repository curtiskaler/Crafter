using Auturge.Identifiers;

namespace Crafter.Model.Identifiers;

/// <summary>
/// A <see cref="URNReference"/> paired with a resolvable <see cref="Uri"/>. Equality also
/// considers the link, inherited from <see cref="ResourceLink{T}"/>.
/// </summary>
public class URNLink(URNReference reference, Uri link) : ResourceLink<URN>(reference, link)
{
    /// <summary>Creates a link from a full <c>urn:auturge-crafter:…</c> string.</summary>
    public URNLink(string displayName, string urnString, string link)
        : this(new URNReference(displayName, urnString), new Uri(link))
    {
    }

    /// <summary>Creates a link from a URN's <paramref name="entityType"/> and <paramref name="id"/>.</summary>
    public URNLink(string displayName, string entityType, string id, string link)
        : this(new URNReference(displayName, entityType, id), new Uri(link))
    {
    }
}
