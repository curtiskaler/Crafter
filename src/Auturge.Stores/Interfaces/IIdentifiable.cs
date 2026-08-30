namespace Auturge.Stores;

public interface IIdentifiable<TKey> : IEntity where TKey : notnull
{
    TKey Id { get; init; }
}
