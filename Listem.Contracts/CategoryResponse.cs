namespace Listem.Contracts;

public class CategoryResponse : Entity
{
    public string Name { get; init; } = null!;
    public string ListId { get; init; } = null!;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; init; }

    public override string ToString()
    {
        return $"[CategoryResponse] '{Name}' {Id} in {ListId}";
    }
}
