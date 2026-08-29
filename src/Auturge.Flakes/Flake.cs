// ReSharper disable MemberCanBePrivate.Global

namespace Auturge.Flakes;

/// <summary>
/// A unique identifier.
/// </summary>
public readonly struct Flake : IEquatable<Flake>, IComparable<Flake>, IComparable<long>
{
    private static FlakeConfig _config = FlakeConfigs.Funsies;

    public static FlakeConfig Config
    {
        get => _config;
        set
        {
            if (_config == value) return;
            _config = value;
            Generator = new FlakeGenerator(_config);
        }
    }

    private static FlakeGenerator Generator { get; set; } = new(Config);

    public long Value { get; init; }
    public long DataCenterId { get; init; }
    public long MachineId { get; init; }
    public long Sequence { get; init; }
    public DateTime TimeStamp { get; init; }


    public override string ToString()
    {
        return $@"D:{DataCenterId} M:{MachineId} S:{Sequence} T:{TimeStamp:yyyy-MM-ddTHH:mm:ss.fffZ}";
    }

    public static implicit operator long(Flake flake) => flake.Value;

    public static implicit operator string(Flake flake) => flake.ToString();


    public Flake(long sequence, long timestamp, long dataCenterId, long machineId)
    {
        DataCenterId = dataCenterId;
        MachineId = machineId;
        Sequence = sequence;

        if (dataCenterId > _config.MaxDatacenterNum || dataCenterId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataCenterId),
                $@"datacenterId can't be greater than {_config.MaxDatacenterNum} or less than 0");
        }

        if (machineId > _config.MaxMachineNum || machineId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(machineId),
                $@"machineId can't be greater than {_config.MaxMachineNum} or less than 0");
        }

        var msSinceEpoch = timestamp - _config.Epoch;

        Value = (msSinceEpoch << _config.TimestampOffset)
                | (dataCenterId << _config.DatacenterOffset)
                | (machineId << _config.MachineOffset)
                | sequence;

        TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
    }

    public Flake(long value)
    {
        Value = value;

        var sequenceMask = (1U << _config.SequenceBits) - 1;
        Sequence = (value >> 0) & sequenceMask;

        var machineMask = (1U << _config.MachineBits) - 1;
        MachineId = (value >> _config.MachineOffset) & machineMask;
        if (MachineId > _config.MaxMachineNum || MachineId < 0)
        {
            throw new ArgumentException($@"machineId can't be greater than {_config.MaxMachineNum} or less than 0");
        }

        var datacenterMask = (1U << _config.DatacenterBits) - 1;
        DataCenterId = (value >> _config.DatacenterOffset) & datacenterMask;
        if (DataCenterId > _config.MaxDatacenterNum || DataCenterId < 0)
        {
            throw new ArgumentException(
                $@"datacenterId can't be greater than {_config.MaxDatacenterNum} or less than 0");
        }

        var tsValue = value >> _config.TimestampOffset;
        var unixTimestampSeconds = tsValue + _config.Epoch;
        TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampSeconds).DateTime;
    }

    /// <summary>
    /// Generates a new flake.
    /// </summary>
    public static long NewFlake() => Generator.GetNextId();

    #region IEquality

    public bool Equals(Flake other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Flake other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Value, DataCenterId, MachineId, Sequence, TimeStamp);

    public static bool operator ==(Flake x, Flake y) => x.Equals(y);
    public static bool operator !=(Flake x, Flake y) => !(x == y);

    #endregion Equality

    #region IComparable

    public int CompareTo(Flake other) => CompareTo(other.Value);
    public int CompareTo(long other) => Value.CompareTo(other);

    #endregion IComparable
}
