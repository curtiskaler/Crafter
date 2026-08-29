using System.Linq.Expressions;

namespace Auturge.Stores;

/// <summary> Defines a generic, asynchronous repository contract for a unified data store. </summary>
/// <remarks>
/// This abstraction decouples business services from infrastructure choices. It handles standard 
/// CRUD operations, predicate-driven queries, and transactional persistence. It supports both 
/// relational databases (via expressions) and high-performance in-memory mock testing providers.
/// </remarks>
/// <typeparam name="TId">The contravariant type of the primary key identifier. Must be non-nullable.</typeparam>
/// <typeparam name="TEntity">The type of the entity managed by the store. Must be a reference class type.</typeparam>
public interface IStore<in TId, TEntity> 
    where TId : IEquatable<TId> 
    where TEntity : class, IStoredEntity<TId>
{
    /// <summary>
    /// Asynchronously inserts a new entity into the data store.
    /// </summary>
    /// <param name="entity">The entity instance to add to the store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the persisted entity as tracked by the store.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided entity is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<TEntity> Add(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously inserts a collection of new entities into the in-memory data store.
    /// </summary>
    /// <param name="entities">The collection of entities to add to the store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of successfully persisted entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided collection or any entity within it is null.</exception>
    /// <exception cref="ArgumentException">Thrown if an entity with a duplicate ID is detected or already exists.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<IEnumerable<TEntity>> AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously determines whether the data store contains an entity with the specified unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier to locate in the data store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the entity exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided ID is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<bool> ContainsKey(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously compiles the expression predicate and scans the in-memory store for the first matching entity.
    /// </summary>
    /// <param name="predicate">An expression tree representing the conditional check logic.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching entity, or null if no match is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided predicate expression is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<TEntity?> FindBy(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously filters a collection of entities based on a predicate logic.
    /// </summary>
    /// <param name="predicate">A function to test each entity for a condition.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains all matching elements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided predicate is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<IEnumerable<TEntity>> FindAllBy(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously retrieves an entity from the in-memory data store using its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching entity, or null if no match is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided ID is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<TEntity?> GetById(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously retrieves all entities currently present in the in-memory data store.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of all entities.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously updates an existing entity within the in-memory data store.
    /// </summary>
    /// <param name="entity">The entity instance containing updated values.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided entity is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the entity does not exist in the store.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<TEntity> Update(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously removes an entity from the in-memory data store using its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided ID is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if no entity matches the provided ID.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<bool> Delete(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously removes a specific entity instance from the in-memory data store.
    /// </summary>
    /// <param name="entity">The entity instance to remove from the store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided entity is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the entity does not exist in the store.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<bool> Delete(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary> Flushes staged mutations down to the persistent backend. </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the operation. The result contains the number of state mutations applied.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    Task<int> SaveChanges(CancellationToken cancellationToken = default);
}

/// <summary> Defines a generic, asynchronous repository contract for a unified data store. </summary>
/// <remarks>
/// This abstraction decouples business services from infrastructure choices. It handles standard 
/// CRUD operations, predicate-driven queries, and transactional persistence. It supports both 
/// relational databases (via expressions) and high-performance in-memory mock testing providers.
/// </remarks>
/// <typeparam name="TEntity">The type of the entity managed by the store. Must be a reference class type.</typeparam>
public interface IStore<TEntity> : IStore<long, TEntity> where TEntity : class, IStoredEntity<long>;
