namespace Listem.Shared.Contracts;

public class ItemResponse : Entity
{
    public string Title { get; init; } = null!;
    public int Quantity { get; init; }
    public bool IsImportant { get; init; }
    public string ListId { get; init; } = null!;
    public string CategoryId { get; set; } = null!;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; set; }

    public override string ToString()
    {
        return $"[ItemResponse] '{Title}' {Id} in {ListId}, category: {CategoryId}, quantity: {Quantity}, important: {IsImportant}";
    }
}
