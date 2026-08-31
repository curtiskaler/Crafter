using System.Linq.Expressions;

namespace Auturge.Stores;

/// <summary> Base repository that delegates every operation to an injected <see cref="IStore{TEntity}"/> backend. </summary>
/// <typeparam name="TEntity">The reference type of the entity managed by the store.</typeparam>
public abstract class Store<TEntity> : IStore<TEntity>
    where TEntity : class, IStoredEntity
{
    private readonly IStore<TEntity> _backend;

    /// <summary> Creates a repository over the given backend. </summary>
    /// <param name="backend">The store implementation every operation delegates to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="backend"/> is <see langword="null"/>.</exception>
    protected Store(IStore<TEntity> backend)
    {
        ArgumentNullException.ThrowIfNull(backend);
        _backend = backend;
    }

    /// <inheritdoc/>
    public IQueryable<TEntity> Query() => _backend.Query();

    /// <inheritdoc/>
    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _backend.AddAsync(entity, cancellationToken);

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => _backend.AddRangeAsync(entities, cancellationToken);

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _backend.DeleteAsync(entity, cancellationToken);

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> FindAllByAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => _backend.FindAllByAsync(predicate, cancellationToken);

    /// <inheritdoc/>
    public Task<TEntity?> FindByAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => _backend.FindByAsync(predicate, cancellationToken);

    /// <inheritdoc/>
    public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => _backend.GetAllAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _backend.SaveChangesAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        => _backend.UpdateAsync(entity, cancellationToken);
}

/// <summary> Base repository for single-key entities, delegating to an injected <see cref="IStore{TEntity, TKey}"/> backend. </summary>
/// <typeparam name="TEntity">The reference type of the entity managed by the store.</typeparam>
/// <typeparam name="TKey">The non-nullable type of the primary key.</typeparam>
public abstract class Store<TEntity, TKey> : Store<TEntity>, IStore<TEntity, TKey>
    where TEntity : class, IStoredEntity<TKey>
    where TKey : notnull
{
    private readonly IStore<TEntity, TKey> _backend;

    /// <summary> Creates a repository over the given backend. </summary>
    /// <param name="backend">The store implementation every operation delegates to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="backend"/> is <see langword="null"/>.</exception>
    protected Store(IStore<TEntity, TKey> backend) : base(backend)
    {
        _backend = backend;
    }

    /// <inheritdoc/>
    public Task<bool> ContainsKeyAsync(TKey id, CancellationToken cancellationToken = default)
        => _backend.ContainsKeyAsync(id, cancellationToken);

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        => _backend.DeleteAsync(id, cancellationToken);

    /// <inheritdoc/>
    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        => _backend.GetByIdAsync(id, cancellationToken);
}
