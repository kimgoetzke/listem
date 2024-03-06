namespace Listem.API.Domain.Categories;

public class Category
{
    public string Id { get; init; } = null!;
    public string Name { get; set; } = null!;
    public string ListId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; set; }

    public override string ToString()
    {
        return $"[Category] '{Name}' {Id} in {ListId}, owned by: {OwnerId}";
    }
}
