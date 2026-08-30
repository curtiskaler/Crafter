namespace Auturge.Identifiers.Instances;

/// <summary>
/// Ready-made <see cref="FlakeConfig"/> layouts.
/// </summary>
public static class FlakeConfigs
{
    private static readonly DateTime _epoch2025 = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Single-source layout: 20 sequence bits (~1,048,576 ids per millisecond), no machine
    /// or datacenter bits, leaving 43 timestamp bits (~279 years) from the start of 2025.
    /// </summary>
    public static readonly FlakeConfig Funsies = new(typeof(long), _epoch2025, 20, 0, 0);

    /// <summary>
    /// The Twitter snowflake layout: 41 timestamp bits from the Twitter epoch
    /// (2010-11-04 01:42:54 UTC), 12 sequence bits (4,096 ids per millisecond), 5 machine
    /// bits, 5 datacenter bits.
    /// </summary>
    public static readonly FlakeConfig Twitter = new(typeof(long), 1288834974657L, 12, 5, 5);

    /// <summary>
    /// <see cref="Twitter"/>, under its more familiar name.
    /// </summary>
    public static FlakeConfig SnowFlake => Twitter;
}
