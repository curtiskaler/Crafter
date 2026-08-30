using Auturge.Identifiers.Internal;

namespace Auturge.Identifiers;

/// <summary>
/// Describes how a flake's 64 bits are split between timestamp, datacenter, machine, and
/// sequence fields, and the epoch the timestamp is measured from.
/// </summary>
public readonly struct FlakeConfig : IEquatable<FlakeConfig>
{
    /// <summary>
    /// The (numeric) output type of the flake.
    /// </summary>
    public Type OutputType { get; }

    /// <summary>
    /// The bit length of the output type of the flake.
    /// </summary>
    public int BitLength { get; }

    /// <summary>
    /// The instant past which a timestamp no longer fits in <see cref="TimestampBits"/>.
    /// At or beyond this point <see cref="FlakeGenerator.GetNextId"/> throws rather than
    /// wrapping. <see cref="DateTime.MaxValue"/> if it falls outside the representable range.
    /// </summary>
    public DateTime RolloverDate { get; }

    /// <summary>
    /// The point the timestamp counts from, as Unix milliseconds. Must be non-negative and
    /// in the past.
    /// </summary>
    public long Epoch { get; }

    /// <summary>
    /// The number of bits occupied by sequence data
    /// </summary>
    public int SequenceBits { get; }

    /// <summary>
    /// The number of bits occupied by the machine identifier 
    /// </summary>
    public int MachineBits { get; }

    /// <summary>
    /// The number of bits occupied by the datacenter identifier 
    /// </summary>
    public int DatacenterBits { get; }


    /// <summary>
    /// The number of bits occupied by the timestamp 
    /// </summary>
    public int TimestampBits { get; }


    /// <summary>
    /// The maximum value for the datacenter identifier
    /// </summary>
    public int MaxDatacenterNum => -1 ^ (-1 << DatacenterBits);

    /// <summary>
    /// The maximum value for the machine identifier
    /// </summary>
    public int MaxMachineNum => -1 ^ (-1 << MachineBits);

    /// <summary>
    ///  The maximum value for the sequence counter.
    /// </summary>
    public int MaxSequence => -1 ^ (-1 << SequenceBits);


    /// <summary>
    /// The offset of the sequence bits.
    /// </summary>
    public int SequenceOffset => 0;

    /// <summary>
    /// The offset of the machine identifier bits.
    /// </summary>
    public int MachineOffset => SequenceBits;

    /// <summary>
    /// The offset of the datacenter identifier bits.
    /// </summary>
    public int DatacenterOffset => SequenceBits + MachineBits;

    /// <summary>
    /// The offset of the timestamp bits.
    /// </summary>
    public int TimestampOffset => DatacenterOffset + DatacenterBits;


    /// <summary>
    /// Creates a flake layout whose epoch is given as a <see cref="DateTime"/> (an
    /// <see cref="DateTimeKind.Unspecified"/> value is read as UTC).
    /// </summary>
    /// <param name="numericType">Must be <c>typeof(long)</c>.</param>
    /// <param name="epoch">The point timestamps count from. Must be in the past.</param>
    /// <param name="sequenceBits">Bits for the per-millisecond counter; at least 1.</param>
    /// <param name="machineBits">Bits for the machine id; may be 0.</param>
    /// <param name="datacenterBits">Bits for the datacenter id; may be 0.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="numericType"/> is not <c>long</c>, or the fields leave fewer than 35
    /// bits (~1 year) for the timestamp.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="epoch"/> is before 1970, or <paramref name="sequenceBits"/> is 0.
    /// </exception>
    public FlakeConfig(Type numericType, DateTime epoch, ushort sequenceBits, ushort machineBits,
        ushort datacenterBits)
        : this(numericType, epoch.GetUnixTimeMillis(), sequenceBits, machineBits, datacenterBits)
    {
    }


    /// <summary>
    /// Creates a flake layout: a signed 64-bit id split into a timestamp offset from
    /// <paramref name="epoch"/>, then datacenter, machine, and sequence fields.
    /// </summary>
    /// <param name="numericType">Must be <c>typeof(long)</c>.</param>
    /// <param name="epoch">
    /// The point timestamps count from, as non-negative Unix milliseconds. Must be in the past.
    /// </param>
    /// <param name="sequenceBits">Bits for the per-millisecond counter; at least 1.</param>
    /// <param name="machineBits">Bits for the machine id; may be 0.</param>
    /// <param name="datacenterBits">Bits for the datacenter id; may be 0.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="numericType"/> is not <c>long</c>, or the fields leave fewer than 35
    /// bits (~1 year) for the timestamp.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="epoch"/> is negative, or <paramref name="sequenceBits"/> is 0.
    /// </exception>
    public FlakeConfig(Type numericType, long epoch, ushort sequenceBits, ushort machineBits, ushort datacenterBits)
    {
        ValidateInputs(numericType, epoch, sequenceBits, machineBits, datacenterBits);

        OutputType = numericType;
        BitLength = TypeSizer.GetBitSize(numericType);
        Epoch = epoch;
        SequenceBits = sequenceBits;
        MachineBits = machineBits;
        DatacenterBits = datacenterBits;
        TimestampBits = BitLength - (SequenceBits + MachineBits + DatacenterBits) - 1;
        RolloverDate = TimeUtils.FindRolloverDateTime(epoch, TimestampBits);
    }

    // 2^35 ms is a little over a year; a config with fewer timestamp bits than this
    // rolls over inside its first year, which is never intentional.
    private const int _minimumTimestampBits = 35;

    private static void ValidateInputs(Type outputType, long epoch, int sequenceBits, int machineBits, int datacenterBits)
    {
        // Flake and FlakeGenerator pack every id into a signed 64-bit long — `long`
        // arithmetic, `1L`/`1U` shift masks, one reserved sign bit. No other width
        // actually round-trips, so reject it here instead of letting a wider or
        // narrower type silently produce truncated ids.
        if (outputType != typeof(long))
        {
            throw new ArgumentException(
                $"Flake ids are packed into a signed 64-bit long; output type '{outputType}' is not supported.",
                nameof(outputType));
        }

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

    public bool Equals(FlakeConfig other)
    {
        return OutputType == other.OutputType
               && BitLength == other.BitLength
               && RolloverDate.Equals(other.RolloverDate)
               && Epoch == other.Epoch
               && SequenceBits == other.SequenceBits
               && MachineBits == other.MachineBits
               && DatacenterBits == other.DatacenterBits
               && TimestampBits == other.TimestampBits;
    }

    public override bool Equals(object? obj)
    {
        return obj is FlakeConfig other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(OutputType, BitLength, RolloverDate, Epoch, SequenceBits, MachineBits, DatacenterBits,
            TimestampBits);
    }

    public static bool operator ==(FlakeConfig x, FlakeConfig y) => x.Equals(y);
    public static bool operator !=(FlakeConfig x, FlakeConfig y) => !(x == y);

    #endregion Equality
}
