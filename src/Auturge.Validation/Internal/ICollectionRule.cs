namespace Auturge.Validation;

/// <summary>
/// Represents a rule defined against a collection with RuleForEach.
/// </summary>
/// <typeparam name="TObj">Root object</typeparam>
/// <typeparam name="TElement">Type of each element in the collection</typeparam>
public interface ICollectionRule<TObj, TElement> : IValidationRule<TObj, TElement>
{
    /// <summary>
    /// Filter that should include/exclude items in the collection.
    /// </summary>
    Func<TElement, bool>? Filter { get; set; }

    /// <summary>
    /// Constructs the indexer in the property name associated with the error message.
    /// By default, this is "[" + index + "]"
    /// </summary>
    Func<TObj?, IEnumerable<TElement>, TElement, int, string>? IndexBuilder { get; set; }
}
