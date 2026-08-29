using Auturge.Stores;

namespace Crafter.Model;

public class Component(long? id, string name, string? description = null) : StoredEntity(id), IDisplayEntity
{
    public string DisplayName { get; } = name;
    public string Description { get; } = description ?? string.Empty;
}
