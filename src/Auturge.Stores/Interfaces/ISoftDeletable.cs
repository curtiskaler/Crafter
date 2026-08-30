namespace Auturge.Stores;

public interface ISoftDeletable
{
    /// <summary> A soft-delete flag to hide records instead of erasing them from the database permanently. </summary>
    bool IsDeleted { get; set; }

    /// <summary> When the entity was deleted. </summary>
    DateTimeOffset? DeletedAt { get; set; }
}

public interface ISoftDeletable<TUser> : ISoftDeletable
{
    /// <summary> The user who deleted this entity. </summary>
    TUser? DeletedBy { get; set; }
}
