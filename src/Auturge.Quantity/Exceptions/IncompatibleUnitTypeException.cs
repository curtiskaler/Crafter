namespace Auturge.Quantity.Exceptions;

public class IncompatibleUnitTypeException : UnitException
{
    public string FromUnit { get; }
    public string ToUnit { get; }
    public string Expression { get; } = "";

    public IncompatibleUnitTypeException(string op, IQuantity lhs, IQuantity rhs) 
    {
        FromUnit = rhs.Unit.DisplayName;
        ToUnit = lhs.Unit.DisplayName;
        Expression = @$"{lhs} {op} {rhs}";
    }
    
    public IncompatibleUnitTypeException(string fromUnit, string toUnit) 
    {
        FromUnit = fromUnit;
        ToUnit = toUnit;
    }
}
