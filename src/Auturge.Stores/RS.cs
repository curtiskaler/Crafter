using System.Globalization;
using System.Resources;

namespace Auturge.Stores;

/// <summary> Localized resource strings for the <c>Auturge.Stores</c> assembly. </summary>
internal static class RS
{
    private static readonly ResourceManager Manager = new("Auturge.Stores.RS", typeof(RS).Assembly);

    /// <summary> Overrides the culture used for resource lookups. Defaults to the current UI culture. </summary>
    internal static CultureInfo? Culture { get; set; }

    internal static string DuplicateId(object id) => Format(nameof(DuplicateId), id);

    internal static string NullEntityInBatch() => Lookup(nameof(NullEntityInBatch));

    internal static string KeyNotFound(object id) => Format(nameof(KeyNotFound), id);

    internal static string ConcurrencyViolation(object id) => Format(nameof(ConcurrencyViolation), id);

    private static string Lookup(string key) =>
        Manager.GetString(key, Culture ?? CultureInfo.CurrentUICulture) ?? key;

    private static string Format(string key, params object?[] args) =>
        string.Format(Culture ?? CultureInfo.CurrentUICulture, Lookup(key), args);
}
