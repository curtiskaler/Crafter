namespace Auturge.Quantity;

public static class DefinitionExtensions
{
    public static List<Unit> GetUnitsWhere(this UnitDefinition definition,
        Func<KeyValuePair<Unit, short>, bool> whereFn)
    {
        var pairs = definition.Where(whereFn).ToList();
        var units = pairs.Select(x => x.Key).ToList();
        return units;
    }
}
