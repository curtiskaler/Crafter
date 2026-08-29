using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Auturge.Identifiers;

/// <summary>
/// Uniform Resource Name (URN)
/// <para/>
/// A URN is a type of Uniform Resource Identifier (URI) that names a resource by a unique, persistent label,
/// but it does not tell you where the resource is located or how to access it.
/// <para/>
/// Abstract so that you can (MUST!) define it within your own namespace. 
/// </summary>
[DebuggerDisplay("urn:{NID}:{NSS}")]
public abstract class URN : Uri
{
    private const string _urnScheme = "urn";
    protected const RegexOptions _urnRegexOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant;

    private static readonly Regex _urnRegex =
        new("^urn:(?<NID>[a-z|A-Z][a-z|A-Z|-]{0,30}[a-z|A-Z]):(?<NSS>.*)$", _urnRegexOptions);

    /// <summary>
    /// Namespace Identifier (NID)
    /// <para/>
    /// The Namespace Identifier (NID) names a specific category or domain of data.
    /// NIDs are case-insensitive, and must be unique across the entire "urn" scheme. 
    /// </summary>
    public string NID { get; }

    /// <summary>
    /// Namespace Specific String (NSS)
    /// <para/>
    /// The NSS is a string, unique within a URN namespace, that is assigned
    /// and managed in a consistent way and that conforms to the definition
    /// of the relevant URN namespace.  The combination of the NID (unique
    /// across the entire "urn" scheme) and the NSS (unique within the URN
    /// namespace) ensures that the resulting URN is globally unique.
    /// </summary>
    public string NSS { get; }

    public URN(string s) : base(s, UriKind.Absolute)
    {
        if (Scheme != _urnScheme) throw new FormatException($"URN scheme must be '{_urnScheme}'.");
        Match match = _urnRegex.Match(this.AbsoluteUri);
        if (!match.Success) throw new FormatException("URN's NID is invalid.");
        NID = match.Groups["NID"].Value;
        NSS = match.Groups["NSS"].Value;
    }

    public URN(string nid, string nss) : this($"urn:{nid}:{nss}")
    {
    }

    public override bool Equals(object? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(other, this)) return true;
        return
            other is URN u &&
            string.Equals(NID, u.NID, StringComparison.InvariantCultureIgnoreCase) &&
            string.Equals(NSS, u.NSS, StringComparison.Ordinal);
    }

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(URN u1, URN u2)
    {
        if (ReferenceEquals(u1, u2)) return true;
        if (ReferenceEquals(u1, null) || ReferenceEquals(u2, null)) return false;
        return u1.Equals(u2);
    }

    public static bool operator !=(URN u1, URN u2)
    {
        if (ReferenceEquals(u1, u2)) return false;
        if (ReferenceEquals(u1, null) || ReferenceEquals(u2, null)) return true;
        return !u1.Equals(u2);
    }
}
