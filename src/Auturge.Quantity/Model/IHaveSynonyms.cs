namespace Auturge.Quantity;

internal interface IHaveSynonyms<out T>
{
    /// <summary>
    /// The synonyms for this entity.
    /// </summary>
    List<Synonym> Synonyms { get; }
    
    T AddSynonym(IHaveNameAndSymbol synonym);
}
