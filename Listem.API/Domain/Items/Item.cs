namespace Listem.API.Domain.Items;

public class Item
{
    public string Id { get; init; } = null!;
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public bool IsImportant { get; set; }
    public string CategoryId { get; set; } = null!;
    public string ListId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; set; }

    public override string ToString()
    {
        return $"[Item] '{Name}' {Id} in {ListId}, category: {CategoryId}, quantity: {Quantity}, important: {IsImportant}";
    }
}
