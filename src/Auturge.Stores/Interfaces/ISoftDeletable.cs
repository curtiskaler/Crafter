namespace Auturge.Stores;

/// <summary>
/// An entity that is hidden on delete rather than removed. The store's read paths skip
/// soft-deleted rows; a delete flags the row instead of erasing it. Inappropriate for history tables.
/// </summary>
public interface ISoftDeletable
{
    /// <summary> Whether the row is soft-deleted and therefore hidden from queries. </summary>
    bool IsDeleted { get; set; }

    /// <summary> When the entity was deleted, or <see langword="null"/> if it is live. </summary>
    DateTimeOffset? DeletedAt { get; set; }
}

/// <summary> An <see cref="ISoftDeletable"/> entity that also records which user deleted it. </summary>
/// <typeparam name="TUser">The user/principal type.</typeparam>
public interface ISoftDeletable<TUser> : ISoftDeletable
{
    /// <summary> The user who deleted this entity, or <see langword="null"/> when the system did (or it is live). </summary>
    TUser? DeletedBy { get; set; }
}
