using System.Diagnostics;

namespace Auturge.Stores;

public interface IConcurrentEntity : IEntity
{
    /// <summary> A concurrency token to stop two users from overwriting the same data at the same time. </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    Guid ConcurrencyToken { get; set; }
}
