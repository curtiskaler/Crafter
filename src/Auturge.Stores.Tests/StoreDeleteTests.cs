using Auturge.Identifiers;
using Auturge.Stores.Tests.TestObjects;

namespace Auturge.Stores.Tests;

[TestFixture]
public class StoreDeleteTests
{
    public UserStore CreateSystemUnderTest() => new();

    [Test]
    public async Task DeleteAsync_Should_ReturnTrue_When_EntityIsSoftDeletableAndPresent()
    {
        UserStore store = CreateSystemUnderTest();
        var user = new User(Flake.NewFlake(), "to-delete");
        await store.AddAsync(user);

        bool deleted = await store.DeleteAsync(user.Id);

        Assert.That(deleted, Is.True);
    }

    [Test]
    public async Task DeleteAsync_Should_KeepKeyOccupied_When_EntityIsSoftDeletable()
    {
        UserStore store = CreateSystemUnderTest();
        long id = Flake.NewFlake();
        await store.AddAsync(new User(id, "soft"));
        await store.DeleteAsync(id);

        ArgumentException? exception = Assert.ThrowsAsync<ArgumentException>(
            async () => await store.AddAsync(new User(id, "reused")));

        Assert.That(exception!.Message, Does.Contain($"'{id}'"));
    }

    [Test]
    public async Task DeleteAsync_Should_ReturnFalse_When_EntityAlreadySoftDeleted()
    {
        UserStore store = CreateSystemUnderTest();
        var user = new User(Flake.NewFlake(), "double-delete");
        await store.AddAsync(user);
        await store.DeleteAsync(user.Id);

        bool secondDelete = await store.DeleteAsync(user.Id);

        Assert.That(secondDelete, Is.False);
    }

    [Test]
    public async Task DeleteAsync_Should_ReturnFalse_When_KeyIsAbsent()
    {
        UserStore store = CreateSystemUnderTest();

        bool deleted = await store.DeleteAsync(Flake.NewFlake());

        Assert.That(deleted, Is.False);
    }

    [Test]
    public async Task GetByIdAsync_Should_ReturnNull_When_EntityIsSoftDeleted()
    {
        UserStore store = CreateSystemUnderTest();
        var user = new User(Flake.NewFlake(), "hidden");
        await store.AddAsync(user);
        await store.DeleteAsync(user.Id);

        User? retrieved = await store.GetByIdAsync(user.Id);

        Assert.That(retrieved, Is.Null);
    }

    [Test]
    public async Task ContainsKeyAsync_Should_ReturnFalse_When_EntityIsSoftDeleted()
    {
        UserStore store = CreateSystemUnderTest();
        var user = new User(Flake.NewFlake(), "hidden");
        await store.AddAsync(user);
        await store.DeleteAsync(user.Id);

        bool contains = await store.ContainsKeyAsync(user.Id);

        Assert.That(contains, Is.False);
    }

    [Test]
    public async Task GetAllAsync_Should_ExcludeSoftDeletedEntities()
    {
        UserStore store = CreateSystemUnderTest();
        var kept = new User(Flake.NewFlake(), "kept");
        var removed = new User(Flake.NewFlake(), "removed");
        await store.AddAsync(kept);
        await store.AddAsync(removed);
        await store.DeleteAsync(removed.Id);

        IEnumerable<User> all = await store.GetAllAsync();

        Assert.That(all, Is.EquivalentTo(new[] { kept }));
    }

    [Test]
    public async Task Query_Should_ExcludeSoftDeletedEntities()
    {
        UserStore store = CreateSystemUnderTest();
        var kept = new User(Flake.NewFlake(), "kept");
        var removed = new User(Flake.NewFlake(), "removed");
        await store.AddAsync(kept);
        await store.AddAsync(removed);
        await store.DeleteAsync(removed.Id);

        List<User> visible = store.Query().ToList();

        Assert.That(visible, Is.EquivalentTo(new[] { kept }));
    }

    [Test]
    public async Task FindByAsync_Should_ReturnNull_When_OnlyMatchIsSoftDeleted()
    {
        UserStore store = CreateSystemUnderTest();
        var user = new User(Flake.NewFlake(), "needle");
        await store.AddAsync(user);
        await store.DeleteAsync(user.Id);

        User? found = await store.FindByAsync(u => u.UserName == "needle");

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task FindAllByAsync_Should_ExcludeSoftDeletedEntities()
    {
        UserStore store = CreateSystemUnderTest();
        var kept = new User(Flake.NewFlake(), "shared") { GivenName = "kept" };
        var removed = new User(Flake.NewFlake(), "shared") { GivenName = "removed" };
        await store.AddAsync(kept);
        await store.AddAsync(removed);
        await store.DeleteAsync(removed.Id);

        IEnumerable<User> matches = await store.FindAllByAsync(u => u.UserName == "shared");

        Assert.That(matches, Is.EquivalentTo(new[] { kept }));
    }
}
