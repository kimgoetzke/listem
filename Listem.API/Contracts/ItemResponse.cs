using Listem.API.Domain.Items;

namespace Listem.API.Contracts;

public class ItemResponse
{
    public string Id { get; init; } = null!;
    public string Title { get; init; } = null!;
    public int Quantity { get; init; }
    public bool IsImportant { get; init; }
    public string ListId { get; init; } = null!;
    public string CategoryId { get; set; } = null!;
    public DateTime AddedOn { get; init; }
    public DateTime UpdatedOn { get; set; }

    public static ItemResponse FromItem(Item item)
    {
        return new ItemResponse
        {
            Id = item.Id,
            Title = item.Name,
            Quantity = item.Quantity,
            IsImportant = item.IsImportant,
            ListId = item.ListId,
            CategoryId = item.CategoryId,
            AddedOn = item.AddedOn,
            UpdatedOn = item.UpdatedOn
        };
    }

    public override string ToString()
    {
        return $"[Item] '{Title}' {Id} in {ListId}, category: {CategoryId}, quantity: {Quantity}, important: {IsImportant}";
    }
}
