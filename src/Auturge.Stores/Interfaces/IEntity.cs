namespace Auturge.Stores;

public interface IEntity; // a marker interface

public interface IStoredEntity : IEntity;

public interface IStoredEntity<TKey> : IStoredEntity, IIdentifiable<TKey> where TKey : notnull;

// Archetype guidance:
// - IIdentifiable<TKey> is not appropriate for composite-key entities.
// - IAudit is typically inappropriate for lookup tables.
// - ISoftDeletable and IConcurrentEntity are inappropriate for history tables.
// Compose the archetypes directly, or derive from StoredEntity (Id + concurrency)
// or AuditEntity (adds audit + soft-delete).
