using System.Linq.Expressions;

namespace Auturge.Stores;

/// <summary> Defines a generic, asynchronous repository contract for a unified data store. </summary>
/// <remarks>
/// This abstraction decouples business services from infrastructure choices. It handles standard
/// CRUD operations, predicate-driven queries, and transactional persistence, and fronts both
/// relational databases and in-memory providers. Implement this version directly for
/// composite-key or join tables.
/// </remarks>
/// <typeparam name="TEntity">The reference type of the entity managed by the store.</typeparam>
public interface IStore<TEntity> where TEntity : class, IStoredEntity
{
    /// <summary> Returns a composable query over the store's non-deleted entities. </summary>
    IQueryable<TEntity> Query();

    /// <summary> Asynchronously inserts a new entity into the store. </summary>
    /// <param name="entity">The entity instance to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// The added entity with store-assigned fields populated (concurrency token, audit timestamps).
    /// Stores may track an independent copy; mutations made after this call are not persisted until <see cref="UpdateAsync"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The provided entity is null.</exception>
    /// <exception cref="ArgumentException">An entity with the same identifier already exists.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary> Asynchronously inserts a collection of new entities as a single batch. </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The added entities with store-assigned fields populated.</returns>
    /// <exception cref="ArgumentNullException">The collection is null, or contains a null entry.</exception>
    /// <exception cref="ArgumentException">An entity with a duplicate identifier was detected; the batch is rolled back.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary> Asynchronously deletes an entity. Soft-deletable entities are flagged rather than removed. </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><see langword="true"/> if an entity was deleted; <see langword="false"/> if it was not present or was already deleted.</returns>
    /// <exception cref="ArgumentNullException">The provided entity is null.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary> Asynchronously returns every non-deleted entity matching a predicate. </summary>
    /// <param name="predicate">A function to test each entity for a condition.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>All matching entities.</returns>
    /// <exception cref="ArgumentNullException">The provided predicate is null.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<IEnumerable<TEntity>> FindAllByAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary> Asynchronously returns the first non-deleted entity matching a predicate, or <see langword="null"/>. </summary>
    /// <param name="predicate">A function to test each entity for a condition.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching entity, or <see langword="null"/> if none match.</returns>
    /// <exception cref="ArgumentNullException">The provided predicate is null.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<TEntity?> FindByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary> Asynchronously retrieves every non-deleted entity in the store. </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>All non-deleted entities.</returns>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary> Flushes staged mutations to the backing store. </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of state entries written.</returns>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary> Asynchronously updates an existing entity. </summary>
    /// <param name="entity">The entity instance carrying the updated values.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The updated entity with a refreshed concurrency token and audit timestamp.</returns>
    /// <exception cref="ArgumentNullException">The provided entity is null.</exception>
    /// <exception cref="KeyNotFoundException">No entity with a matching identifier exists.</exception>
    /// <exception cref="InvalidOperationException">The entity's concurrency token is stale (it was modified by another writer).</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
}

/// <summary> Defines a generic, asynchronous repository contract for a single-key entity table. </summary>
/// <remarks>
/// Extends <see cref="IStore{TEntity}"/> with identifier-addressed operations. Implement this version
/// directly for entities keyed by a single property.
/// </remarks>
/// <typeparam name="TEntity">The reference type of the entity managed by the store.</typeparam>
/// <typeparam name="TKey">The contravariant, non-nullable type of the primary key.</typeparam>
public interface IStore<TEntity, in TKey> : IStore<TEntity>
    where TEntity : class, IStoredEntity, IIdentifiable<TKey>
    where TKey : notnull
{
    /// <summary> Asynchronously determines whether a non-deleted entity with the given identifier exists. </summary>
    /// <param name="id">The identifier to locate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><see langword="true"/> if a non-deleted entity exists; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The provided identifier is null.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<bool> ContainsKeyAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary> Asynchronously deletes the entity with the given identifier. Soft-deletable entities are flagged rather than removed. </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><see langword="true"/> if an entity was deleted; <see langword="false"/> if it was not present or was already deleted.</returns>
    /// <exception cref="ArgumentNullException">The provided identifier is null.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary> Asynchronously retrieves the non-deleted entity with the given identifier, or <see langword="null"/>. </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching entity, or <see langword="null"/> if none exists or it is soft-deleted.</returns>
    /// <exception cref="ArgumentNullException">The provided identifier is null.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the cancellation token.</exception>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
}
