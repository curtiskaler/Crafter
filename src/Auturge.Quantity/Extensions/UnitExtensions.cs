namespace Auturge.Quantity;

public static class UnitExtensions
{
    public static List<Unit> GetNumeratorUnits(this Unit unit)
    {
        List<Unit> list = unit.Definition.GetUnitsWhere(x => x.Value > 0);
        return list.Count > 0 ? list : [Unit.One];
    }

    public static List<Unit> GetDenominatorUnits(this Unit unit)
    {
        List<Unit> list = unit.Definition.GetUnitsWhere(x => x.Value < 0);
        return list.Count > 0 ? list : [Unit.One];
    }
}
