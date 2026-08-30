using Auturge.Identifiers.Internal;

namespace Auturge.Identifiers;

/// <summary>
/// Describes how a flake's signed 64 bits are split between timestamp, datacenter, machine,
/// and sequence fields, and the epoch the timestamp is measured from.
/// </summary>
public readonly struct FlakeConfig : IEquatable<FlakeConfig>
{
    /// <summary>
    /// The instant past which a timestamp no longer fits in <see cref="TimestampBits"/>. At or
    /// beyond this point <see cref="FlakeGenerator.GetNextId"/> throws rather than wrapping.
    /// <see cref="DateTime.MaxValue"/> if it falls outside the representable range.
    /// </summary>
    public DateTime RolloverDate { get; }

    /// <summary>
    /// The point the timestamp counts from, as Unix milliseconds. Must be non-negative and
    /// in the past.
    /// </summary>
    public long Epoch { get; }

    /// <summary> Bits reserved for the per-millisecond sequence counter. </summary>
    public int SequenceBits { get; }

    /// <summary> Bits reserved for the machine id. </summary>
    public int MachineBits { get; }

    /// <summary> Bits reserved for the datacenter id. </summary>
    public int DatacenterBits { get; }

    /// <summary> Bits left for the timestamp offset, i.e. <c>63 - SequenceBits - MachineBits - DatacenterBits</c>. </summary>
    public int TimestampBits { get; }


    /// <summary> Largest datacenter id this layout can hold. </summary>
    public int MaxDatacenterNum => -1 ^ (-1 << DatacenterBits);

    /// <summary> Largest machine id this layout can hold. </summary>
    public int MaxMachineNum => -1 ^ (-1 << MachineBits);

    /// <summary> Largest sequence value before it wraps within a millisecond. </summary>
    public int MaxSequence => -1 ^ (-1 << SequenceBits);


    /// <summary> Bit position of the sequence field (least significant). Always 0. </summary>
    public int SequenceOffset => 0;

    /// <summary> Bit position of the machine field. </summary>
    public int MachineOffset => SequenceBits;

    /// <summary> Bit position of the datacenter field. </summary>
    public int DatacenterOffset => SequenceBits + MachineBits;

    /// <summary> Bit position of the timestamp field (most significant, below the sign bit). </summary>
    public int TimestampOffset => DatacenterOffset + DatacenterBits;


    /// <summary>
    /// Creates a flake layout whose epoch is given as a <see cref="DateTime"/> (an
    /// <see cref="DateTimeKind.Unspecified"/> value is read as UTC).
    /// </summary>
    /// <param name="epoch">The point timestamps count from. Must be in the past.</param>
    /// <param name="sequenceBits">Bits for the per-millisecond counter; at least 1.</param>
    /// <param name="machineBits">Bits for the machine id; may be 0.</param>
    /// <param name="datacenterBits">Bits for the datacenter id; may be 0.</param>
    /// <exception cref="ArgumentException">
    /// The fields leave fewer than 35 bits (~1 year) for the timestamp.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="epoch"/> is before 1970, or <paramref name="sequenceBits"/> is 0.
    /// </exception>
    public FlakeConfig(DateTime epoch, ushort sequenceBits, ushort machineBits, ushort datacenterBits)
        : this(epoch.GetUnixTimeMillis(), sequenceBits, machineBits, datacenterBits)
    {
    }


    /// <summary>
    /// Creates a flake layout: a signed 64-bit id split into a timestamp offset from
    /// <paramref name="epoch"/>, then datacenter, machine, and sequence fields.
    /// </summary>
    /// <param name="epoch">
    /// The point timestamps count from, as non-negative Unix milliseconds. Must be in the past.
    /// </param>
    /// <param name="sequenceBits">Bits for the per-millisecond counter; at least 1.</param>
    /// <param name="machineBits">Bits for the machine id; may be 0.</param>
    /// <param name="datacenterBits">Bits for the datacenter id; may be 0.</param>
    /// <exception cref="ArgumentException">
    /// The fields leave fewer than 35 bits (~1 year) for the timestamp.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="epoch"/> is negative, or <paramref name="sequenceBits"/> is 0.
    /// </exception>
    public FlakeConfig(long epoch, ushort sequenceBits, ushort machineBits, ushort datacenterBits)
    {
        ValidateInputs(epoch, sequenceBits, machineBits, datacenterBits);

        Epoch = epoch;
        SequenceBits = sequenceBits;
        MachineBits = machineBits;
        DatacenterBits = datacenterBits;
        TimestampBits = 64 - (SequenceBits + MachineBits + DatacenterBits) - 1;
        RolloverDate = TimeUtils.FindRolloverDateTime(epoch, TimestampBits);
    }

    // 2^35 ms is a little over a year; a config with fewer timestamp bits than this
    // rolls over inside its first year, which is never intentional.
    private const int _minimumTimestampBits = 35;

    private static void ValidateInputs(long epoch, int sequenceBits, int machineBits, int datacenterBits)
    {
        if (epoch < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epoch), epoch,
                "epoch must be a non-negative Unix-millisecond value.");
        }

        // The generator needs at least one sequence bit to hand out more than one id
        // per millisecond; with zero it hits the sequence-exhausted spin on every
        // second call within a millisecond.
        if (sequenceBits < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceBits), sequenceBits,
                "at least one sequence bit is required.");
        }

        const int signBit = 1;
        int timestampBits = 64 - (sequenceBits + machineBits + datacenterBits) - signBit;
        if (timestampBits < _minimumTimestampBits)
        {
            throw new ArgumentException(
                $"sequence ({sequenceBits}) + machine ({machineBits}) + datacenter ({datacenterBits}) bits leave "
                + $"only {timestampBits} for the timestamp; at least {_minimumTimestampBits} are required.",
                nameof(sequenceBits));
        }
    }

    #region Equality

    // RolloverDate and TimestampBits are derived from Epoch + the three bit widths, so those
    // four fields are the whole identity.
    /// <inheritdoc/>
    public bool Equals(FlakeConfig other)
        => Epoch == other.Epoch
           && SequenceBits == other.SequenceBits
           && MachineBits == other.MachineBits
           && DatacenterBits == other.DatacenterBits;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is FlakeConfig other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(Epoch, SequenceBits, MachineBits, DatacenterBits);

    /// <summary>Compares two layouts by epoch and field widths.</summary>
    public static bool operator ==(FlakeConfig x, FlakeConfig y) => x.Equals(y);

    /// <summary>Compares two layouts by epoch and field widths.</summary>
    public static bool operator !=(FlakeConfig x, FlakeConfig y) => !(x == y);

    #endregion Equality
}
