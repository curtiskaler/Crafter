using Auturge.Identifiers.Instances;

namespace Auturge.Identifiers.Tests;

// These mutate the process-wide Flake.Config, so each test restores it in TearDown.
public class FlakeAmbientConfigTests
{
    private FlakeConfig _original;

    [SetUp]
    public void CaptureConfig() => _original = Flake.Config;

    [TearDown]
    public void RestoreConfig() => Flake.Config = _original;

    [Test]
    public void Config_Should_RoundTripAssignedValue()
    {
        var custom = new FlakeConfig(typeof(long), 0L, 15, 4, 4);

        Flake.Config = custom;

        Assert.That(Flake.Config, Is.EqualTo(custom));
    }

    [Test]
    public void Config_Should_ReturnToDefault_When_Restored()
    {
        Flake.Config = new FlakeConfig(typeof(long), 0L, 15, 4, 4);

        Flake.Config = _original;

        Assert.That(Flake.Config, Is.EqualTo(_original));
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
        var custom = new FlakeConfig(typeof(long),
            new DateTimeOffset(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)).ToUnixTimeMilliseconds(),
            10, 5, 5);
        Flake.Config = custom;

        var decoded = new Flake(Flake.NewFlake());

        Assert.That(decoded.Sequence, Is.InRange(0, custom.MaxSequence));
        Assert.That(decoded.TimeStamp, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromMinutes(1)));
    }
}
