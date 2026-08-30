using Auturge.Identifiers;
using Auturge.Stores.Stores;
using Auturge.Stores.Tests.TestObjects;

namespace Auturge.Stores.Tests;

[TestFixture]
public class InMemoryStoreTests
{
    private static InMemoryStore<Widget> CreateSystemUnderTest() => new();

    [Test]
    public async Task LongKeyedStore_Should_RoundTripEntity_When_AddedThenFetched()
    {
        InMemoryStore<Widget> store = CreateSystemUnderTest();
        var widget = new Widget(Flake.NewFlake(), "gadget");

        await store.AddAsync(widget);
        Widget? fetched = await store.GetByIdAsync(widget.Id);

        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.Name, Is.EqualTo("gadget"));
    }

    [Test]
    public async Task DeleteAsync_Should_HardRemove_When_EntityIsNotSoftDeletable()
    {
        InMemoryStore<Widget> store = CreateSystemUnderTest();
        long id = Flake.NewFlake();
        await store.AddAsync(new Widget(id, "original"));

        bool deleted = await store.DeleteAsync(id);

        Assert.That(deleted, Is.True);
        Assert.That(await store.GetByIdAsync(id), Is.Null);
        Assert.DoesNotThrowAsync(async () => await store.AddAsync(new Widget(id, "replacement")));
    }

    [Test]
    public async Task DeleteAsyncByEntity_Should_RemoveEntity_When_Present()
    {
        InMemoryStore<Widget> store = CreateSystemUnderTest();
        var widget = new Widget(Flake.NewFlake(), "disposable");
        await store.AddAsync(widget);

        bool deleted = await store.DeleteAsync(widget);

        Assert.That(deleted, Is.True);
        Assert.That(await store.GetByIdAsync(widget.Id), Is.Null);
    }

    [Test]
    public async Task AddAsync_Should_IsolateStoredState_When_CallerMutatesAfterAdd()
    {
        InMemoryStore<Widget> store = CreateSystemUnderTest();
        var widget = new Widget(Flake.NewFlake(), "original");
        await store.AddAsync(widget);

        widget.Name = "mutated after add";
        Widget? reloaded = await store.GetByIdAsync(widget.Id);

        Assert.That(reloaded!.Name, Is.EqualTo("original"));
    }

    [Test]
    public async Task GetByIdAsync_Should_ReturnIndependentInstances_When_CalledTwice()
    {
        InMemoryStore<Widget> store = CreateSystemUnderTest();
        var widget = new Widget(Flake.NewFlake(), "original");
        await store.AddAsync(widget);

        Widget first = (await store.GetByIdAsync(widget.Id))!;
        first.Name = "local edit";
        Widget second = (await store.GetByIdAsync(widget.Id))!;

        Assert.That(second.Name, Is.EqualTo("original"));
    }

    [Test]
    public async Task UpdateAsync_Should_IgnoreConcurrencyToken_When_EntityIsNotConcurrent()
    {
        var store = new InMemoryStore<PlainRecord, long>(r => r.Id);
        var record = new PlainRecord { Id = Flake.NewFlake(), Label = "before" };
        await store.AddAsync(record);

        var edit = new PlainRecord { Id = record.Id, Label = "after" };
        await store.UpdateAsync(edit);

        Assert.That((await store.GetByIdAsync(record.Id))!.Label, Is.EqualTo("after"));
    }
}
