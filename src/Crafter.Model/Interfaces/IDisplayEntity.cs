using Auturge.Stores;

namespace Crafter.Model;

public interface IDisplayEntity<out TIdentifier> : IDisplayName, IStoredEntity<TIdentifier>
    where TIdentifier : IEquatable<TIdentifier>;

public interface IDisplayEntity : IDisplayEntity<long>;
