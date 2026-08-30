namespace Auturge.Stores;

public enum ChangeType
{
    Insert,
    Update,
    Delete
}

/// <summary> Entity for history storage. </summary>
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

    /// <summary> The old value(s). </summary>
    public string? OldValuesJson { get; set; }

    /// <summary> The new value(s). </summary>
    public string? NewValuesJson { get; set; }
}

/// <summary>
/// A typical entity for history storage.
/// </summary>
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

    public static HistoryEntry Create(TKey id, string tableName, TEntityKey entityId, ChangeType action,
        TUser changedBy, DateTimeOffset timeStamp, string? oldValuesJson, string? newValuesJson) =>
        new(id, tableName, entityId, action, changedBy, timeStamp, oldValuesJson, newValuesJson);

    public static HistoryEntry Create(TKey id, string tableName, TEntityKey entityId, ChangeType action,
        TUser changedBy, string? oldValuesJson, string? newValuesJson) =>
        Create(id, tableName, entityId, action, changedBy, DateTimeOffset.UtcNow, oldValuesJson, newValuesJson);

    #region Equality

    public bool Equals(HistoryEntity<TKey, TEntityKey, TUser>? other)
    {
        if (other is null) return false;
        if (other.GetType() != GetType()) return false;
        return ReferenceEquals(this, other) || Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((HistoryEntity<TKey, TEntityKey, TUser>)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id);

    public static bool operator ==(HistoryEntity<TKey, TEntityKey, TUser>? lhs,
        HistoryEntity<TKey, TEntityKey, TUser>? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(HistoryEntity<TKey, TEntityKey, TUser>? lhs,
        HistoryEntity<TKey, TEntityKey, TUser>? rhs) => !(lhs == rhs);

    #endregion Equality

    public sealed class HistoryEntry : HistoryEntity<TKey, TEntityKey, TUser>
    {
        public HistoryEntry(TKey id, string tableName, TEntityKey entityId, ChangeType action, TUser changedBy,
            DateTimeOffset timeStamp, string? oldValuesJson, string? newValuesJson)
            : base(id, tableName, entityId, action, changedBy, timeStamp, oldValuesJson, newValuesJson)
        {
        }
    }
}

/// <summary>
/// A typical entity for history storage.
/// </summary>
public abstract class HistoryEntity<TUser> : HistoryEntity<long, long, TUser>
{
    protected HistoryEntity(long id, string tableName, long entityId, ChangeType action, TUser changedBy,
        DateTimeOffset timeStamp, string? oldValuesJson, string? newValuesJson)
        : base(id, tableName, entityId, action, changedBy, timeStamp, oldValuesJson, newValuesJson)
    {
    }
}
