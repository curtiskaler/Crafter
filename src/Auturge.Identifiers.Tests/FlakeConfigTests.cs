using Auturge.Identifiers.Instances;

namespace Auturge.Identifiers.Tests;

public class FlakeConfigTests
{
    [Test]
    public void Epoch_Should_MatchUnixMillisOfEpochDate_When_ConstructedFromDateTime()
    {
        var config = new FlakeConfig(typeof(long), new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 12, 5, 5);

        Assert.That(config.Epoch, Is.EqualTo(1735689600000L));
    }

    [Test]
    public void Epoch_Should_IgnoreLocalTimeOfDay_When_ConstructedFromUtcMidnight()
    {
        var config = new FlakeConfig(typeof(long), new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), 12, 5, 5);

        Assert.That(config.Epoch, Is.EqualTo(946684800000L));
    }

    [Test]
    public void RolloverDate_Should_BeDecadesAfterEpoch_When_TimestampBitsAreLarge()
    {
        var config = new FlakeConfig(typeof(long), 0L, 12, 5, 5);

        Assert.That(config.RolloverDate.Year, Is.GreaterThan(2030));
    }

    [Test]
    public void RolloverDate_Should_BeCenturiesAway_When_UsingFunsiesConfig()
    {
        Assert.That(FlakeConfigs.Funsies.RolloverDate.Year, Is.GreaterThan(2250));
    }

    [Test]
    public void MaxValues_Should_MatchBitWidths_When_Configured()
    {
        var config = new FlakeConfig(typeof(long), 0L, 12, 5, 5);

        Assert.That(config.MaxSequence, Is.EqualTo(4095));
        Assert.That(config.MaxMachineNum, Is.EqualTo(31));
        Assert.That(config.MaxDatacenterNum, Is.EqualTo(31));
    }

    [Test]
    public void Offsets_Should_StackFromLeastSignificantField_When_Configured()
    {
        var config = new FlakeConfig(typeof(long), 0L, 12, 5, 5);

        Assert.That(config.SequenceOffset, Is.EqualTo(0));
        Assert.That(config.MachineOffset, Is.EqualTo(12));
        Assert.That(config.DatacenterOffset, Is.EqualTo(17));
        Assert.That(config.TimestampOffset, Is.EqualTo(22));
    }

    [Test]
    public void Equals_Should_ReturnTrue_When_ConstructedWithSameParameters()
    {
        var a = new FlakeConfig(typeof(long), 0L, 12, 5, 5);
        var b = new FlakeConfig(typeof(long), 0L, 12, 5, 5);

        Assert.That(a.Equals(b), Is.True);
        Assert.That(a == b, Is.True);
        Assert.That(a != b, Is.False);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_SequenceBitsDiffer()
    {
        var a = new FlakeConfig(typeof(long), 0L, 12, 5, 5);
        var b = new FlakeConfig(typeof(long), 0L, 11, 5, 5);

        Assert.That(a == b, Is.False);
        Assert.That(a != b, Is.True);
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_EpochDiffers()
    {
        var a = new FlakeConfig(typeof(long), 0L, 12, 5, 5);
        var b = new FlakeConfig(typeof(long), 1_000_000L, 12, 5, 5);

        Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Equals_Should_ReturnTrue_When_EpochGivenAsDateTimeOrEquivalentMillis()
    {
        var fromDate = new FlakeConfig(typeof(long), new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 12, 5, 5);
        var fromMillis = new FlakeConfig(typeof(long), 1735689600000L, 12, 5, 5);

        Assert.That(fromDate, Is.EqualTo(fromMillis));
        Assert.That(fromDate.GetHashCode(), Is.EqualTo(fromMillis.GetHashCode()));
    }

    [Test]
    public void EqualityOperator_Should_ReturnTrue_When_ComparingTwitterWithSnowFlakeAlias()
    {
        Assert.That(FlakeConfigs.Twitter == FlakeConfigs.SnowFlake, Is.True);
    }

    [Test]
    public void Equals_Should_ReturnFalse_When_ComparedWithNullOrNonConfig()
    {
        var config = new FlakeConfig(typeof(long), 0L, 12, 5, 5);

        Assert.That(config.Equals("nope"), Is.False);
        Assert.That(config.Equals((object?)null), Is.False);
    }
}
