using Auturge.Identifiers;

namespace Auturge.Stores.Tests.TestObjects;

/// <summary> Marks a built-in principal that is owned by the system rather than created by a user. </summary>
internal interface ISystemObject;

public class User : AuditEntity<long, User>, IEquatable<User>
{
    public const long SYSTEM_ID = 0;
    public const long ADMIN_ID = 1;

    /// <summary> The system principal itself. Has no creator. </summary>
    public static readonly User SYSTEM = new SystemUser(SYSTEM_ID, "SYSTEM");

    /// <summary> The built-in administrator. Created by <see cref="SYSTEM"/>. </summary>
    public static readonly User ADMIN = new SystemUser(ADMIN_ID, "ADMIN");

    public string UserName { get; set; } = "";
    public string GivenName { get; set; } = "";
    public string SurName { get; set; } = "";

    public User(string userName) : this(null, userName)
    {
    }

    public User(long? id, string userName) : this(id, userName, ADMIN)
    {
    }

    public User(long? id, string userName, User? creator) : base(id ?? Flake.NewFlake(), creator)
    {
        UserName = userName;
    }

    /// <summary> True for the built-in <see cref="SYSTEM"/> and <see cref="ADMIN"/> principals. </summary>
    public bool IsSystemObject => this is ISystemObject;

    public bool Equals(User? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && GivenName == other.GivenName && SurName == other.SurName;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((User)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary> A built-in principal (<see cref="SYSTEM"/> or <see cref="ADMIN"/>). </summary>
    public sealed class SystemUser : User, ISystemObject
    {
        // SYSTEM is unattributed; every other built-in principal is created by SYSTEM.
        internal SystemUser(long id, string userName) : base(id, userName, id == SYSTEM_ID ? null : SYSTEM)
        {
        }
    }
}
