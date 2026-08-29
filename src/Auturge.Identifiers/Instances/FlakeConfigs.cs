namespace Auturge.Identifiers.Instances;

/// <summary>
/// A set of flake configurations.
/// </summary>
public struct FlakeConfigs
{
    private static readonly DateTime _epoch2025 = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The funsies flake config.
    ///     A fast single-source id generator.
    ///     The generator is unlikely to create more than 250k ID's in one millisecond, so
    ///     20 bits (1048576) should suffice, except on the most ridiculous processors. 
    ///     A 64-bit integer, with one generator source and
    ///         20-bits of sequence (1,048,576 possible id's per millisecond), leaving
    ///         43 bits (+1 bit to ensure positive values) for timestamp (approx 279 years) starting at the first millisecond of 2025.
    /// </summary>
    public static readonly FlakeConfig Funsies = new(typeof(long), _epoch2025, 20, 0, 0);

    /// <summary>
    /// The Twitter snowflake config.
    ///     A 64-bit integer, with
    ///         41-bit timestamp (+1 bit to ensure positive) starting at the twitter epoch (Nov 04 2010 01:42:54), 
    ///         and 12 sequence bits (4096 possible id's per millisecond),
    ///         and 5 machine bits (1024 possible machine sources),
    ///         and 5 datacenter bits (1024 possible datacenter sources).
    /// </summary>
    public static readonly FlakeConfig Twitter = new(typeof(long), 1288834974657L, 12, 5, 5);

    /// <summary>
    /// The Twitter snowflake config.
    ///     A 64-bit integer, with
    ///         41-bit timestamp (+1 bit to ensure positive) starting at the twitter epoch (Nov 04 2010 01:42:54), 
    ///         and 12 sequence bits (4096 possible id's per millisecond),
    ///         and 5 machine bits (1024 possible machine sources),
    ///         and 5 datacenter bits (1024 possible datacenter sources).
    /// </summary>
    public static FlakeConfig SnowFlake => Twitter;
}
