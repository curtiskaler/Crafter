using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
public abstract partial class URN : Uri, IEquatable<URN>
{
    /// <summary>Regex options shared with subclasses that parse their own NSS.</summary>
    protected const RegexOptions _urnRegexOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant;

    // RFC 8141: scheme is ASCII case-insensitive; NID is 2-32 alphanumerics with internal
    // hyphens; NSS is one or more path characters (unreserved / sub-delims / ":" / "@" / "/")
    // or percent-encoded octets.
    [GeneratedRegex(
        @"^[Uu][Rr][Nn]:(?<NID>[A-Za-z0-9][A-Za-z0-9-]{0,30}[A-Za-z0-9]):(?<NSS>(?:[-A-Za-z0-9._~!$&'()*+,;=:@/]|%[0-9A-Fa-f]{2})+)$",
        _urnRegexOptions)]
    private static partial Regex UrnRegex();

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9-]{0,30}[A-Za-z0-9]$", _urnRegexOptions)]
    private static partial Regex NidRegex();

    [GeneratedRegex(@"^(?:[-A-Za-z0-9._~!$&'()*+,;=:@/]|%[0-9A-Fa-f]{2})+$", _urnRegexOptions)]
    private static partial Regex NssRegex();

    /// <summary>
    /// Namespace Identifier (NID)
    /// <para/>
    /// Names a category or domain of data. Case-insensitive; unique across the entire "urn" scheme.
    /// </summary>
    public string NID { get; }

    /// <summary>
    /// Namespace Specific String (NSS)
    /// <para/>
    /// The label within the namespace. Combined with the <see cref="NID"/> it identifies the
    /// resource uniquely. Compared case-sensitively.
    /// </summary>
    public string NSS { get; }

    /// <summary>
    /// Parses a full <c>urn:&lt;nid&gt;:&lt;nss&gt;</c> string.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="s"/> is not a syntactically valid URN.</exception>
    public URN(string s) : base(Validated(s), UriKind.Absolute)
    {
        Match match = UrnRegex().Match(s);
        NID = match.Groups["NID"].Value;
        NSS = match.Groups["NSS"].Value;
    }

    /// <summary>
    /// Composes a URN from its <paramref name="nid"/> and <paramref name="nss"/> parts.
    /// </summary>
    /// <exception cref="ArgumentNullException">A part is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="nid"/> or <paramref name="nss"/> is not valid.</exception>
    public URN(string nid, string nss) : this(Compose(nid, nss))
    {
    }

    private static string Validated(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (!UrnRegex().IsMatch(s))
        {
            throw new FormatException(
                $"'{s}' is not a valid URN. Expected 'urn:<nid>:<nss>' where <nid> is 2-32 "
                + "alphanumerics with internal hyphens and <nss> is one or more unreserved, "
                + "sub-delimiter, or percent-encoded characters (RFC 8141).");
        }

        return s;
    }

    private static string Compose(string nid, string nss)
    {
        ArgumentNullException.ThrowIfNull(nid);
        ArgumentNullException.ThrowIfNull(nss);
        if (!NidRegex().IsMatch(nid))
        {
            throw new FormatException(
                $"'{nid}' is not a valid URN NID: 2-32 characters, alphanumeric with internal hyphens (RFC 8141).");
        }

        if (!NssRegex().IsMatch(nss))
        {
            throw new FormatException(
                $"'{nss}' is not a valid URN NSS: one or more unreserved, sub-delimiter, or "
                + "percent-encoded characters (RFC 8141).");
        }

        return $"urn:{nid}:{nss}";
    }

    /// <summary>
    /// Splits <paramref name="s"/> into its NID and NSS without throwing. Intended for
    /// subclasses implementing their own <c>TryParse</c>.
    /// </summary>
    protected static bool TryParseParts(string? s, [NotNullWhen(true)] out string? nid, [NotNullWhen(true)] out string? nss)
    {
        nid = null;
        nss = null;
        if (s is null) return false;

        Match match = UrnRegex().Match(s);
        if (!match.Success) return false;

        nid = match.Groups["NID"].Value;
        nss = match.Groups["NSS"].Value;
        return true;
    }

    // Uri implements IEquatable<Uri>, so a URN-typed Equals(other) call would otherwise bind
    // to Uri's case-sensitive comparison instead of this one. Route every path
    // (Equals(object), Equals(URN), ==) through the same NID/NSS check.
    /// <inheritdoc/>
    public bool Equals(URN? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(other, this)) return true;
        return
            string.Equals(NID, other.NID, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(NSS, other.NSS, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? other) => Equals(other as URN);

    // Mirrors Equals: NID case-insensitive (ASCII, per RFC 8141), NSS ordinal.
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        StringComparer.OrdinalIgnoreCase.GetHashCode(NID),
        StringComparer.Ordinal.GetHashCode(NSS));

    /// <summary>Compares two URNs by <see cref="NID"/> (case-insensitively) and <see cref="NSS"/>.</summary>
    public static bool operator ==(URN? u1, URN? u2)
        => u1 is null ? u2 is null : u1.Equals(u2);

    /// <summary>Compares two URNs by <see cref="NID"/> (case-insensitively) and <see cref="NSS"/>.</summary>
    public static bool operator !=(URN? u1, URN? u2) => !(u1 == u2);
}
