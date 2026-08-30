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
public abstract class URN : Uri, IEquatable<URN>
{
    private const string _urnScheme = "urn";
    protected const RegexOptions _urnRegexOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant;

    // RFC 8141: NID is alphanumeric with internal hyphens (2-32 chars); NSS is non-empty.
    private static readonly Regex _urnRegex =
        new("^urn:(?<NID>[a-zA-Z0-9][a-zA-Z0-9-]{0,30}[a-zA-Z0-9]):(?<NSS>.+)$", _urnRegexOptions);

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

    // Uri implements IEquatable<Uri>, so a URN-typed Equals(other) call would otherwise
    // bind to Uri's case-sensitive comparison instead of this one. Route every path
    // (Equals(object), Equals(URN), ==) through the same NID/NSS check.
    public bool Equals(URN? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(other, this)) return true;
        return
            string.Equals(NID, other.NID, StringComparison.InvariantCultureIgnoreCase) &&
            string.Equals(NSS, other.NSS, StringComparison.Ordinal);
    }

    public override bool Equals(object? other) => Equals(other as URN);

    // Must mirror Equals: NID compared case-insensitively, NSS ordinally.
    public override int GetHashCode() => HashCode.Combine(
        StringComparer.InvariantCultureIgnoreCase.GetHashCode(NID),
        StringComparer.Ordinal.GetHashCode(NSS));

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
