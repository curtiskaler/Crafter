using System.Linq.Expressions;
using Auturge.Stores.Stores;

namespace Auturge.Stores;

public abstract class Store<TEntity> : Store<long, TEntity>
    where TEntity : class, IStoredEntity<long>
{
    protected Store(IStore<long, TEntity> store) : base(store)
    {
    }

    protected Store() : base(new InMemoryStore<TEntity>())
    {
    }
}

public abstract class Store<TId, TEntity> : IStore<TId, TEntity>
    where TEntity : class, IStoredEntity<TId>
    where TId : IEquatable<TId>
{
    private readonly IStore<TId, TEntity> _store;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected Store(IStore<TId, TEntity> store)
    {
        _store = store;
    }

    public Task<TEntity> Add(TEntity entity, CancellationToken cancellationToken = default) =>
        _store.Add(entity, cancellationToken);

    public Task<IEnumerable<TEntity>> AddRange(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        => _store.AddRange(entities, cancellationToken);

    public Task<bool> ContainsKey(TId id, CancellationToken cancellationToken = default)
        => _store.ContainsKey(id, cancellationToken);

    public Task<IEnumerable<TEntity>> FindAllBy(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => _store.FindAllBy(predicate, cancellationToken);

    public Task<TEntity?> FindBy(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => _store.FindBy(predicate, cancellationToken);

    public Task<TEntity?> GetById(TId id, CancellationToken cancellationToken = default)
        => _store.GetById(id, cancellationToken);

    public Task<IEnumerable<TEntity>> GetAll(CancellationToken cancellationToken = default)
        => _store.GetAll(cancellationToken);

    public Task<TEntity> Update(TEntity entity, CancellationToken cancellationToken = default)
        => _store.Update(entity, cancellationToken);

    public Task<bool> Delete(TId id, CancellationToken cancellationToken = default)
        => _store.Delete(id, cancellationToken);

    public Task<bool> Delete(TEntity entity, CancellationToken cancellationToken = default)
        => _store.Delete(entity, cancellationToken);

    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
        => _store.SaveChanges(cancellationToken);
}
