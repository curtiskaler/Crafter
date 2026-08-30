// ReSharper disable MemberCanBePrivate.Global

using System.Diagnostics;
using System.Globalization;
using Auturge.Identifiers.Instances;

namespace Auturge.Identifiers;

/// <summary>
/// A Snowflake-scheme unique identifier for distributed systems. The identifier itself is the
/// 64-bit <see cref="Value"/>; this struct is a decoded view that also surfaces the timestamp,
/// datacenter, machine, and sequence components packed into it.
/// </summary>
[DebuggerDisplay("{Value} ({ToComponentString()})")]
public readonly struct Flake : IEquatable<Flake>, IComparable<Flake>, IComparable<long>
{
    // A config and the generator that matches it travel together as one immutable object,
    // swapped by a single volatile reference write. Every reader (the decode constructors,
    // NewFlake) takes one snapshot, so a concurrent reassignment of Config can never be
    // seen as a torn FlakeConfig struct or as a generator built for a different layout.
    private sealed class Ambient(FlakeConfig config, long dataCenterId, long machineId)
    {
        public FlakeConfig Config { get; } = config;
        public long DataCenterId { get; } = dataCenterId;
        public long MachineId { get; } = machineId;
        public FlakeGenerator Generator { get; } = new(config, dataCenterId, machineId);
    }

    private static volatile Ambient _ambient = new(FlakeConfigs.Funsies, 0, 0);

    /// <summary>
    /// The layout used to encode and decode flakes when no configuration is passed
    /// explicitly, and the layout used by <see cref="NewFlake"/>. Process-wide.
    /// </summary>
    /// <remarks>
    /// Intended to be set once at start-up (see <see cref="Configure"/> to also set the
    /// datacenter and machine ids). The current source ids are carried over, so assigning a
    /// layout that can't hold them throws. Individual reads and writes are thread-safe, but
    /// flakes produced under one layout cannot be decoded under another.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The new layout has too few machine or datacenter bits for the currently configured
    /// source ids.
    /// </exception>
    public static FlakeConfig Config
    {
        get => _ambient.Config;
        set
        {
            Ambient current = _ambient;
            if (current.Config == value) return;
            _ambient = new Ambient(value, current.DataCenterId, current.MachineId);
        }
    }

    /// <summary>
    /// Sets the layout and source ids used by <see cref="NewFlake"/> and by the
    /// ambient-<see cref="Config"/> decode constructor. Call once at start-up; in a
    /// distributed deployment give each node a distinct datacenter/machine pair so their
    /// ids can't collide within a millisecond.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="dataCenterId"/> or <paramref name="machineId"/> is outside the range
    /// <paramref name="config"/> allows.
    /// </exception>
    public static void Configure(FlakeConfig config, long dataCenterId = 0, long machineId = 0)
        => _ambient = new Ambient(config, dataCenterId, machineId);

    /// <summary> The packed 64-bit identifier. Equality and ordering are defined solely by this. </summary>
    public long Value { get; init; }

    /// <summary> The datacenter component. </summary>
    public long DataCenterId { get; init; }

    /// <summary> The machine component, used to keep concurrent sources from clashing. </summary>
    public long MachineId { get; init; }

    /// <summary> The per-source counter that distinguishes flakes generated in the same millisecond. </summary>
    public long Sequence { get; init; }

    /// <summary> The UTC instant the flake was generated (millisecond precision). </summary>
    public DateTime TimeStamp { get; init; }

    /// <summary> The identifier as a string, identical to <c>Value.ToString()</c>. </summary>
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Renders the decoded parts — datacenter, machine, sequence, and timestamp — for
    /// diagnostics. <see cref="ToString"/> gives the numeric form.
    /// </summary>
    public string ToComponentString()
        => $@"D:{DataCenterId} M:{MachineId} S:{Sequence} T:{TimeStamp:yyyy-MM-ddTHH:mm:ss.fffZ}";

    /// <summary> Unwraps the flake to its packed <see cref="Value"/>. </summary>
    public static implicit operator long(Flake flake) => flake.Value;

    /// <summary>
    /// Encodes a flake from its components using the ambient <see cref="Config"/>.
    /// </summary>
    /// <param name="sequence">Per-source counter; must be in <c>[0, MaxSequence]</c> for the active <see cref="Config"/>.</param>
    /// <param name="timestamp">Unix milliseconds; must be in <c>[Epoch, Epoch + 2^TimestampBits)</c> for the active <see cref="Config"/>.</param>
    /// <param name="dataCenterId">Datacenter id; must be in <c>[0, MaxDatacenterNum]</c> for the active <see cref="Config"/>.</param>
    /// <param name="machineId">Machine id; must be in <c>[0, MaxMachineNum]</c> for the active <see cref="Config"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">A component lies outside the range the configuration can represent.</exception>
    public Flake(long sequence, long timestamp, long dataCenterId, long machineId)
    {
        // Take a single snapshot so a concurrent Config reassignment can't make the range
        // checks below disagree with the field widths used for packing.
        FlakeConfig config = Config;

        if (dataCenterId > config.MaxDatacenterNum || dataCenterId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataCenterId),
                string.Format(CultureInfo.CurrentCulture, RS.Identifiers_DatacenterIdOutOfRange, config.MaxDatacenterNum));
        }

        if (machineId > config.MaxMachineNum || machineId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(machineId),
                string.Format(CultureInfo.CurrentCulture, RS.Identifiers_MachineIdOutOfRange, config.MaxMachineNum));
        }

        if (sequence > config.MaxSequence || sequence < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence),
                string.Format(CultureInfo.CurrentCulture, RS.Identifiers_SequenceOutOfRange, config.MaxSequence));
        }

        // A timestamp outside [epoch, epoch + 2^TimestampBits) would silently overflow
        // into the neighbouring fields (or the sign bit) when packed below.
        long msSinceEpoch = timestamp - config.Epoch;
        long maxMsSinceEpoch = (1L << config.TimestampBits) - 1;
        if (msSinceEpoch < 0 || msSinceEpoch > maxMsSinceEpoch)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), RS.Flake_TimestampOutOfRange);
        }

        DataCenterId = dataCenterId;
        MachineId = machineId;
        Sequence = sequence;

        Value = (msSinceEpoch << config.TimestampOffset)
                | (dataCenterId << config.DatacenterOffset)
                | (machineId << config.MachineOffset)
                | sequence;

        TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
    }

    /// <summary>
    /// Decodes a flake value with the ambient <see cref="Config"/>.
    /// </summary>
    public Flake(long value) : this(value, Config)
    {
    }

    /// <summary>
    /// Decodes a flake value with an explicit configuration instead of the ambient <see cref="Config"/>.
    /// </summary>
    public Flake(long value, FlakeConfig config)
    {
        Value = value;

        // The field widths are validated by FlakeConfig, so each mask keeps the
        // extracted value within [0, max] — no range check is needed here.
        uint sequenceMask = (1U << config.SequenceBits) - 1;
        Sequence = value & sequenceMask;

        uint machineMask = (1U << config.MachineBits) - 1;
        MachineId = (value >> config.MachineOffset) & machineMask;

        uint datacenterMask = (1U << config.DatacenterBits) - 1;
        DataCenterId = (value >> config.DatacenterOffset) & datacenterMask;

        long tsValue = value >> config.TimestampOffset;
        long unixTimestampMillis = tsValue + config.Epoch;
        TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMillis).DateTime;
    }

    /// <summary>
    /// Generates the next identifier as a raw 64-bit value, using the ambient <see cref="Config"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The clock has moved backwards since the previous id, or currently reads outside the
    /// window the ambient <see cref="Config"/> can encode (before its epoch or past its
    /// rollover date).
    /// </exception>
    public static long NewFlake() => _ambient.Generator.GetNextId();

    #region IEquality

    /// <inheritdoc/>
    public bool Equals(Flake other) => Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Flake other && Equals(other);

    /// <inheritdoc/>
    /// <remarks>
    /// Hashes <see cref="Value"/> only, to match <see cref="Equals(Flake)"/>: the component
    /// properties are derived from <see cref="Value"/> and may be left unset when the struct
    /// is built with an object initializer.
    /// </remarks>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary> Compares two flakes by <see cref="Value"/>. </summary>
    public static bool operator ==(Flake x, Flake y) => x.Equals(y);

    /// <summary> Compares two flakes by <see cref="Value"/>. </summary>
    public static bool operator !=(Flake x, Flake y) => !(x == y);

    #endregion Equality

    #region IComparable

    /// <inheritdoc/>
    public int CompareTo(Flake other) => CompareTo(other.Value);

    /// <summary> Orders the flake against a raw <see cref="Value"/>. </summary>
    public int CompareTo(long other) => Value.CompareTo(other);

    #endregion IComparable
}
