namespace Auturge.Stores.Tests.TestObjects;

/// <summary> A lookup-style entity: <see cref="IStoredEntity{TKey}"/> only, with no concurrency, audit, or soft-delete. </summary>
public sealed class PlainRecord : IStoredEntity<long>
{
    public long Id { get; init; }
    public string Label { get; set; } = "";
}
