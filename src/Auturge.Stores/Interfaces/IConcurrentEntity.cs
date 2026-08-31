using System.Diagnostics;

namespace Auturge.Stores;

/// <summary>
/// An entity that carries an optimistic-concurrency token. The store refreshes the token on every
/// write and rejects an update whose token no longer matches the stored row.
/// </summary>
public interface IConcurrentEntity : IEntity
{
    /// <summary> A concurrency token that stops two writers from overwriting the same row unnoticed. </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid ConcurrencyToken { get; set; }
}
