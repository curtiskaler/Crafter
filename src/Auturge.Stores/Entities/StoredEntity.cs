using Auturge.Identifiers;

namespace Auturge.Stores;


/// <summary>
/// A typical entity for storage, which does not include audit or soft-delete.
/// </summary>
/// <param name="id"></param>
public abstract class StoredEntity<TKey>(TKey id)
    : IStoredEntity<TKey>, IConcurrentEntity, IEquatable<StoredEntity<TKey>> where TKey : notnull
{
    /// <inheritdoc/>
    public TKey Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));

    /// <inheritdoc/>
    public Guid ConcurrencyToken { get; set; } = Guid.CreateVersion7();

    #region Equality

    public bool Equals(StoredEntity<TKey>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // ID is a unique key, so there must not exist two distinct objects with the same ID.
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StoredEntity<TKey>)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id);

    public static bool operator ==(StoredEntity<TKey>? lhs, StoredEntity<TKey>? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(StoredEntity<TKey>? a, StoredEntity<TKey>? b) => !(a == b);

    #endregion Equality
}

/// <summary>
/// A typical entity for storage. Does not include audit or soft-delete.
/// </summary>
/// <param name="id"></param>
public abstract class StoredEntity(long? id = null) : StoredEntity<long>(id ?? Flake.NewFlake())
    , IEquatable<StoredEntity>
{
    #region Equality

    public virtual bool Equals(StoredEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // ID is a unique key, so there must not exist two distinct objects with the same ID.
        return base.Equals(other);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StoredEntity)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id);

    public static bool operator ==(StoredEntity? lhs, StoredEntity? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(StoredEntity? a, StoredEntity? b) => !(a == b);

    #endregion Equality
}
