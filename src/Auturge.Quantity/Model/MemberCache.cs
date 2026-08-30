#pragma warning disable CS8604 // Possible null reference argument.

using System.Diagnostics.CodeAnalysis;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable MemberCanBeProtected.Global

namespace Auturge.Quantity;

public abstract class MemberCache<TSelf, TElement>
    where TSelf : MemberCache<TSelf, TElement>, new()
    where TElement : class
{
    private static readonly List<TElement> _items = [];
    private static int _previousStaticCount = -1;
    private static bool _settled;

    /// <summary>
    /// Every known element: the <c>public static</c> members declared on <typeparamref name="TSelf"/>
    /// (reflected in) plus any added at runtime via <see cref="Add"/>.
    /// </summary>
    internal static List<TElement> Items
    {
        get
        {
            if (!_settled)
            {
                MergeStaticElements();
            }

            return _items;
        }
    }

    /// <summary>
    /// Adds one or more elements to the cache, skipping any that are already present.
    /// </summary>
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

    // The static members are merged in on access rather than in a static constructor: a type
    // initializer for MemberCache can be triggered while TSelf's own initializer is still running
    // (TSelf init -> a Unit/Dimension operator -> MemberCache.Items), which would otherwise freeze
    // the cache around a half-built snapshot and lose every member declared further down the file.
    // Re-scanning until the reflected member count settles closes that window.
    private static void MergeStaticElements()
    {
        List<TElement> staticElements = GetStaticElements();

        foreach (TElement element in staticElements)
        {
            int index = _items.FindIndex(existing => existing.Equals(element));
            if (index < 0)
            {
                _items.Add(element);
            }
            else if (!ReferenceEquals(_items[index], element))
            {
                // A named static member supersedes an equivalent auto-generated entry.
                _items[index] = element;
            }
        }

        // Once two consecutive scans report the same count, TSelf has finished initializing.
        if (staticElements.Count == _previousStaticCount)
        {
            _settled = true;
        }

        _previousStaticCount = staticElements.Count;
    }
}
