using Auturge.Identifiers.Instances;

namespace Auturge.Identifiers.Tests;

// These mutate the process-wide ambient config; each test resets it to the default in TearDown.
public class FlakeAmbientConfigTests
{
    private static long Millis(int year)
        => new DateTimeOffset(new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc)).ToUnixTimeMilliseconds();

    [TearDown]
    public void ResetAmbient() => Flake.Configure(FlakeConfigs.Funsies, 0, 0);

    [Test]
    public void Config_Should_RoundTripAssignedValue()
    {
        var custom = new FlakeConfig(typeof(long), 0L, 15, 4, 4);

        Flake.Config = custom;

        Assert.That(Flake.Config, Is.EqualTo(custom));
    }

    [Test]
    public void Config_Should_ReturnToDefault_When_Reset()
    {
        Flake.Config = new FlakeConfig(typeof(long), 0L, 15, 4, 4);

        Flake.Configure(FlakeConfigs.Funsies, 0, 0);

        Assert.That(Flake.Config, Is.EqualTo(FlakeConfigs.Funsies));
    }

    [Test]
    public void Decode_Should_UseTheReassignedConfig()
    {
        var custom = new FlakeConfig(typeof(long), 0L, 10, 5, 5);
        Flake.Config = custom;
        long packed = (3L << custom.MachineOffset) | 42L;

        var flake = new Flake(packed);

        Assert.That(flake.Sequence, Is.EqualTo(42));
        Assert.That(flake.MachineId, Is.EqualTo(3));
    }

    [Test]
    public void NewFlake_Should_EncodeWithTheReassignedConfig()
    {
        var custom = new FlakeConfig(typeof(long), Millis(2020), 10, 5, 5);
        Flake.Config = custom;

        var decoded = new Flake(Flake.NewFlake());

        Assert.That(decoded.Sequence, Is.InRange(0, custom.MaxSequence));
        Assert.That(decoded.TimeStamp, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromMinutes(1)));
    }

    [Test]
    public void Configure_Should_SetSourceIdsUsedByNewFlake()
    {
        Flake.Configure(FlakeConfigs.Twitter, dataCenterId: 3, machineId: 7);

        var decoded = new Flake(Flake.NewFlake());

        Assert.That(decoded.DataCenterId, Is.EqualTo(3));
        Assert.That(decoded.MachineId, Is.EqualTo(7));
    }

    [Test]
    public void Config_Should_CarryOverSourceIds_When_Reassigned()
    {
        Flake.Configure(FlakeConfigs.Twitter, dataCenterId: 2, machineId: 9);

        Flake.Config = new FlakeConfig(typeof(long), FlakeConfigs.Twitter.Epoch, 10, 5, 5);

        var decoded = new Flake(Flake.NewFlake());
        Assert.That(decoded.DataCenterId, Is.EqualTo(2));
        Assert.That(decoded.MachineId, Is.EqualTo(9));
    }

    [Test]
    public void Config_Should_Throw_When_NewLayoutCannotHoldConfiguredSourceIds()
    {
        Flake.Configure(FlakeConfigs.Twitter, dataCenterId: 3, machineId: 7);

        Assert.That(() => Flake.Config = FlakeConfigs.Funsies,
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }
}
