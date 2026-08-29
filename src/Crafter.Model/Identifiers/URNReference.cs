using Auturge.Identifiers;

namespace Crafter.Model.Identifiers;

public class URNReference : Reference<URN>, IEquatable<URNReference>
{
    public URN URN => Resource;

    public URNReference(string displayName, string entityType, string id) : base(displayName,
        new URN(entityType, id))
    {
    }

    public URNReference(string displayName, string urnString) : this(displayName, new URN(urnString))
    {
    }

    public URNReference(string displayName, URN original) : base(displayName, original)
    {
    }

    public URNReference(URNReference reference) : base(reference)
    {
    }

    #region Equality

    public bool Equals(URNReference? other) => base.Equals(other);

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((URNReference)obj);
    }

    public override int GetHashCode() => base.GetHashCode();
    
    public static bool operator ==(URNReference? lhs, URNReference? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(URNReference? lhs, URNReference? rhs) => !(lhs == rhs);

    #endregion Equality
}
