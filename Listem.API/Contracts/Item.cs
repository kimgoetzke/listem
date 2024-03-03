namespace Listem.API.Contracts;

public class Item
{
    public string Id { get; init; } = null!;
    public string ListId { get; init; } = null!;
    public string Title { get; init; } = null!;
    public int Quantity { get; init; }
    public bool IsImportant { get; init; }
    public DateTime AddedOn { get; init; }
    public string CategoryName { get; set; } = null!;

    public override string ToString()
    {
        return Title;
    }

    public string ToLoggableString()
    {
        return $"[Item] {Title} {Id} in {ListId} (category: {CategoryName}, quantity: {Quantity}, important: {IsImportant})";
    }
}
