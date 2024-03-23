using Listem.Shared.Contracts;
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

    public static List FromListResponse(ListResponse listResponse)
    {
        return new List
        {
            Id = listResponse.Id,
            Name = listResponse.Name,
            ListType = listResponse.ListType,
            AddedOn = listResponse.AddedOn,
            UpdatedOn = listResponse.UpdatedOn
        };
    }

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[List] '{Name}' {Id}, type: '{ListType.ToString()}', last updated: {UpdatedOn}";
    }
}
