using System.Globalization;
using Auturge.Identifiers.Instances;

namespace Auturge.Identifiers;

/// <summary>
/// Produces monotonically increasing snowflake ids for one datacenter/machine pair.
/// A single instance is thread-safe; use one per source.
/// </summary>
public sealed class FlakeGenerator
{
    // Every id is packed into a signed 64-bit long, so only long-based configs are
    // supported; FlakeConfig now rejects any other output type.

    private readonly long _dataCenterId;
    private readonly long _machineId;

    private readonly FlakeConfig _config;
    private readonly TimeProvider _time;
    private readonly long _maxMsSinceEpoch;
    private long _sequence;
    private long _lastStamp = -1L;

    private readonly Lock _lockObj = new();

    /// <summary>
    /// Creates a single-source generator using <see cref="FlakeConfigs.SnowFlake"/>.
    /// </summary>
    public FlakeGenerator() : this(FlakeConfigs.SnowFlake)
    {
    }

    /// <summary>
    /// Creates a generator for the given layout and source ids.
    /// </summary>
    /// <param name="config">Bit layout to use; <see langword="null"/> selects <see cref="FlakeConfigs.Twitter"/>.</param>
    /// <param name="datacenterId">Datacenter id, in <c>[0, config.MaxDatacenterNum]</c>.</param>
    /// <param name="machineId">Machine id, in <c>[0, config.MaxMachineNum]</c>.</param>
    /// <param name="timeProvider">
    /// Clock used for id timestamps; defaults to <see cref="TimeProvider.System"/>. Exposed
    /// primarily so tests can control the clock.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="datacenterId"/> or <paramref name="machineId"/> is outside the range the
    /// layout allows.
    /// </exception>
    public FlakeGenerator(FlakeConfig? config, long datacenterId = 0, long machineId = 0,
        TimeProvider? timeProvider = null)
    {
        _config = config ?? FlakeConfigs.Twitter;
        _time = timeProvider ?? TimeProvider.System;
        _maxMsSinceEpoch = (1L << _config.TimestampBits) - 1;

        if (datacenterId > _config.MaxDatacenterNum || datacenterId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(datacenterId),
                string.Format(CultureInfo.CurrentCulture, RS.Identifiers_DatacenterIdOutOfRange, _config.MaxDatacenterNum));
        }

        if (machineId > _config.MaxMachineNum || machineId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(machineId),
                string.Format(CultureInfo.CurrentCulture, RS.Identifiers_MachineIdOutOfRange, _config.MaxMachineNum));
        }

        _dataCenterId = datacenterId;
        _machineId = machineId;
    }

    /// <summary>
    /// Generates the next id and returns it as a decoded <see cref="Flake"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">See <see cref="GetNextId"/>.</exception>
    public Flake NewFlake()
    {
        long value = GetNextId();
        return new Flake(value, _config);
    }

    /// <summary>
    /// Generates the next id.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The clock has moved backwards since the previous id was issued, or it currently reads
    /// outside the window this configuration can encode (before the epoch, or past the
    /// rollover date). Either way uniqueness / correctness can no longer be guaranteed;
    /// callers expecting transient clock corrections may catch this and retry.
    /// </exception>
    public long GetNextId()
    {
        lock (_lockObj)
        {
            while (true)
            {
                long timestamp = CurrentMillis();

                // A backwards clock breaks the uniqueness guarantee: this generator keeps
                // _lastStamp only in memory, so after a restart a clock that is now behind
                // where it was would re-issue timestamps (and ids) it already handed out.
                // The old code silently clamped to _lastStamp and carried on, which hid the
                // problem and, once the sequence for that millisecond filled, spun for the
                // whole length of the jump. Fail loudly instead.
                if (timestamp < _lastStamp)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.CurrentCulture, RS.FlakeGenerator_ClockMovedBackwards,
                            _lastStamp - timestamp, timestamp, _lastStamp));
                }

                // The timestamp field only holds an offset from the epoch that fits in
                // TimestampBits. If the clock is before the epoch (negative offset) or past
                // the rollover point, `msSinceEpoch << TimestampOffset` below would spill into
                // an adjacent field or the sign bit and yield a corrupt (often negative) id.
                long msSinceEpoch = timestamp - _config.Epoch;
                if (msSinceEpoch < 0 || msSinceEpoch > _maxMsSinceEpoch)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.CurrentCulture, RS.FlakeGenerator_ClockOutsideConfigWindow,
                            timestamp, _config.Epoch, _config.Epoch + _maxMsSinceEpoch,
                            _config.RolloverDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture)));
                }

                if (_lastStamp == timestamp)
                {
                    // Same millisecond: advance the sequence, wrapping to 0 past MaxSequence.
                    _sequence = (_sequence + 1) & _config.MaxSequence;

                    if (_sequence == 0L)
                    {
                        // This millisecond's sequence space is spent; wait for the next one.
                        long exhaustedStamp = _lastStamp;
                        SpinWait.SpinUntil(() => CurrentMillis() > exhaustedStamp);
                        continue;
                    }
                }
                else
                {
                    _sequence = 0L;
                }

                _lastStamp = timestamp;

                return (msSinceEpoch << _config.TimestampOffset)
                       | (_dataCenterId << _config.DatacenterOffset)
                       | (_machineId << _config.MachineOffset)
                       | _sequence;
            }
        }
    }

    private long CurrentMillis() => _time.GetUtcNow().ToUnixTimeMilliseconds();
}
