namespace Auturge.Stores.Tests.TestObjects;

public class User : StoredEntity, IEquatable<User>
{
    public string UserName { get; set; }
    public string GivenName { get; set; } = "";
    public string SurName { get; set; } = "";

    public User(string userName) : this(null, userName)
    {
    }

    public User(long? id, string userName) : base(id)
    {
        UserName = userName;
    }

    public bool Equals(User? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && GivenName == other.GivenName && SurName == other.SurName;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((User)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id);
}
