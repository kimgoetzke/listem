using Listem.Models;

namespace Listem.Utilities;

public class ItemChangedDto(string listId, ObservableItem item)
{
    public string ListId { get; set; } = listId;
    public ObservableItem Item { get; set; } = item;
}
