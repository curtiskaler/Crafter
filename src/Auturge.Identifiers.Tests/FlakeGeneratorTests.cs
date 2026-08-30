using Auturge.Identifiers.Instances;

namespace Auturge.Identifiers.Tests;

public class FlakeGeneratorTests
{
    private const long _twitterMachineOffset = 12;
    private const long _twitterDatacenterOffset = 17;
    private const long _fiveBitMask = 0x1F;

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
}
