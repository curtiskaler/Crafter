using System.Text.RegularExpressions;

namespace Crafter.Model.Identifiers;

public class URN : Auturge.Identifiers.URN
{
    private const string _namespaceId = "auturge-crafter";

    private static readonly Regex _nssRegex =
        new("^(?<EntityType>.*):(?<Id>.*)$", _urnRegexOptions);

    private static string ToNSS(string entityType, string id) => $"{entityType}:{id}";

    private static string GetPart(URN urn, string matcher)
    {
        Match match = _nssRegex.Match(urn.NSS);
        if (!match.Success) throw new FormatException("URN's NSS is invalid.");
        return match.Groups[matcher].Value;
    }

    public string EntityType
    {
        get => GetPart(this, nameof(EntityType));
    }

    public string Id
    {
        get => GetPart(this, nameof(Id));
    }

    public URN(string s) : base(s)
    {
        if (!string.Equals(NID, _namespaceId, StringComparison.InvariantCultureIgnoreCase))
            throw new FormatException($"NID (Namespace ID) must be '{_namespaceId}'.");
    }

    public URN(string entityType, string id) : base(_namespaceId, ToNSS(entityType, id))
    {
    }

    public URN(URN urn) : this(urn.NID.ToLowerInvariant(), urn.NSS)
    {
    }
}
