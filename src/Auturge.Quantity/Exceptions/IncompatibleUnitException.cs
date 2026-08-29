namespace Auturge.Quantity.Exceptions;

public class IncompatibleUnitException : UnitException
{
    public string FromUnit { get; }
    public string ToUnit { get; }
    public string Expression { get; } = "";

    public IncompatibleUnitException(string op, IQuantity lhs, IQuantity rhs) 
    {
        FromUnit = rhs.Unit.DisplayName;
        ToUnit = lhs.Unit.DisplayName;
        Expression = @$"{lhs} {op} {rhs}";
    }
    
    public IncompatibleUnitException(string fromUnit, string toUnit) 
    {
        FromUnit = fromUnit;
        ToUnit = toUnit;
    }
}
