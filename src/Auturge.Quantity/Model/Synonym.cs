// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Auturge.Quantity;

public class Synonym : IHaveNameAndSymbol
{
    public string? DisplayName { get; init; }
    public string? Symbol { get; init; }
    public override string ToString() => $@"{DisplayName} ({Symbol})";

    public Synonym()
    {
    }

    public Synonym(IHaveNameAndSymbol nameAndSymbol) : this(nameAndSymbol.DisplayName, nameAndSymbol.Symbol)
    {
    }

    public Synonym(string? displayName, string? symbol)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        DisplayName = displayName;
        Symbol = symbol;
    }
}
