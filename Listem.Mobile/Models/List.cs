using Listem.Mobile.Services;
using MongoDB.Bson;
using Realms;

namespace Listem.Mobile.Models;

public partial class List : IRealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("owner_id")]
    // [Required]
    public string OwnerId { get; set; }

    [MapTo("name")]
    // [Required]
    public string Name { get; set; } = null!;

    [MapTo("list_type")]
    // [Required]
    public string ListType { get; set; }

    [MapTo("added_on")]
    public DateTimeOffset AddedOn { get; set; }

    [MapTo("updated_on")]
    public DateTimeOffset UpdatedOn { get; set; }
    public bool IsMine => OwnerId == RealmService.User.Id;

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[List] '{Name}' {Id}, type: '{ListType.ToString()}', last updated: {UpdatedOn}";
    }
}
