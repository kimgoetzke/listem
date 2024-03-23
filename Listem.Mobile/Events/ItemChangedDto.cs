using Listem.Mobile.Models;

namespace Listem.Mobile.Events;

public class ItemChangedDto(string listId, ObservableItem item)
{
    public string ListId { get; set; } = listId;
    public ObservableItem Item { get; set; } = item;
}
