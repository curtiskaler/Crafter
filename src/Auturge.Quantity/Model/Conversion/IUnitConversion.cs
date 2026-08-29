namespace Auturge.Quantity;

public interface IUnitConversion
{
    public Unit SourceUnit { get; }
    public Unit TargetUnit { get; }
}
