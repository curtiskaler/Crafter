using Auturge.Identifiers;
using Auturge.Stores.Tests.TestObjects;

namespace Auturge.Stores.Tests;

[TestFixture]
public class StoreUpdateTests
{
    public UserStore CreateSystemUnderTest() => new();

    [Test]
    public async Task UpdateAsync_Should_PersistChangesAndReturnEntity_When_EntityExists()
    {
        UserStore store = CreateSystemUnderTest();
        long id = Flake.NewFlake();
        var originalEntity = new User(id, "Original Name");
        await store.AddAsync(originalEntity);

        var updatedEntity = new User(id, "Updated Name") { ConcurrencyToken = originalEntity.ConcurrencyToken };
        User result = await store.UpdateAsync(updatedEntity);

        Assert.That(result.UserName, Is.EqualTo("Updated Name"));
        User? retrieved = await store.GetByIdAsync(id);
        Assert.That(retrieved!.UserName, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task UpdateAsync_Should_AdvanceVersion_When_EntityIsConcurrent()
    {
        UserStore store = CreateSystemUnderTest();
        long id = Flake.NewFlake();
        var original = new User(id, "v1");
        await store.AddAsync(original);
        Guid versionAfterAdd = original.ConcurrencyToken;

        var update = new User(id, "v2") { ConcurrencyToken = versionAfterAdd };
        await store.UpdateAsync(update);

        Assert.That(update.ConcurrencyToken, Is.Not.EqualTo(versionAfterAdd));
    }

    [Test]
    public async Task UpdateAsync_Should_BumpLastUpdated_When_EntityIsAudited()
    {
        UserStore store = CreateSystemUnderTest();
        long id = Flake.NewFlake();
        var original = new User(id, "audited");
        await store.AddAsync(original);

        var update = new User(id, "audited edited") { ConcurrencyToken = original.ConcurrencyToken, LastUpdated = DateTimeOffset.MinValue };
        var before = DateTimeOffset.UtcNow;
        await store.UpdateAsync(update);

        Assert.That(update.LastUpdated, Is.GreaterThanOrEqualTo(before));
    }

    [Test]
    public async Task UpdateAsync_Should_ThrowInvalidOperationException_When_VersionIsStale()
    {
        UserStore store = CreateSystemUnderTest();
        long id = Flake.NewFlake();
        var original = new User(id, "contested");
        await store.AddAsync(original);

        var staleUpdate = new User(id, "stale writer") { ConcurrencyToken = Guid.NewGuid() };
        InvalidOperationException? exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await store.UpdateAsync(staleUpdate));

        Assert.That(exception!.Message, Does.Contain("Concurrency violation"));
    }

    [Test]
    public void UpdateAsync_Should_ThrowArgumentNullException_When_EntityIsNull()
    {
        UserStore store = CreateSystemUnderTest();

        ArgumentNullException? exception = Assert.ThrowsAsync<ArgumentNullException>(
            async () => await store.UpdateAsync(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("entity"));
    }

    [Test]
    public void UpdateAsync_Should_ThrowKeyNotFoundException_When_KeyDoesNotExist()
    {
        UserStore store = CreateSystemUnderTest();
        var nonExistentEntity = new User(Flake.NewFlake(), "Ghost Entity");

        KeyNotFoundException? exception = Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await store.UpdateAsync(nonExistentEntity));

        Assert.That(exception!.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task UpdateAsync_Should_ThrowAndLeaveStateUnchanged_When_CancellationIsRequested()
    {
        UserStore store = CreateSystemUnderTest();
        long id = Flake.NewFlake();
        var entity = new User(id, "Test Entity");
        await store.AddAsync(entity);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await store.UpdateAsync(entity, cts.Token));

        User? retrieved = await store.GetByIdAsync(id, CancellationToken.None);
        Assert.That(retrieved!.UserName, Is.EqualTo("Test Entity"));
    }
}
