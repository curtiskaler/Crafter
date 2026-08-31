namespace Auturge.Stores;

/// <summary>
/// A storage entity with audit, concurrency, and soft-delete. A <see langword="null"/> <paramref name="creator"/>
/// marks a system-created row (seed data, migrations, imports) that is not attributed to a user.
/// </summary>
/// <param name="id">The primary key.</param>
/// <param name="creator">The user creating the entity, or <see langword="null"/> for the system.</param>
public abstract class AuditEntity<TKey, TUser>(TKey id, TUser? creator = default) :
    StoredEntity<TKey>(id),
    IAudit<TUser>, ISoftDeletable<TUser>, IEquatable<AuditEntity<TKey, TUser>>
    where TKey : notnull
{
    /// <inheritdoc/>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public TUser? CreatedBy { get; set; } = creator;

    /// <inheritdoc/>
    public TUser? LastUpdatedBy { get; set; } = creator;

    /// <inheritdoc/>
    public bool IsDeleted { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <inheritdoc/>
    public TUser? DeletedBy { get; set; }

    #region Equality

    /// <summary>
    /// Identity equality: two instances are equal when they represent the same row (same
    /// <see cref="StoredEntity{TKey}.Id"/>). Audit and concurrency fields are deliberately excluded
    /// so an entity still equals its pre-update self.
    /// </summary>
    public bool Equals(AuditEntity<TKey, TUser>? other) => base.Equals(other);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AuditEntity<TKey, TUser>)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary> Equality by <see cref="StoredEntity{TKey}.Id"/>. </summary>
    public static bool operator ==(AuditEntity<TKey, TUser>? lhs, AuditEntity<TKey, TUser>? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    /// <summary> Inequality by <see cref="StoredEntity{TKey}.Id"/>. </summary>
    public static bool operator !=(AuditEntity<TKey, TUser>? a, AuditEntity<TKey, TUser>? b) => !(a == b);

    #endregion Equality
}

/// <summary>
/// A storage entity with audit, concurrency, and soft-delete, keyed by <see cref="long"/>.
/// A <see langword="null"/> <paramref name="creator"/> marks a system-created row.
/// </summary>
/// <param name="id">The primary key.</param>
/// <param name="creator">The user creating the entity, or <see langword="null"/> for the system.</param>
public abstract class AuditEntity<TUser>(long id, TUser? creator = default) : AuditEntity<long, TUser>(id, creator)
    , IEquatable<AuditEntity<TUser>>
{
    /// <inheritdoc cref="AuditEntity{TKey,TUser}.Equals(AuditEntity{TKey,TUser})"/>
    public bool Equals(AuditEntity<TUser>? other) => base.Equals(other);
}
