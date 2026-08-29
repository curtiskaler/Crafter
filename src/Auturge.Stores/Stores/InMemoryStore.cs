using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Auturge.Stores.Stores;

/// <summary>
/// A high-performance, thread-safe, in-memory implementation of the <see cref="IStore{TId, TEntity}"/> contract.
/// </summary>
/// <remarks>
/// This class uses an internal <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/> 
/// to manage entities, making it highly suitable for high-concurrency web applications, lightweight 
/// local caching, or fast-executing automated unit and integration testing pipelines. 
/// Memory pointer swaps and lookup structures run in O(1) constant time, while expression-driven queries 
/// execute as linear O(N) scans via runtime reflection compilation.
/// </remarks>
/// <typeparam name="TId">The unique identifier type for the entities. Must be non-nullable and support equality checks.</typeparam>
/// <typeparam name="TEntity">The reference type of the domain or database entity being managed.</typeparam>
public class InMemoryStore<TId, TEntity>(
    Func<TEntity, TId> idSelector,
    Func<TEntity, string>? versionSelector = null,
    Action<TEntity, string>? versionUpdater = null
) : IStore<TId, TEntity>
    where TId : IEquatable<TId>
    where TEntity : class, IStoredEntity<TId>
{
    // Thread-safe collection to store entities by their unique ID
    private readonly ConcurrentDictionary<TId, TEntity> _storage = new();

    // Delegate or function to extract the ID from an entity dynamically
    private readonly Func<TEntity, TId> _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));

    // Delegates to read and assign the concurrency version safely
    private readonly Func<TEntity, string> _versionSelector =
        versionSelector ?? (e => e.ConcurrencyToken);

    private readonly Action<TEntity, string> _versionUpdater =
        versionUpdater ?? ((entity, token) => { entity.ConcurrencyToken = token; });

    /// <inheritdoc/>
    public Task<TEntity> Add(TEntity? entity, CancellationToken cancellationToken = default)
    {
        // 1. Validate input immediately before dealing with async tasks
        if (entity == null)
        {
            return Task.FromException<TEntity>(new ArgumentNullException(nameof(entity)));
        }

        // 2. Honor the cancellation token immediately before starting any work
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TEntity>(cancellationToken);
        }

        try
        {
            // 3. Extract the primary key from the object
            TId id = _idSelector(entity);

            // 4. Attempt a thread-safe insertion
            if (!_storage.TryAdd(id, entity))
            {
                throw new ArgumentException($"An item with the ID '{id}' already exists in the store.");
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            string newVersionToken = now.Ticks.ToString();            
            _versionUpdater(entity, newVersionToken);
            
            entity.Created = now;
            entity.LastUpdated = now;

            // 5. Complete synchronously since no physical disk or network I/O happened
            return Task.FromResult(entity);
        }
        catch (Exception ex)
        {
            // Return failed tasks for internal system exceptions to preserve the async signature behavior
            return Task.FromException<TEntity>(ex);
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> AddRange(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate the primary input argument
        if (entities == null) // this will never happen, assuming IEnumerable<TEntity> is not nullable
        {
            return Task.FromException<IEnumerable<TEntity>>(new ArgumentNullException(nameof(entities)));
        }

        // 2. Honor the cancellation token immediately before starting any work
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
        }

        try
        {
            // Track items added during this batch operation to facilitate rollback or return paths
            var addedIds = new List<TId>();
            var addedEntities = new List<TEntity>();

            foreach (var entity in entities)
            {
                // Periodically check cancellation inside the loop for larger batches
                if (cancellationToken.IsCancellationRequested)
                {
                    Rollback(addedIds);
                    return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
                }

                if (entity == null)
                {
                    Rollback(addedIds);
                    throw new ArgumentNullException(nameof(entities), "The collection contains a null entity entry.");
                }

                TId id = _idSelector(entity);

                // Attempt atomic insertion. Roll back the entire batch if a duplicate ID breaks transactional integrity.
                if (!_storage.TryAdd(id, entity))
                {
                    Rollback(addedIds);
                    throw new ArgumentException($"An item with the ID '{id}' already exists. Transaction aborted.");
                }

                DateTimeOffset now = DateTimeOffset.UtcNow;
                string newVersionToken = now.Ticks.ToString();            
                _versionUpdater(entity, newVersionToken);
                entity.Created = now;
                entity.LastUpdated = now;

                addedIds.Add(id);
                addedEntities.Add(entity);
            }

            return Task.FromResult<IEnumerable<TEntity>>(addedEntities);
        }
        catch (Exception ex)
        {
            return Task.FromException<IEnumerable<TEntity>>(ex);
        }
    }

    /// <inheritdoc/>
    public Task<bool> ContainsKey(TId id, CancellationToken cancellationToken = default)
    {
        // 1. Validate input immediately
        if (id == null)
        {
            return Task.FromException<bool>(new ArgumentNullException(nameof(id)));
        }

        // 2. Honor cancellation token before reading state
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(cancellationToken);
        }

        // 3. Atomically check existence inside the thread-safe collection
        bool exists = _storage.ContainsKey(id);

        // 4. Wrap the boolean answer in a completed task to satisfy the interface asynchronously
        return Task.FromResult(exists);
    }

    /// <inheritdoc/>
    public Task<bool> Delete(TId id, CancellationToken cancellationToken = default)
    {
        // 1. Validate input immediately
        if (id == null)
        {
            return Task.FromException<bool>(new ArgumentNullException(nameof(id)));
        }

        // 2. Honor cancellation token before mutating data
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(cancellationToken);
        }

        // TryRemove returns true if the key was found and removed, false otherwise
        bool removed = _storage.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    /// <inheritdoc/>
    public Task<bool> Delete(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            return Task.FromException<bool>(new ArgumentNullException(nameof(entity)));
        }

        try
        {
            // Extract the key using the configured selector and route to the primary delete routine
            TId id = _idSelector(entity);
            return Delete(id, cancellationToken);
        }
        catch (Exception ex)
        {
            return Task.FromException<bool>(ex);
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> FindAllBy(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate parameters immediately
        if (predicate == null)
        {
            return Task.FromException<IEnumerable<TEntity>>(new ArgumentNullException(nameof(predicate)));
        }

        // 2. Check cancellation token before running expression compilation
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
        }

        try
        {
            // 3. Compile the Expression Tree into an executable Func delegate
            Func<TEntity, bool> compiledPredicate = predicate.Compile();

            var matchedResults = new List<TEntity>();

            // 4. Iterate over a thread-safe snapshot of the storage values
            foreach (TEntity entity in _storage.Values)
            {
                // Check cancellation mid-loop to remain responsive during large table scans
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
                }

                if (compiledPredicate(entity))
                {
                    matchedResults.Add(entity);
                }
            }

            // 5. Wrap the finalized list inside a completed task
            return Task.FromResult<IEnumerable<TEntity>>(matchedResults);
        }
        catch (Exception ex)
        {
            return Task.FromException<IEnumerable<TEntity>>(ex);
        }
    }

    /// <inheritdoc/>
    public Task<TEntity?> FindBy(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate input arguments
        if (predicate == null)
        {
            return Task.FromException<TEntity?>(new ArgumentNullException(nameof(predicate)));
        }

        // 2. Honor cancellation token before reading state
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TEntity?>(cancellationToken);
        }

        try
        {
            // 3. Compile the Expression Tree into an executable Func delegate
            Func<TEntity, bool> compiledPredicate = predicate.Compile();

            // 4. Iterate over a thread-safe snapshot of values
            foreach (var entity in _storage.Values)
            {
                // Verify cancellation mid-loop if dealing with a high volume of entries
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled<TEntity?>(cancellationToken);
                }

                if (compiledPredicate(entity))
                {
                    return Task.FromResult<TEntity?>(entity);
                }
            }

            // 5. Explicitly return null if no matching records are discovered
            return Task.FromResult<TEntity?>(null);
        }
        catch (Exception ex)
        {
            return Task.FromException<TEntity?>(ex);
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default)
    {
        // 1. Honor cancellation token immediately
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
        }

        // 2. Extract a thread-safe snapshot of the values collection
        // ConcurrentDictionary.Values extracts a snapshot copy to prevent subsequent mutations from breaking iteration
        IEnumerable<TEntity> currentValues = _storage.Values;

        return Task.FromResult(currentValues);
    }

    /// <inheritdoc/>
    public Task<TEntity?> GetById(TId id, CancellationToken cancellationToken = default)
    {
        // 1. Validate input immediately
        if (id == null)
        {
            return Task.FromException<TEntity?>(new ArgumentNullException(nameof(id)));
        }

        // 2. Honor cancellation token before reading state
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TEntity?>(cancellationToken);
        }

        // 3. Atomically look up the key in the thread-safe collection
        if (_storage.TryGetValue(id, out TEntity? entity))
        {
            return Task.FromResult<TEntity?>(entity);
        }

        // 4. Return null inside a completed task to mimic a missing database record
        return Task.FromResult<TEntity?>(null);
    }

    /// <summary>
    /// Flushes staged mutations down to the persistent backend. 
    /// For an in-memory store, operations are instant, making this a high-performance no-op.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the synchronous completion of the save operation.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        // Honor the cancellation token immediately
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<int>(cancellationToken);
        }

        // The in-memory store applies updates instantly, so return 0 or a mocked row count
        return Task.FromResult(0);
    }

    /// <inheritdoc/>
    public Task<TEntity> Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        // 1. Validate input immediately
        if (entity == null)
        {
            return Task.FromException<TEntity>(new ArgumentNullException(nameof(entity)));
        }

        // 2. Honor cancellation token before modifying state
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TEntity>(cancellationToken);
        }

        // 3. Extract the primary key from the object
        TId id = _idSelector(entity);

        // ConcurrentDictionary.Update requires a loop pattern to handle atomic compare-and-swap safely
        while (true)
        {
            // 1. Ensure the record actually exists first
            if (!_storage.TryGetValue(id, out TEntity? existingEntity))
            {
                return Task.FromException<TEntity>(
                    new KeyNotFoundException($"Cannot update item. ID '{id}' not found."));
            }

            // 2. Extract tokens from both the existing data and the incoming update payload
            string currentStoredVersion = _versionSelector(existingEntity);
            string incomingVersion = _versionSelector(entity);

            // 3. Concurrency Check: If they don't match, another thread modified this row in the background
            if (currentStoredVersion != incomingVersion)
            {
                // This mimics Entity Framework's DbUpdateConcurrencyException
                return Task.FromException<TEntity>(new InvalidOperationException(
                    $"Concurrency violation encountered. The entity with ID '{id}' has been modified by another process."));
            }

            // 4. Generate a brand new version token for this update cycle
            string newVersionToken = DateTimeOffset.UtcNow.Ticks.ToString();
            _versionUpdater(entity, newVersionToken);

            // 5. Perform an atomic pointer replacement. 
            // If another thread slipped past us and swapped existingEntity out right now, TryUpdate returns false.
            if (_storage.TryUpdate(id, entity, existingEntity))
            {
                return Task.FromResult(entity); // Success! Break out of loop.
            }
        }
    }

    private void Rollback(IEnumerable<TId> idsToRemove)
    {
        foreach (TId id in idsToRemove)
        {
            _storage.TryRemove(id, out _);
        }
    }
}

/// <summary>
/// A high-performance, thread-safe, in-memory implementation of the <see cref="IStore{TId, TEntity}"/> contract.
/// </summary>
/// <remarks>
/// This class uses an internal <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/> 
/// to manage entities, making it highly suitable for high-concurrency web applications, lightweight 
/// local caching, or fast-executing automated unit and integration testing pipelines. 
/// Memory pointer swaps and lookup structures run in O(1) constant time, while expression-driven queries 
/// execute as linear O(N) scans via runtime reflection compilation.
/// </remarks>
/// <typeparam name="TEntity">The reference type of the domain or database entity being managed.</typeparam>
public class InMemoryStore<TEntity>() : InMemoryStore<long, TEntity>(entity => entity.Id)
    where TEntity : class, IStoredEntity<long>;
