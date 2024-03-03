using SQLite;

namespace Listem.Models;

public class ItemList
{
    [PrimaryKey]
    public string Id { get; init; } = null!;
    public string Name { get; set; } = null!;
    public ListType ListType { get; set; }
    public DateTime AddedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[ItemList] '{Name}' {Id}, type: '{ListType.ToString()}', last updated: {UpdatedOn}";
    }
}
