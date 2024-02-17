using SQLite;

namespace Listem.Models;

public class ItemList
{
    [PrimaryKey]
    public string Id { get; } = "L~" + Guid.NewGuid().ToString().Replace("-", "");
    public string Name { get; set; } = string.Empty;
    public List<Item> Items { get; set; } = [];
    public DateTime AddedOn { get; set; } = DateTime.Now;
    public DateTime UpdatedOn { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"{Name} #{Id} with {Items.Count} items";
    }
}
