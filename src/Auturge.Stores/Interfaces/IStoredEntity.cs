using System.Diagnostics;

namespace Auturge.Stores;


public interface IStoredEntity<out TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    /// <summary> A unique identifier (primary key) used to identify the entity/database row. </summary>
    TIdentifier Id { get; }
    
    /// <summary> A concurrency token to stop two users from overwriting the same data at the same time. </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string ConcurrencyToken { get; set; }
    
    /// <summary> A UTC timestamp recording when the row was first saved. </summary>
    DateTimeOffset Created { get; set; }

    /// <summary> A UTC timestamp recording when the row was last updated. </summary>
    DateTimeOffset LastUpdated { get; set; }
    
    /// <summary> A soft-delete flag to hide records instead of erasing them from the database permanently. </summary>
    bool IsDeleted { get; }
}

public interface IStoredEntity : IStoredEntity<long>;
