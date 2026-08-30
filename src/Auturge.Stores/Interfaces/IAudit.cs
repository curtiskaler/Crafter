namespace Auturge.Stores;

public interface IAudit
{
    /// <summary> A UTC timestamp recording when the row was first saved. </summary>
    DateTimeOffset Created { get; set; }

    /// <summary> A UTC timestamp recording when the row was last updated. </summary>
    DateTimeOffset LastUpdated { get; set; }
}

public interface IAudit<TUser> : IAudit
{
    /// <summary> The user that created this entity, or <see langword="null"/> when the system created it. </summary>
    TUser? CreatedBy { get; set; }

    /// <summary> The last user to update this entity, or <see langword="null"/> when the system last updated it. </summary>
    TUser? LastUpdatedBy { get; set; }
}
