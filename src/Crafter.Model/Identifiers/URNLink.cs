using Auturge.Identifiers;

namespace Crafter.Model.Identifiers;

public class URNLink(URNReference reference, Uri link) : ResourceLink<URN>(reference, link)
{
    public URNLink(string displayName, string urnString, string link) : this(new URNReference(displayName, urnString),
        new Uri(link))
    {
    }

    public URNLink(string displayName, string entityType, string id, string link) : this(
        new URNReference(displayName, entityType, id), new Uri(link))
    {
    }
}
