namespace Auturge.Stores;

/// <summary> The kind of change recorded by a history row. </summary>
public enum ChangeType
{
    /// <summary> The tracked entity was created. </summary>
    Insert,

    /// <summary> The tracked entity was modified. </summary>
    Update,

    /// <summary> The tracked entity was deleted. </summary>
    Delete
}

/// <summary>
/// A change-log row: a single insert/update/delete of another entity, capturing who changed it,
/// when, and the before/after values. History rows are append-only and carry no concurrency or
/// soft-delete state.
/// </summary>
/// <typeparam name="TKey">The history row's own primary-key type.</typeparam>
/// <typeparam name="TEntityKey">The key type of the entity being tracked.</typeparam>
/// <typeparam name="TUser">The user/principal type.</typeparam>
public interface IHistoryEntity<TKey, TEntityKey, TUser> : IStoredEntity<TKey>
    where TKey : notnull
    where TEntityKey : notnull
{
    /// <summary> The ID of the entity that changed. </summary>
    public TEntityKey EntityId { get; set; }

    /// <summary> The type of change. </summary>
    public ChangeType Action { get; set; }

    /// <summary> The user that made the change. </summary>
    public TUser ChangedBy { get; set; }

    /// <summary> When the change was made. </summary>
    public DateTimeOffset TimeStamp { get; set; }

    /// <summary> The table holding the entity that changed. </summary>
    public string TableName { get; set; }

    /// <summary> The prior values, serialized as JSON; <see langword="null"/> for an insert. </summary>
    public string? OldValuesJson { get; set; }

    /// <summary> The new values, serialized as JSON; <see langword="null"/> for a delete. </summary>
    public string? NewValuesJson { get; set; }
}

/// <summary>
/// Base class for a change-log row. Derive to add table-specific members, or use the ready-made
/// <see cref="HistoryEntry"/> via the <c>Create</c> factories.
/// </summary>
/// <typeparam name="TKey">The history row's own primary-key type.</typeparam>
/// <typeparam name="TEntityKey">The key type of the entity being tracked.</typeparam>
/// <typeparam name="TUser">The user/principal type.</typeparam>
public abstract class HistoryEntity<TKey, TEntityKey, TUser>
    : IHistoryEntity<TKey, TEntityKey, TUser>,
        IEquatable<HistoryEntity<TKey, TEntityKey, TUser>>
    where TKey : notnull
    where TEntityKey : notnull
{
    /// <inheritdoc/>
    public TKey Id { get; init; }

    /// <inheritdoc/>
    public TEntityKey EntityId { get; set; }

    /// <inheritdoc/>
    public ChangeType Action { get; set; }

    /// <inheritdoc/>
    public TUser ChangedBy { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset TimeStamp { get; set; }

    /// <inheritdoc/>
    public string TableName { get; set; }

    /// <inheritdoc/>
    public string? OldValuesJson { get; set; }

    /// <inheritdoc/>
    public string? NewValuesJson { get; set; }

    /// <summary> Populates every field of the history row. </summary>
    /// <param name="id">The history row's own primary key.</param>
    /// <param name="tableName">The table holding the entity that changed.</param>
    /// <param name="entityId">The id of the entity that changed.</param>
    /// <param name="action">The kind of change.</param>
    /// <param name="changedBy">The user that made the change.</param>
    /// <param name="timeStamp">When the change was made.</param>
    /// <param name="oldValuesJson">Prior values as JSON, or <see langword="null"/>.</param>
    /// <param name="newValuesJson">New values as JSON, or <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="id"/>, <paramref name="tableName"/>, <paramref name="entityId"/>, or <paramref name="changedBy"/> is <see langword="null"/>.</exception>
    // ReSharper disable ConvertToPrimaryConstructor
    protected HistoryEntity(TKey id, string tableName, TEntityKey entityId, ChangeType action, TUser changedBy,
        DateTimeOffset timeStamp, string? oldValuesJson, string? newValuesJson)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        ChangedBy = changedBy ?? throw new ArgumentNullException(nameof(changedBy));
        Action = action;
        TimeStamp = timeStamp;
        OldValuesJson = oldValuesJson;
        NewValuesJson = newValuesJson;
    }

    /// <summary> Creates a concrete <see cref="HistoryEntry"/> at the given timestamp. </summary>
    public static HistoryEntry Create(TKey id, string tableName, TEntityKey entityId, ChangeType action,
        TUser changedBy, DateTimeOffset timeStamp, string? oldValuesJson, string? newValuesJson) =>
        new(id, tableName, entityId, action, changedBy, timeStamp, oldValuesJson, newValuesJson);

    /// <summary> Creates a concrete <see cref="HistoryEntry"/> stamped at <see cref="DateTimeOffset.UtcNow"/>. </summary>
    public static HistoryEntry Create(TKey id, string tableName, TEntityKey entityId, ChangeType action,
        TUser changedBy, string? oldValuesJson, string? newValuesJson) =>
        Create(id, tableName, entityId, action, changedBy, DateTimeOffset.UtcNow, oldValuesJson, newValuesJson);

    #region Equality

    /// <summary> Equality by <see cref="Id"/> and runtime type. </summary>
    public bool Equals(HistoryEntity<TKey, TEntityKey, TUser>? other)
    {
        if (other is null) return false;
        if (other.GetType() != GetType()) return false;
        return ReferenceEquals(this, other) || Id.Equals(other.Id);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((HistoryEntity<TKey, TEntityKey, TUser>)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary> Equality by <see cref="Id"/>. </summary>
    public static bool operator ==(HistoryEntity<TKey, TEntityKey, TUser>? lhs,
        HistoryEntity<TKey, TEntityKey, TUser>? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    /// <summary> Inequality by <see cref="Id"/>. </summary>
    public static bool operator !=(HistoryEntity<TKey, TEntityKey, TUser>? lhs,
        HistoryEntity<TKey, TEntityKey, TUser>? rhs) => !(lhs == rhs);

    #endregion Equality

    /// <summary> The ready-made concrete history row returned by the <c>Create</c> factories. </summary>
    public sealed class HistoryEntry : HistoryEntity<TKey, TEntityKey, TUser>
    {
        /// <inheritdoc cref="HistoryEntity{TKey,TEntityKey,TUser}(TKey, string, TEntityKey, ChangeType, TUser, DateTimeOffset, string, string)"/>
        public HistoryEntry(TKey id, string tableName, TEntityKey entityId, ChangeType action, TUser changedBy,
            DateTimeOffset timeStamp, string? oldValuesJson, string? newValuesJson)
            : base(id, tableName, entityId, action, changedBy, timeStamp, oldValuesJson, newValuesJson)
        {
        }
    }
}

/// <summary> Base class for a change-log row keyed by <see cref="long"/>, tracking a <see cref="long"/>-keyed entity. </summary>
/// <typeparam name="TUser">The user/principal type.</typeparam>
public abstract class HistoryEntity<TUser> : HistoryEntity<long, long, TUser>
{
    /// <inheritdoc cref="HistoryEntity{TKey,TEntityKey,TUser}(TKey, string, TEntityKey, ChangeType, TUser, DateTimeOffset, string, string)"/>
    protected HistoryEntity(long id, string tableName, long entityId, ChangeType action, TUser changedBy,
        DateTimeOffset timeStamp, string? oldValuesJson, string? newValuesJson)
        : base(id, tableName, entityId, action, changedBy, timeStamp, oldValuesJson, newValuesJson)
    {
    }
}
