namespace Auturge.Stores.Tests.TestObjects;

/// <summary> A minimal entity that is <see cref="IConcurrentEntity"/> but not <see cref="ISoftDeletable"/> or <see cref="IAudit"/>. </summary>
public sealed class Widget(long? id = null, string name = "") : StoredEntity(id)
{
    public string Name { get; set; } = name;
}
