using Listem.API.Contracts;
using Listem.API.Utilities;

namespace Listem.API.Domain.Lists;

public class List
{
    public string Id { get; private init; } = null!;
    public string Name { get; set; } = null!;
    public ListType ListType { get; set; }
    public string OwnerId { get; private init; } = null!;
    public DateTime AddedOn { get; private init; }
    public DateTime UpdatedOn { get; set; }

    public static List From(ListRequest listRequest, string userId)
    {
        return new List
        {
            Id = IdProvider.NewId(nameof(ListRequest)),
            Name = listRequest.Name,
            ListType = listRequest.ListType,
            OwnerId = userId,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
    }

    public void Update(ListRequest listRequest)
    {
        Name = listRequest.Name;
        ListType = listRequest.ListType;
        UpdatedOn = DateTime.Now;
    }

    public ListResponse ToResponse()
    {
        return new ListResponse
        {
            Id = Id,
            Name = Name,
            ListType = ListType,
            AddedOn = AddedOn,
            UpdatedOn = UpdatedOn
        };
    }

    public override string ToString()
    {
        return $"[List] '{Name}' {Id}, type: '{ListType.ToString()}', last updated: {UpdatedOn}";
    }
}
