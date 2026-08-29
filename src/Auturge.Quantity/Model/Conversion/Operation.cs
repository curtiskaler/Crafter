namespace Auturge.Quantity;

public abstract class Operation
{
    /// <summary>
    /// The list of functions required to execute the whole operation.
    /// </summary>
    protected internal readonly List<Func<object, object>> Functions = [];
}
