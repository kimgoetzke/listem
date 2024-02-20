using SQLite;

namespace Listem.Models;

public class Item
{
    [PrimaryKey]
    public string Id { get; init; } = null!;
    public string ListId { get; init; } = null!;
    public string Title { get; init; } = null!;
    public int Quantity { get; init; }
    public bool IsImportant { get; init; }
    public DateTime AddedOn { get; init; }
    public string CategoryName { get; set; } = null!;

    public static Item From(ObservableItem observableItem)
    {
        return new Item
        {
            Id = observableItem.Id,
            ListId = observableItem.ListId,
            Title = observableItem.Title,
            Quantity = observableItem.Quantity,
            IsImportant = observableItem.IsImportant,
            CategoryName = observableItem.CategoryName,
            AddedOn = observableItem.AddedOn
        };
    }

    public override string ToString()
    {
        return Title;
    }

    public string ToLoggableString()
    {
        return $"[Item] {Title} {Id} in {ListId} (category: {CategoryName}, quantity: {Quantity}, important: {IsImportant})";
    }
}
