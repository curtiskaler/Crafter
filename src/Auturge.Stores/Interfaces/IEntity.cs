namespace Auturge.Stores;

/// <summary> Marker for a type that the store layer manages. Carries no members. </summary>
public interface IEntity;

/// <summary> Marker for an entity that is persisted as a stored record. </summary>
public interface IStoredEntity : IEntity;

/// <summary> An <see cref="IStoredEntity"/> addressed by a single primary key of type <typeparamref name="TKey"/>. </summary>
/// <remarks>
/// Compose the capability interfaces directly, or derive from <c>StoredEntity</c> (adds an id and a
/// concurrency token) or <c>AuditEntity</c> (also adds audit and soft-delete). Guidance:
/// <list type="bullet">
///   <item><see cref="IIdentifiable{TKey}"/> is not appropriate for composite-key entities.</item>
///   <item><see cref="IAudit"/> is typically inappropriate for lookup tables.</item>
///   <item><see cref="ISoftDeletable"/> and <see cref="IConcurrentEntity"/> are inappropriate for history tables.</item>
/// </list>
/// </remarks>
/// <typeparam name="TKey">The non-nullable primary-key type.</typeparam>
public interface IStoredEntity<TKey> : IStoredEntity, IIdentifiable<TKey> where TKey : notnull;
