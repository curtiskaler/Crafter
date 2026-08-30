using System.Text.RegularExpressions;
using Auturge.Identifiers;

namespace Crafter.Model.Identifiers;

/// <summary>
/// A Crafter URN: <c>urn:auturge-crafter:&lt;entityType&gt;:&lt;id&gt;</c>. The NSS is an
/// entity-type path (which may itself contain <c>:</c>) followed by a single colon-free id
/// segment.
/// </summary>
public partial class URN : Auturge.Identifiers.URN
{
    private const string _namespaceId = "auturge-crafter";

    // EntityType is everything before the final ':'; Id is the last colon-free segment.
    [GeneratedRegex(@"^(?<EntityType>.+):(?<Id>[^:]+)$", _urnRegexOptions)]
    private static partial Regex NssRegex();

    /// <summary>The entity-type portion of the NSS (everything before the final <c>:</c>).</summary>
    public string EntityType { get; }

    /// <summary>The id portion of the NSS (the final colon-free segment).</summary>
    public string Id { get; }

    /// <summary>
    /// Parses a full <c>urn:auturge-crafter:&lt;entityType&gt;:&lt;id&gt;</c> string.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The scheme, NID, or NSS shape is wrong.</exception>
    public URN(string s) : base(s)
    {
        if (!string.Equals(NID, _namespaceId, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"URN NID must be '{_namespaceId}', not '{NID}'.");
        }

        Match match = NssRegex().Match(NSS);
        if (!match.Success)
        {
            throw new FormatException($"URN NSS must be '<entityType>:<id>', not '{NSS}'.");
        }

        EntityType = match.Groups["EntityType"].Value;
        Id = match.Groups["Id"].Value;
    }

    /// <summary>
    /// Composes a Crafter URN from an <paramref name="entityType"/> and <paramref name="id"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">A part is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">
    /// A part contains characters not valid in a URN, or <paramref name="id"/> contains a <c>:</c>.
    /// </exception>
    public URN(string entityType, string id) : this(Compose(entityType, id))
    {
    }

    private static string Compose(string entityType, string id)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(id);
        if (id.Contains(':'))
        {
            throw new FormatException($"URN id must not contain ':' — got '{id}'.");
        }

        return $"urn:{_namespaceId}:{entityType}:{id}";
    }
}
