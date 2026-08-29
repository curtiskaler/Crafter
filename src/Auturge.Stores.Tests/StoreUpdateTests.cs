using Auturge.Identifiers;
using Auturge.Stores.Tests.TestObjects;

namespace Auturge.Stores.Tests;

[TestFixture]
public class StoreUpdateTests
{
    [SetUp]
    public void Setup()
    {
    }

    public UserStore CreateSystemUnderTest() => new();

    [Test]
    public async Task UpdateAsync_WhenEntityExists_ShouldUpdateStateAndReturnEntity()
    {
        // Arrange
        UserStore store = CreateSystemUnderTest();
        long id = Flake.NewFlake();
        var originalEntity = new User(id, "Original Name");
        await store.Add(originalEntity);

        // Modify the entity fields
        var updatedEntity = new User(id, "Updated Name") { ConcurrencyToken = originalEntity.ConcurrencyToken };

        // Act
        User result = await store.Update(updatedEntity);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserName, Is.EqualTo("Updated Name"));

        // Double-check the internal state by fetching it back out
        User? retrieved = await store.GetById(id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.UserName, Is.EqualTo("Updated Name"));
    }

    [Test]
    public void UpdateAsync_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        UserStore store = CreateSystemUnderTest();

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await store.Update(null!));

        Assert.That(exception.ParamName, Is.EqualTo("entity"));
    }

    [Test]
    public void UpdateAsync_WhenKeyDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        UserStore store = CreateSystemUnderTest();
        var nonExistentEntity = new User(Flake.NewFlake(), "Ghost Entity");

        // Act & Assert
        KeyNotFoundException? exception = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await store.Update(nonExistentEntity));

        Assert.That(exception.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task UpdateAsync_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var store = CreateSystemUnderTest();
        var id = Flake.NewFlake();
        var entity = new User(id, "Test Entity");
        await store.Add(entity);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Force immediate cancellation state

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await store.Update(entity, cts.Token));

        // Verify internal store state remains unchanged
        User? retrieved = await store.GetById(id, CancellationToken.None);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.UserName, Is.EqualTo("Test Entity"));
    }
}
