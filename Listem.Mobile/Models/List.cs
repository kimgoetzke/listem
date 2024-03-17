using Listem.Shared.Enums;
using SQLite;

namespace Listem.Mobile.Models;

public class List
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
        return $"[List] '{Name}' {Id}, type: '{ListType.ToString()}', last updated: {UpdatedOn}";
    }
}
