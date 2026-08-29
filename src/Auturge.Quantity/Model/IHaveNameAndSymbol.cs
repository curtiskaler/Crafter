namespace Auturge.Quantity;

public interface IHaveNameAndSymbol
{
    /// <summary>
    /// The name (or i18n key) to be resolved and displayed in the UI.
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// The symbol or abbreviation for the given entity.
    /// </summary>
    string? Symbol { get; }
}
