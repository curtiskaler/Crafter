using Auturge.Identifiers.Instances;

namespace Auturge.Identifiers;

/// <summary>
/// The flake factory.
/// </summary>
public sealed class FlakeGenerator
{
    // Note that the config values don't really matter to the ID
    // ... and this only really supports configs based on longs.
    // ... so if you want a config based on int, you'll need to deal with that.

    private readonly long _dataCenterId;
    private readonly long _machineId;

    private readonly FlakeConfig _config;
    private long _sequence;
    private long _lastStamp = -1L;

    private readonly Lock _lockObj = new();

    public FlakeGenerator() : this(FlakeConfigs.SnowFlake)
    {
    }

    public FlakeGenerator(FlakeConfig? config, long datacenterId = 0, long machineId = 0)
    {
        _config = config ?? FlakeConfigs.Twitter;

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
    ///     Generate the next ID
    /// </summary>
    /// <returns></returns>
    public long GetNextId()
    {
        lock (_lockObj)
        {
            while (true)
            {
                var timestamp = GetNewStamp();
                if (timestamp < _lastStamp)
                {
                    // Set the clock back and update it to the timestamp of the last generated ID
                    timestamp = _lastStamp;
                }

                if (_lastStamp == timestamp)
                {
                    // In the same millisecond, the sequence number increases automatically
                    _sequence = (_sequence + 1) & _config.MaxSequence;

                    // The maximum number of sequences in the same millisecond has been reached
                    if (_sequence == 0L)
                    {
                        var localTimeStamp = _lastStamp;
                        SpinWait.SpinUntil(() => GetNewStamp() > localTimeStamp);
                        continue;
                    }
                }
                else
                {
                    // In different milliseconds, the sequence number is set to 0
                    _sequence = 0L;
                }

                _lastStamp = timestamp;
                var msSinceEpoch = timestamp - _config.Epoch;

                return (msSinceEpoch << _config.TimestampOffset)
                       | (_dataCenterId << _config.DatacenterOffset)
                       | (_machineId << _config.MachineOffset)
                       | _sequence;
            }
        }
    }

    private static long GetNewStamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
