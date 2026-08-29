using Auturge.Identifiers;

namespace Auturge.Stores;

public abstract class StoredEntity<TId> : IStoredEntity<TId> where TId : IEquatable<TId>
{
    protected StoredEntity(TId id) : this(id, null)
    {
    }

    private StoredEntity(TId id, DateTimeOffset? createdDate = null)
    {
        Id = id;
        DateTimeOffset now = createdDate ?? DateTimeOffset.UtcNow;
        Created = now;
        LastUpdated = now;
        ConcurrencyToken = now.Ticks.ToString();
    }

    /// <inheritdoc/>
    public TId Id { get; }

    /// <inheritdoc/>
    public string ConcurrencyToken { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset Created { get; set; } 

    /// <inheritdoc/>
    public DateTimeOffset LastUpdated { get; set; }

    /// <inheritdoc/>
    public bool IsDeleted { get; internal set; } = false;
}

public abstract class StoredEntity(long? id = null) : StoredEntity<long>(id ?? Flake.NewFlake());
