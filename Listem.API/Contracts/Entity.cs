namespace Listem.API.Contracts;

public abstract class Entity
{
    protected Entity(string id) => Id = id;

    protected Entity() { }

    public string Id { get; init; }
}
