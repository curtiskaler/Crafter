using Auturge.Flakes.Internal;

namespace Auturge.Flakes;

/// <summary>
/// The configuration of the flake.
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
    /// The date and time at which this flake configuration rolls over to zero. 
    /// </summary>
    public DateTime RolloverDate { get; }

    /// <summary>
    /// The starting timestamp.
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


    public FlakeConfig(Type numericType, DateTime epoch, ushort sequenceBits, ushort machineBits,
        ushort datacenterBits)
        : this(numericType, epoch.GetUnixTimeMillis(), sequenceBits, machineBits, datacenterBits)
    {
    }


    public FlakeConfig(Type numericType, long epoch, ushort sequenceBits, ushort machineBits, ushort datacenterBits)
    {
        OutputType = numericType;
        BitLength = TypeSizer.GetBitSize(OutputType);

        ValidateInputs(BitLength, sequenceBits, machineBits, datacenterBits);

        Epoch = epoch;
        SequenceBits = sequenceBits;
        MachineBits = machineBits;
        DatacenterBits = datacenterBits;
        TimestampBits = BitLength - (SequenceBits + MachineBits + DatacenterBits) - 1;
        RolloverDate = TimeUtils.FindRolloverDateTime(epoch, TimestampBits);
    }

    private static void ValidateInputs(int bitLength, ushort sequenceBits, ushort machineBits, ushort datacenterBits)
    {
        ValidateInput("sequence bits", sequenceBits);
        ValidateInput("machine bits", machineBits);
        ValidateInput("datacenter bits", datacenterBits);

        // There are 35 bits worth of ms in a year (even a leap-year).
        const int bitsInYear = 35;

        // The config must be good for at least 1 year.
        ValidateInput("bits", (sequenceBits + machineBits + datacenterBits + bitsInYear + 1));
        return;

        void ValidateInput(string purpose, int value)
        {
            if (value >= bitLength - 1)
            {
                throw new ArgumentException(@$"The total number of {purpose} must be fewer than {bitLength}-1.");
            }
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
