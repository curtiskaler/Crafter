namespace Auturge.Stores;

/// <summary> An entity with a single primary key. </summary>
/// <typeparam name="TKey">The non-nullable primary-key type.</typeparam>
public interface IIdentifiable<TKey> : IEntity where TKey : notnull
{
    /// <summary> The primary key. Set once when the entity is created. </summary>
    TKey Id { get; init; }
}
