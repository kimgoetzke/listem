namespace Listem.API.Contracts;

public class Category
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string ListId { get; init; } = null!;

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[Category] {Name} {Id} in {ListId}";
    }
}
