using Auturge.Identifiers;
using Auturge.Stores.Tests.TestObjects;

namespace Auturge.Stores.Tests;

[TestFixture]
public class StoreAddTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task AddAsync_WhenEntityIsValid_ShouldInsertSuccessfully()
    {
        var store = new UserStore();
        var user = new User("maxHeadroom") { GivenName = "Max", SurName = "Headroom" };

        Assert.DoesNotThrowAsync(async () => await store.Add(user));

        User? retrieved = await store.GetById(user.Id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.GivenName, Is.EqualTo(user.GivenName));
    }

    [Test]
    public async Task AddAsync_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var store = new UserStore();

        // Act & Assert
        // Because our implementation validates and returns Task.FromException immediately 
        // without awaiting inside a state machine, the exception surfaces right away.
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await store.Add(null!));

        // Assert
        Assert.That(exception.ParamName, Is.EqualTo("entity"));
    }

    [Test]
    public async Task AddAsync_WhenIdAlreadyExists_ShouldThrowArgumentException()
    {
        // Arrange
        var store = new UserStore();
        long duplicateId = Flake.NewFlake();
        var originalEntity = new User(duplicateId, "Original");
        var duplicateEntity = new User(duplicateId, "Duplicate");

        await store.Add(originalEntity);

        // Act & Assert
        ArgumentException? exception =
            Assert.ThrowsAsync<ArgumentException>(async () => await store.Add(duplicateEntity));

        Assert.That(exception.Message, Does.Contain($"'{duplicateId}' already exists"));
    }

    [Test]
    public async Task AddAsync_WhenCancellationIsRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var store = new UserStore();
        var entity = new User("Cancelled Run");

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Force immediate cancellation state

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () => await store.Add(entity, cts.Token));

        // Ensure data was never tracked due to early exit
        User? retrieved = await store.GetById(entity.Id);
        Assert.That(retrieved, Is.Null);
    }
}
