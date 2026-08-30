using Auturge.Identifiers.Instances;

namespace Auturge.Identifiers;

/// <summary>
/// The flake factory.
/// </summary>
public sealed class FlakeGenerator
{
    // Every id is packed into a signed 64-bit long, so only long-based configs are
    // supported; FlakeConfig now rejects any other output type.

    private readonly long _dataCenterId;
    private readonly long _machineId;

    private readonly FlakeConfig _config;
    private readonly TimeProvider _time;
    private long _sequence;
    private long _lastStamp = -1L;

    private readonly Lock _lockObj = new();

    public FlakeGenerator() : this(FlakeConfigs.SnowFlake)
    {
    }

    /// <summary>
    /// Creates a generator for the given <paramref name="config"/> and source ids.
    /// </summary>
    /// <param name="timeProvider">
    /// Clock used for id timestamps; defaults to <see cref="TimeProvider.System"/>. Exposed
    /// primarily so tests can control the clock.
    /// </param>
    public FlakeGenerator(FlakeConfig? config, long datacenterId = 0, long machineId = 0,
        TimeProvider? timeProvider = null)
    {
        _config = config ?? FlakeConfigs.Twitter;
        _time = timeProvider ?? TimeProvider.System;

        if (datacenterId > _config.MaxDatacenterNum || datacenterId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(datacenterId),
                @$"datacenterId can't be greater than {_config.MaxDatacenterNum} or less than 0");
        }

        if (machineId > _config.MaxMachineNum || machineId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(machineId),
                @$"machineId can't be greater than {_config.MaxMachineNum} or less than 0");
        }

        _dataCenterId = datacenterId;
        _machineId = machineId;
    }

    public Flake NewFlake()
    {
        long value = GetNextId();
        return new Flake(value, _config);
    }

    /// <summary>
    /// Generates the next id.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The clock has moved backwards since the previous id was issued, so uniqueness can no
    /// longer be guaranteed. Callers that expect transient clock corrections may catch this
    /// and retry after a short delay.
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
                        $"Clock moved backwards by {_lastStamp - timestamp} ms (now {timestamp}, last id used "
                        + $"{_lastStamp}); refusing to generate ids that could collide with ones already issued.");
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
                long msSinceEpoch = timestamp - _config.Epoch;

                return (msSinceEpoch << _config.TimestampOffset)
                       | (_dataCenterId << _config.DatacenterOffset)
                       | (_machineId << _config.MachineOffset)
                       | _sequence;
            }
        }
    }

    private long CurrentMillis() => _time.GetUtcNow().ToUnixTimeMilliseconds();
}
