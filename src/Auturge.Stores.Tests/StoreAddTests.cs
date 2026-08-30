using Auturge.Identifiers;
using Auturge.Stores.Tests.TestObjects;

namespace Auturge.Stores.Tests;

[TestFixture]
public class StoreAddTests
{
    [Test]
    public async Task AddAsync_Should_InsertEntity_When_EntityIsValid()
    {
        var store = new UserStore();
        var user = new User("maxHeadroom") { GivenName = "Max", SurName = "Headroom" };

        await store.AddAsync(user);

        User? retrieved = await store.GetByIdAsync(user.Id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.GivenName, Is.EqualTo("Max"));
    }

    [Test]
    public async Task AddAsync_Should_StampCreatedAndLastUpdated_When_EntityIsAudited()
    {
        var store = new UserStore();
        var user = new User("stamped") { Created = DateTimeOffset.MinValue, LastUpdated = DateTimeOffset.MinValue };
        var before = DateTimeOffset.UtcNow;

        await store.AddAsync(user);

        Assert.That(user.Created, Is.GreaterThanOrEqualTo(before));
        Assert.That(user.LastUpdated, Is.EqualTo(user.Created));
    }

    [Test]
    public async Task AddAsync_Should_AssignVersion_When_EntityIsConcurrent()
    {
        var store = new UserStore();
        var user = new User("versioned");

        await store.AddAsync(user);

        Assert.That(user.ConcurrencyToken, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void AddAsync_Should_ThrowArgumentNullException_When_EntityIsNull()
    {
        var store = new UserStore();

        ArgumentNullException? exception =
            Assert.ThrowsAsync<ArgumentNullException>(async () => await store.AddAsync(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("entity"));
    }

    [Test]
    public async Task AddAsync_Should_ThrowArgumentException_When_IdAlreadyExists()
    {
        var store = new UserStore();
        long duplicateId = Flake.NewFlake();
        await store.AddAsync(new User(duplicateId, "Original"));

        ArgumentException? exception = Assert.ThrowsAsync<ArgumentException>(
            async () => await store.AddAsync(new User(duplicateId, "Duplicate")));

        Assert.That(exception!.Message, Does.Contain($"'{duplicateId}' already exists"));
    }

    [Test]
    public async Task AddAsync_Should_ThrowAndNotTrack_When_CancellationIsRequested()
    {
        var store = new UserStore();
        var user = new User("Cancelled Run");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await store.AddAsync(user, cts.Token));

        User? retrieved = await store.GetByIdAsync(user.Id);
        Assert.That(retrieved, Is.Null);
    }
}
