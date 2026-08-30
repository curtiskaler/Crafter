namespace Auturge.Identifiers.Tests;

public class FlakeTests
{
    private static long UnixMillis(DateTime utc) => new DateTimeOffset(utc).ToUnixTimeMilliseconds();

    [Test]
    public void Ctor_Should_Throw_When_SequenceExceedsConfigMaximum()
    {
        long timestamp = UnixMillis(new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc));

        Assert.That(() => new Flake(sequence: Flake.Config.MaxSequence + 1L, timestamp: timestamp, dataCenterId: 0, machineId: 0),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_TimestampIsBeforeEpoch()
    {
        Assert.That(() => new Flake(sequence: 0, timestamp: Flake.Config.Epoch - 1, dataCenterId: 0, machineId: 0),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_TimestampExceedsConfiguredBits()
    {
        long tooFar = Flake.Config.Epoch + (1L << Flake.Config.TimestampBits) + 1;

        Assert.That(() => new Flake(sequence: 0, timestamp: tooFar, dataCenterId: 0, machineId: 0),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Ctor_Should_DecodeComponents_When_GivenExplicitConfig()
    {
        var config = new FlakeConfig(typeof(long), 0L, 12, 5, 5);
        long value = (500L << config.TimestampOffset)
                     | (3L << config.DatacenterOffset)
                     | (7L << config.MachineOffset)
                     | 42L;

        var flake = new Flake(value, config);

        Assert.That(flake.Sequence, Is.EqualTo(42));
        Assert.That(flake.MachineId, Is.EqualTo(7));
        Assert.That(flake.DataCenterId, Is.EqualTo(3));
    }

    [Test]
    public void Ctor_Should_RoundTripValueAndComponents_When_DecodedFromValue()
    {
        long timestamp = UnixMillis(new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc));
        var original = new Flake(sequence: 42, timestamp: timestamp, dataCenterId: 0, machineId: 0);

        var decoded = new Flake(original.Value);

        Assert.That(decoded.Value, Is.EqualTo(original.Value));
        Assert.That(decoded.Sequence, Is.EqualTo(42));
        Assert.That(decoded.TimeStamp, Is.EqualTo(original.TimeStamp));
    }

    [Test]
    public void Ctor_Should_DecodeAbsoluteCalendarTime_When_DecodedFromValue()
    {
        var moment = new DateTime(2025, 6, 15, 12, 30, 45, DateTimeKind.Utc);
        var flake = new Flake(sequence: 1, timestamp: UnixMillis(moment), dataCenterId: 0, machineId: 0);

        var decoded = new Flake(flake.Value);

        Assert.That(decoded.TimeStamp, Is.EqualTo(new DateTime(2025, 6, 15, 12, 30, 45)));
    }

    [Test]
    public void GetHashCode_Should_BeConsistentWithEquals_When_ValuesMatchButComponentsDiffer()
    {
        var viaInitializer = new Flake { Value = 12345L };
        var viaDecode = new Flake(12345L);

        Assert.That(viaInitializer, Is.EqualTo(viaDecode));
        Assert.That(viaInitializer.GetHashCode(), Is.EqualTo(viaDecode.GetHashCode()));
    }

    [Test]
    public void Flakes_Should_DeduplicateInHashSet_When_ValuesMatch()
    {
        var set = new HashSet<Flake> { new() { Value = 999L }, new(999L) };

        Assert.That(set, Has.Count.EqualTo(1));
    }

    [Test]
    public void CompareTo_Should_OrderByValue_When_Compared()
    {
        var low = new Flake(100L);
        var high = new Flake(200L);

        Assert.That(low.CompareTo(high), Is.Negative);
        Assert.That(high.CompareTo(low), Is.Positive);
        Assert.That(low.CompareTo(100L), Is.Zero);
    }

    [Test]
    public void Equals_Should_ReturnTrue_When_ValuesMatch()
    {
        var a = new Flake { Value = 777L };
        var b = new Flake { Value = 777L };

        Assert.That(a.Equals(b), Is.True);
        Assert.That(a == b, Is.True);
        Assert.That(a != b, Is.False);
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_ValuesDiffer()
    {
        var a = new Flake { Value = 1L };
        var b = new Flake { Value = 2L };

        Assert.That(a.Equals(b), Is.False);
        Assert.That(a == b, Is.False);
        Assert.That(a != b, Is.True);
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_ComparedWithNullOrNonFlake()
    {
        var flake = new Flake { Value = 5L };

        Assert.That(flake.Equals("5"), Is.False);
        Assert.That(flake.Equals((object?)null), Is.False);
    }

    [Test]
    public void Equals_Should_ReturnTrue_When_ComparedWithBoxedEqualFlake()
    {
        var flake = new Flake { Value = 5L };
        object boxed = new Flake { Value = 5L };

        Assert.That(flake.Equals(boxed), Is.True);
    }

    [Test]
    public void Ctor_Should_Throw_When_DataCenterIdIsNegative()
    {
        Assert.That(() => new Flake(sequence: 0, timestamp: 0, dataCenterId: -1, machineId: 0),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Ctor_Should_Throw_When_MachineIdExceedsConfigMaximum()
    {
        Assert.That(() => new Flake(sequence: 0, timestamp: 0, dataCenterId: 0, machineId: 1),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }
}
