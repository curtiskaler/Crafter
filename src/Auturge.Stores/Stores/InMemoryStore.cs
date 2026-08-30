using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Auturge.Stores.Stores;

/// <summary>
/// A thread-safe, in-memory implementation of <see cref="IStore{TEntity, TKey}"/> backed by a
/// <see cref="ConcurrentDictionary{TKey, TValue}"/>. Suited to unit and integration tests and to
/// lightweight local caching.
/// </summary>
/// <remarks>
/// The store keeps entities isolated from caller state: a field-wise copy is taken on every write and
/// returned on every read, so mutating an instance after handing it to the store (or after reading one
/// back) has no effect until the next <see cref="UpdateAsync"/>. The copy is shallow &#8212; reference-typed
/// members (nested entities, collections) are shared between the caller's instance and the stored copy.
/// </remarks>
/// <typeparam name="TEntity">The reference type of the entity being managed.</typeparam>
/// <typeparam name="TKey">The non-nullable primary-key type.</typeparam>
public class InMemoryStore<TEntity, TKey>(Func<TEntity, TKey>? idSelector = null) : IStore<TEntity, TKey>
    where TEntity : class, IStoredEntity<TKey>
    where TKey : notnull
{
    private static readonly ConcurrentDictionary<Type, FieldInfo[]> FieldCache = new();

    private readonly ConcurrentDictionary<TKey, TEntity> _storage = new();
    private readonly Func<TEntity, TKey> _idSelector = idSelector ?? (e => e.Id);

    // A soft-deleted entity is hidden from every read path (Query, GetById, GetAll, Find*, ContainsKey).
    // Entities that don't implement ISoftDeletable are always visible.
    private static bool IsSoftDeleted(TEntity entity) => entity is ISoftDeletable { IsDeleted: true };

    /// <summary> Applies store-assigned insert fields: the given concurrency token and audit timestamps. </summary>
    private static void StampForInsert(TEntity entity, Guid version, DateTimeOffset now)
    {
        if (entity is IConcurrentEntity concurrent)
        {
            concurrent.ConcurrencyToken = version;
        }

        if (entity is IAudit audit)
        {
            audit.Created = now;
            audit.LastUpdated = now;
        }
    }

    /// <summary> Returns a field-wise shallow copy, decoupling stored state from the caller's instance. </summary>
    private static TEntity Detach(TEntity source)
    {
        Type type = source.GetType();
        var copy = (TEntity)RuntimeHelpers.GetUninitializedObject(type);

        foreach (FieldInfo field in FieldCache.GetOrAdd(type, ReadInstanceFields))
        {
            field.SetValue(copy, field.GetValue(source));
        }

        return copy;
    }

    private static FieldInfo[] ReadInstanceFields(Type type)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        var fields = new List<FieldInfo>();
        for (Type? level = type; level is not null && level != typeof(object); level = level.BaseType)
        {
            fields.AddRange(level.GetFields(flags));
        }

        return fields.ToArray();
    }

    /// <inheritdoc/>
    public IQueryable<TEntity> Query() =>
        _storage.Values.Where(e => !IsSoftDeleted(e)).Select(Detach).AsQueryable();

    /// <inheritdoc/>
    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            return Task.FromException<TEntity>(new ArgumentNullException(nameof(entity)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TEntity>(cancellationToken);
        }

        try
        {
            TKey id = _idSelector(entity);
            DateTimeOffset now = DateTimeOffset.UtcNow;
            Guid version = Guid.CreateVersion7(now);

            // Publish an already-stamped copy so a concurrent reader never sees an unstamped row,
            // and the caller's instance is left untouched if the insert fails.
            TEntity candidate = Detach(entity);
            StampForInsert(candidate, version, now);

            if (!_storage.TryAdd(id, candidate))
            {
                throw new ArgumentException(RS.DuplicateId(id));
            }

            StampForInsert(entity, version, now);
            return Task.FromResult(entity);
        }
        catch (Exception ex)
        {
            return Task.FromException<TEntity>(ex);
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        if (entities == null)
        {
            return Task.FromException<IEnumerable<TEntity>>(new ArgumentNullException(nameof(entities)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
        }

        try
        {
            var addedIds = new List<TKey>();
            var addedEntities = new List<TEntity>();

            // One timestamp for the whole batch; a distinct concurrency token per row.
            DateTimeOffset now = DateTimeOffset.UtcNow;

            foreach (TEntity entity in entities)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    RollBack(addedIds);
                    return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
                }

                if (entity == null)
                {
                    RollBack(addedIds);
                    throw new ArgumentNullException(nameof(entities), RS.NullEntityInBatch());
                }

                TKey id = _idSelector(entity);
                Guid version = Guid.CreateVersion7(now);

                TEntity candidate = Detach(entity);
                StampForInsert(candidate, version, now);

                if (!_storage.TryAdd(id, candidate))
                {
                    RollBack(addedIds);
                    throw new ArgumentException(RS.DuplicateId(id));
                }

                StampForInsert(entity, version, now);
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
    public Task<bool> ContainsKeyAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (id == null)
        {
            return Task.FromException<bool>(new ArgumentNullException(nameof(id)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(cancellationToken);
        }

        bool exists = _storage.TryGetValue(id, out TEntity? entity) && !IsSoftDeleted(entity);
        return Task.FromResult(exists);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (id == null)
        {
            return Task.FromException<bool>(new ArgumentNullException(nameof(id)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(cancellationToken);
        }

        if (!_storage.TryGetValue(id, out TEntity? entity))
        {
            return Task.FromResult(false);
        }

        if (entity is ISoftDeletable soft)
        {
            // Idempotent: an already soft-deleted row is invisible to reads, so re-deleting changes nothing.
            if (soft.IsDeleted)
            {
                return Task.FromResult(false);
            }

            soft.IsDeleted = true;
            soft.DeletedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(true);
        }

        return Task.FromResult(_storage.TryRemove(id, out _));
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            return Task.FromException<bool>(new ArgumentNullException(nameof(entity)));
        }

        try
        {
            return DeleteAsync(_idSelector(entity), cancellationToken);
        }
        catch (Exception ex)
        {
            return Task.FromException<bool>(ex);
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> FindAllByAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return Task.FromException<IEnumerable<TEntity>>(new ArgumentNullException(nameof(predicate)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
        }

        try
        {
            Func<TEntity, bool> compiled = predicate.Compile();
            var matches = new List<TEntity>();

            foreach (TEntity entity in _storage.Values)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
                }

                if (!IsSoftDeleted(entity) && compiled(entity))
                {
                    matches.Add(Detach(entity));
                }
            }

            return Task.FromResult<IEnumerable<TEntity>>(matches);
        }
        catch (Exception ex)
        {
            return Task.FromException<IEnumerable<TEntity>>(ex);
        }
    }

    /// <inheritdoc/>
    public Task<TEntity?> FindByAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return Task.FromException<TEntity?>(new ArgumentNullException(nameof(predicate)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TEntity?>(cancellationToken);
        }

        try
        {
            Func<TEntity, bool> compiled = predicate.Compile();

            foreach (TEntity entity in _storage.Values)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled<TEntity?>(cancellationToken);
                }

                if (!IsSoftDeleted(entity) && compiled(entity))
                {
                    return Task.FromResult<TEntity?>(Detach(entity));
                }
            }

            return Task.FromResult<TEntity?>(null);
        }
        catch (Exception ex)
        {
            return Task.FromException<TEntity?>(ex);
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<IEnumerable<TEntity>>(cancellationToken);
        }

        IEnumerable<TEntity> snapshot = _storage.Values
            .Where(e => !IsSoftDeleted(e))
            .Select(Detach)
            .ToList();

        return Task.FromResult(snapshot);
    }

    /// <inheritdoc/>
    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (id == null)
        {
            return Task.FromException<TEntity?>(new ArgumentNullException(nameof(id)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TEntity?>(cancellationToken);
        }

        if (_storage.TryGetValue(id, out TEntity? entity) && !IsSoftDeleted(entity))
        {
            return Task.FromResult<TEntity?>(Detach(entity));
        }

        return Task.FromResult<TEntity?>(null);
    }

    /// <summary>
    /// No-op for the in-memory store: writes are applied immediately by the other methods.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Always <c>0</c>.</returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        cancellationToken.IsCancellationRequested
            ? Task.FromCanceled<int>(cancellationToken)
            : Task.FromResult(0);

    /// <inheritdoc/>
    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            return Task.FromException<TEntity>(new ArgumentNullException(nameof(entity)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TEntity>(cancellationToken);
        }

        TKey id = _idSelector(entity);
        Guid? baselineToken = (entity as IConcurrentEntity)?.ConcurrencyToken;

        // ConcurrentDictionary.TryUpdate is a compare-and-swap; loop to retry when another writer wins the race.
        while (true)
        {
            if (!_storage.TryGetValue(id, out TEntity? stored))
            {
                return Task.FromException<TEntity>(new KeyNotFoundException(RS.KeyNotFound(id)));
            }

            // The incoming payload must carry the token it was last read with; anything else means a
            // concurrent writer moved the row (either since the caller read it, or since our last retry).
            if (stored is IConcurrentEntity storedToken && storedToken.ConcurrencyToken != baselineToken)
            {
                return Task.FromException<TEntity>(new InvalidOperationException(RS.ConcurrencyViolation(id)));
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            Guid nextToken = Guid.CreateVersion7(now);

            // Build the row to store from a detached copy so the caller's instance is only touched on success.
            TEntity candidate = Detach(entity);
            if (candidate is IConcurrentEntity candidateToken)
            {
                candidateToken.ConcurrencyToken = nextToken;
            }

            if (candidate is IAudit candidateAudit)
            {
                candidateAudit.LastUpdated = now;
            }

            if (_storage.TryUpdate(id, candidate, stored))
            {
                if (entity is IConcurrentEntity entityToken)
                {
                    entityToken.ConcurrencyToken = nextToken;
                }

                if (entity is IAudit entityAudit)
                {
                    entityAudit.LastUpdated = now;
                }

                return Task.FromResult(entity);
            }
        }
    }

    private void RollBack(IEnumerable<TKey> ids)
    {
        foreach (TKey id in ids)
        {
            _storage.TryRemove(id, out _);
        }
    }
}

/// <summary>
/// A thread-safe, in-memory <see cref="IStore{TEntity, TKey}"/> for entities keyed by <see cref="long"/>.
/// </summary>
/// <typeparam name="TEntity">The reference type of the entity being managed.</typeparam>
public class InMemoryStore<TEntity>() : InMemoryStore<TEntity, long>(entity => entity.Id)
    where TEntity : class, IStoredEntity<long>;
