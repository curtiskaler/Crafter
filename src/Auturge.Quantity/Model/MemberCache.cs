#pragma warning disable CS8604 // Possible null reference argument.

using System.Diagnostics.CodeAnalysis;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable MemberCanBeProtected.Global

namespace Auturge.Quantity;

public abstract class MemberCache<TSelf, TElement>
    where TSelf : MemberCache<TSelf, TElement>, new()
    where TElement : class
{
    internal static List<TElement> Items => field ??= [];

    static MemberCache()
    {
        // populate the static list with all the public static field/property members of type Dimension.
        var staticElements = GetStaticElements();

        // someone might have already populated List, so let's make sure we don't lose any of those.
        // However, we want to be sure that the STATIC dimensions take precedence over any auto-generated dimensions.
        var extendedList = new List<TElement>()
            .Concat(staticElements)
            .Concat(Items);

        var distinct = extendedList.Distinct().ToList();
        Items = distinct;
    }

    /// <summary>
    /// Adds one or more new <see cref="UnitConversion"/>s to the static list.
    /// </summary>
    /// <param name="list"></param>
    public static void Add(params TElement[] list)
    {
        foreach (var entry in list)
        {
            if (!Items.Contains(entry))
            {
                Items.Add(entry);
            }
        }
    }
    
    public static bool TryFind(Func<TElement, bool> selector, [MaybeNullWhen(false)] out TElement unit)
    {
        unit = Items.FirstOrDefault(selector);
        return unit != null;
    }

    internal static List<TElement> GetStaticElements() => Reflection.GetStaticElements<TSelf, TElement>();
}
