using SQLite;

namespace Listem.Models;

public class ItemList
{
    [PrimaryKey]
    public string Id { get; init; } = null!;
    public string Name { get; set; } = null!;
    public DateTime AddedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public static ItemList From(ObservableItemList observableItemList)
    {
        return new ItemList
        {
            Id = observableItemList.Id,
            Name = observableItemList.Name,
            AddedOn = observableItemList.AddedOn,
            UpdatedOn = observableItemList.UpdatedOn
        };
    }

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[ItemList] '{Name}' {Id}, last updated: {UpdatedOn}";
    }
}
