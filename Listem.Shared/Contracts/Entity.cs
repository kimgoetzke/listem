namespace Listem.Shared.Contracts;

public abstract class Entity
{
    protected Entity() { }

    protected Entity(string id) => Id = id;

    public required string Id { get; init; }
}
