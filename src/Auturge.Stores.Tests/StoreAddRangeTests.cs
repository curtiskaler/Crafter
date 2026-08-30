using Auturge.Identifiers;
using Auturge.Stores.Tests.TestObjects;

namespace Auturge.Stores.Tests;

[TestFixture]
public class StoreAddRangeTests
{
    public UserStore CreateSystemUnderTest() => new();

    [Test]
    public async Task AddRangeAsync_Should_PersistEveryEntity_When_BatchIsValid()
    {
        UserStore store = CreateSystemUnderTest();
        var first = new User(Flake.NewFlake(), "first");
        var second = new User(Flake.NewFlake(), "second");

        IEnumerable<User> added = await store.AddRangeAsync(new[] { first, second });

        Assert.That(added, Is.EquivalentTo(new[] { first, second }));
        Assert.That(await store.ContainsKeyAsync(first.Id), Is.True);
        Assert.That(await store.ContainsKeyAsync(second.Id), Is.True);
    }

    [Test]
    public async Task AddRangeAsync_Should_StampEntities_When_BatchIsValid()
    {
        UserStore store = CreateSystemUnderTest();
        var user = new User(Flake.NewFlake(), "stamped")
        {
            Created = DateTimeOffset.MinValue,
            LastUpdated = DateTimeOffset.MinValue
        };
        var before = DateTimeOffset.UtcNow;

        await store.AddRangeAsync(new[] { user });

        Assert.That(user.ConcurrencyToken, Is.Not.EqualTo(Guid.Empty));
        Assert.That(user.Created, Is.GreaterThanOrEqualTo(before));
        Assert.That(user.LastUpdated, Is.EqualTo(user.Created));
    }

    [Test]
    public async Task AddRangeAsync_Should_RollBackEntireBatch_When_BatchContainsDuplicateId()
    {
        UserStore store = CreateSystemUnderTest();
        long sharedId = Flake.NewFlake();
        var good = new User(Flake.NewFlake(), "good");
        var first = new User(sharedId, "first");
        var collides = new User(sharedId, "collides");

        Assert.ThrowsAsync<ArgumentException>(
            async () => await store.AddRangeAsync(new[] { good, first, collides }));

        Assert.That(await store.ContainsKeyAsync(good.Id), Is.False);
        Assert.That(await store.ContainsKeyAsync(sharedId), Is.False);
    }

    [Test]
    public async Task AddRangeAsync_Should_RollBackEntireBatch_When_BatchContainsNull()
    {
        UserStore store = CreateSystemUnderTest();
        var good = new User(Flake.NewFlake(), "good");

        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await store.AddRangeAsync(new[] { good, null! }));

        Assert.That(await store.ContainsKeyAsync(good.Id), Is.False);
    }

    [Test]
    public void AddRangeAsync_Should_ThrowArgumentNullException_When_CollectionIsNull()
    {
        UserStore store = CreateSystemUnderTest();

        ArgumentNullException? exception = Assert.ThrowsAsync<ArgumentNullException>(
            async () => await store.AddRangeAsync(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("entities"));
    }

    [Test]
    public async Task AddRangeAsync_Should_ThrowAndPersistNothing_When_CancellationIsRequested()
    {
        UserStore store = CreateSystemUnderTest();
        var user = new User(Flake.NewFlake(), "cancelled");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Assert.ThrowsAsync<TaskCanceledException>(
            async () => await store.AddRangeAsync(new[] { user }, cts.Token));

        Assert.That(await store.ContainsKeyAsync(user.Id), Is.False);
    }
}
