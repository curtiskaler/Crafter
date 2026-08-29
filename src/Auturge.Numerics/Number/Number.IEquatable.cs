using System.Numerics;

namespace Auturge.Numerics;

public partial struct Number: IEquatable<Number>
{
    public bool Equals(Number other) => this == other; // IEquatable<Number>

    public override bool Equals(object? obj) => obj is Number other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(IsNegative, RawValue, DecimalOffset);
}
