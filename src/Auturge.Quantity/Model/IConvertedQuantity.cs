using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Auturge.Quantity;

internal interface IConvertedQuantity
{
    object ConvertedFrom { get; }
}

internal class ConvertedQuantity<T> : Quantity<T>, IConvertedQuantity where T : IEquatable<T>, INumber<T>, IConvertible
{
    // Stored as an object to prevent any generic variance/casting loops
    public object ConvertedFrom { get; }
    
    object IConvertedQuantity.ConvertedFrom => ConvertedFrom;

    public ConvertedQuantity(Quantity<T> qty, Quantity<T> original) : this(qty.Amount, qty.Unit, original)
    {
    }

    internal ConvertedQuantity(T amount, Unit unit, object convertedFrom) : base(amount, unit)
    {
        // 1. Validate that the object actually is a Quantity<AnyType>
        if (!IsQuantityType(convertedFrom.GetType()))
        {
            throw new ArgumentException("ConvertedFrom must be an instance of Quantity<T>.");
        }
       
        ConvertedFrom = convertedFrom;
    }
    
    
    
    public override Quantity<T> ConvertTo(Unit targetUnit)
    {
        // If possible, cheat: don't convert. Revert.
        bool found = TryFindReversion(this, targetUnit, out Quantity<T>? reversion);
        return found && reversion != null ? reversion : base.ConvertTo(targetUnit);
    }

    private static bool IsQuantityType(Type type)
    {
        // Traverses the inheritance tree to see if the object inherits from Quantity<T>
        while (type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Quantity<>))
            {
                return true;
            }
            type = type.BaseType!;
        }
        return false;
    }
    
    private static bool TryFindReversion(IQuantity convertedFrom, Unit targetUnit,
        [MaybeNullWhen(false)] out Quantity<T> reversion)
    {
        // While recursion is quite readable, this entity is structurally a Linked List,
        // and not a branching tree, so this (performance-wise) is the absolute best case
        // for iteration instead of recursion.
        // Using iteration (instead of recursion) prevents stack overflows and
        // significantly reduces memory overhead.
        // This is unlikely to be premature optimization because we have no idea
        // (or control over!) how deep the list goes.

        while (true)
        {
            reversion = null;
            if (targetUnit == convertedFrom.Unit)
            {
                reversion = (convertedFrom as Quantity<T>)!;
                return true;
            }

            if (convertedFrom is ConvertedQuantity<T> nextStep)
            {
                convertedFrom = (IQuantity)nextStep.ConvertedFrom;
                continue;
            }

            return false;
        }
    }
}
