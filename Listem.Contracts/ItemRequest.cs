namespace Listem.Contracts;

public class ItemRequest
{
    public string Name { get; set; } = null!;
    public int Quantity { get; init; }
    public bool IsImportant { get; init; }
    public string CategoryId { get; set; } = null!;

    public override string ToString()
    {
        return $"[ItemRequest] '{Name}', quantity: {Quantity}, important: {IsImportant}, category: {CategoryId}";
    }
}
