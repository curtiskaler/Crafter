namespace Auturge.Stores;

/// <summary>
/// An entity that records when it was created and last updated. The store stamps both timestamps.
/// Typically inappropriate for lookup tables.
/// </summary>
public interface IAudit
{
    /// <summary> A UTC timestamp recording when the row was first saved. </summary>
    DateTimeOffset Created { get; set; }

    /// <summary> A UTC timestamp recording when the row was last updated. </summary>
    DateTimeOffset LastUpdated { get; set; }
}

/// <summary> An <see cref="IAudit"/> entity that also records which user made each change. </summary>
/// <typeparam name="TUser">The user/principal type.</typeparam>
public interface IAudit<TUser> : IAudit
{
    /// <summary> The user that created this entity, or <see langword="null"/> when the system created it. </summary>
    TUser? CreatedBy { get; set; }

    /// <summary> The last user to update this entity, or <see langword="null"/> when the system last updated it. </summary>
    TUser? LastUpdatedBy { get; set; }
}
