using Auturge.Identifiers.Instances;

namespace Auturge.Identifiers.Tests;

public class FlakeGeneratorTests
{
    private const long _twitterMachineOffset = 12;
    private const long _twitterDatacenterOffset = 17;
    private const long _fiveBitMask = 0x1F;

    // A clock the test drives explicitly; it never moves on its own.
    private sealed class MutableClock(long startMillis) : TimeProvider
    {
        private long _millis = startMillis;
        public override DateTimeOffset GetUtcNow() => DateTimeOffset.FromUnixTimeMilliseconds(_millis);
        public void Rewind(long millis) => _millis -= millis;
    }

    // Yields each scripted timestamp once, then keeps returning the last — enough for a
    // SpinUntil waiting on the clock to advance to terminate.
    private sealed class ScriptedClock(params long[] millis) : TimeProvider
    {
        private readonly Queue<long> _script = new(millis);
        private long _last;

        public override DateTimeOffset GetUtcNow()
        {
            if (_script.Count > 0) _last = _script.Dequeue();
            return DateTimeOffset.FromUnixTimeMilliseconds(_last);
        }
    }

    private static long Millis(int year)
        => new DateTimeOffset(new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc)).ToUnixTimeMilliseconds();

    [Test]
    public void GetNextId_Should_ReturnIncreasingValues_When_CalledRepeatedly()
    {
        var generator = new FlakeGenerator(FlakeConfigs.Twitter);

        long first = generator.GetNextId();
        long second = generator.GetNextId();
        long third = generator.GetNextId();

        Assert.That(second, Is.GreaterThan(first));
        Assert.That(third, Is.GreaterThan(second));
    }

    [Test]
    public void GetNextId_Should_EncodeOwnDatacenterAndMachine_When_AnotherGeneratorExists()
    {
        var first = new FlakeGenerator(FlakeConfigs.Twitter, datacenterId: 1, machineId: 2);
        var second = new FlakeGenerator(FlakeConfigs.Twitter, datacenterId: 3, machineId: 4);

        second.GetNextId();
        long id = first.GetNextId();
        long datacenterId = (id >> (int)_twitterDatacenterOffset) & _fiveBitMask;
        long machineId = (id >> (int)_twitterMachineOffset) & _fiveBitMask;

        Assert.That(datacenterId, Is.EqualTo(1));
        Assert.That(machineId, Is.EqualTo(2));
    }

    [Test]
    public void Ctor_Should_Throw_When_DatacenterIdExceedsConfigMaximum()
    {
        Assert.That(() => new FlakeGenerator(FlakeConfigs.Twitter, datacenterId: 32),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_MachineIdIsNegative()
    {
        Assert.That(() => new FlakeGenerator(FlakeConfigs.Twitter, machineId: -1),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void NewFlake_Should_ProduceDistinctValues_When_CalledRepeatedly()
    {
        var generator = new FlakeGenerator(FlakeConfigs.Twitter);

        Flake first = generator.NewFlake();
        Flake second = generator.NewFlake();

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void NewFlake_Should_DecodeComponentsWithOwnConfig_When_ConfigDiffersFromGlobalDefault()
    {
        var generator = new FlakeGenerator(FlakeConfigs.Twitter, datacenterId: 3, machineId: 7);

        Flake flake = generator.NewFlake();

        Assert.That(flake.DataCenterId, Is.EqualTo(3));
        Assert.That(flake.MachineId, Is.EqualTo(7));
        Assert.That(flake.Sequence, Is.InRange(0, FlakeConfigs.Twitter.MaxSequence));
    }

    [Test]
    public void GetNextId_Should_Throw_When_ClockMovesBackwards()
    {
        var clock = new MutableClock(1_700_000_000_000L);
        var generator = new FlakeGenerator(FlakeConfigs.Twitter, 0, 0, clock);
        generator.GetNextId();

        clock.Rewind(50);

        Assert.That(() => generator.GetNextId(), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void GetNextId_Should_IncrementSequence_When_ClockStaysInTheSameMillisecond()
    {
        var clock = new MutableClock(1_700_000_000_000L);
        var generator = new FlakeGenerator(FlakeConfigs.Twitter, 0, 0, clock);

        long first = generator.GetNextId();
        long second = generator.GetNextId();
        long third = generator.GetNextId();

        Assert.That(first & FlakeConfigs.Twitter.MaxSequence, Is.EqualTo(0));
        Assert.That(second & FlakeConfigs.Twitter.MaxSequence, Is.EqualTo(1));
        Assert.That(third & FlakeConfigs.Twitter.MaxSequence, Is.EqualTo(2));
    }

    [Test]
    public void GetNextId_Should_Throw_When_ClockIsBeforeTheConfiguredEpoch()
    {
        var config = new FlakeConfig(typeof(long), Millis(2025), 12, 5, 5);
        var generator = new FlakeGenerator(config, 0, 0, new MutableClock(Millis(2020)));

        Assert.That(() => generator.GetNextId(), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void GetNextId_Should_Throw_When_ClockIsPastTheRolloverDate()
    {
        var config = new FlakeConfig(typeof(long), 0L, 12, 5, 5); // 41 timestamp bits, rolls over ~2039
        var generator = new FlakeGenerator(config, 0, 0, new MutableClock(Millis(2100)));

        Assert.That(() => generator.GetNextId(), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void GetNextId_Should_Succeed_When_ClockIsInsideTheConfiguredWindow()
    {
        var config = new FlakeConfig(typeof(long), Millis(2020), 12, 5, 5);
        var generator = new FlakeGenerator(config, 0, 0, new MutableClock(Millis(2024)));

        long id = generator.GetNextId();

        Assert.That(id, Is.Positive);
    }

    [Test]
    public void GetNextId_Should_AdvanceToNextMillisecond_When_SequenceIsExhausted()
    {
        long epoch = Millis(2020);
        long t = epoch + 1_000;
        var config = new FlakeConfig(typeof(long), epoch, 1, 0, 0); // MaxSequence == 1
        var generator = new FlakeGenerator(config, 0, 0, new ScriptedClock(t, t, t, t + 1));

        long first = generator.GetNextId();
        long second = generator.GetNextId();
        long third = generator.GetNextId();

        Assert.That(second & config.MaxSequence, Is.EqualTo(1));
        Assert.That(third & config.MaxSequence, Is.Zero);
        Assert.That(third >> config.TimestampOffset, Is.EqualTo((first >> config.TimestampOffset) + 1));
    }
}
