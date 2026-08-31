using Auturge.Identifiers;

namespace Auturge.Stores;


/// <summary>
/// Base class for a stored entity: a primary key plus an optimistic-concurrency token.
/// No audit or soft-delete &#8212; use <see cref="AuditEntity{TKey,TUser}"/> for those.
/// </summary>
/// <param name="id">The primary key. Must not be <see langword="null"/>.</param>
/// <typeparam name="TKey">The non-nullable primary-key type.</typeparam>
public abstract class StoredEntity<TKey>(TKey id)
    : IStoredEntity<TKey>, IConcurrentEntity, IEquatable<StoredEntity<TKey>> where TKey : notnull
{
    /// <inheritdoc/>
    public TKey Id { get; init; } = id ?? throw new ArgumentNullException(nameof(id));

    /// <inheritdoc/>
    public Guid ConcurrencyToken { get; set; } = Guid.CreateVersion7();

    #region Equality

    /// <inheritdoc/>
    public bool Equals(StoredEntity<TKey>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // ID is a unique key, so there must not exist two distinct objects with the same ID.
        return Id.Equals(other.Id);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StoredEntity<TKey>)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary> Equality by <see cref="Id"/>. </summary>
    public static bool operator ==(StoredEntity<TKey>? lhs, StoredEntity<TKey>? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    /// <summary> Inequality by <see cref="Id"/>. </summary>
    public static bool operator !=(StoredEntity<TKey>? a, StoredEntity<TKey>? b) => !(a == b);

    #endregion Equality
}

/// <summary>
/// Base class for a stored entity keyed by <see cref="long"/>. When no id is supplied a new
/// <see cref="Flake"/> is minted. No audit or soft-delete &#8212; use <see cref="AuditEntity{TUser}"/> for those.
/// </summary>
/// <param name="id">The primary key, or <see langword="null"/> to mint a new <see cref="Flake"/>.</param>
public abstract class StoredEntity(long? id = null) : StoredEntity<long>(id ?? Flake.NewFlake())
    , IEquatable<StoredEntity>
{
    #region Equality

    /// <inheritdoc cref="StoredEntity{TKey}.Equals(StoredEntity{TKey})"/>
    public virtual bool Equals(StoredEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // ID is a unique key, so there must not exist two distinct objects with the same ID.
        return base.Equals(other);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StoredEntity)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary> Equality by <see cref="StoredEntity{TKey}.Id"/>. </summary>
    public static bool operator ==(StoredEntity? lhs, StoredEntity? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    /// <summary> Inequality by <see cref="StoredEntity{TKey}.Id"/>. </summary>
    public static bool operator !=(StoredEntity? a, StoredEntity? b) => !(a == b);

    #endregion Equality
}
