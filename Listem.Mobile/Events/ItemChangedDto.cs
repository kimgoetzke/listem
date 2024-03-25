using Listem.Mobile.Models;
using MongoDB.Bson;

namespace Listem.Mobile.Events;

public class ItemChangedDto(ObjectId listId, Item item)
{
    public ObjectId ListId { get; set; } = listId;
    public Item Item { get; set; } = item;
}
