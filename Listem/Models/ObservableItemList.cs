using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Listem.Models;

public partial class ObservableItemList : ObservableObject
{
    [ObservableProperty]
    private string _id = "LST~" + Guid.NewGuid().ToString().Replace("-", "");

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ObservableItem> _items = [];

    [ObservableProperty]
    private DateTime _addedOn = DateTime.Now;

    [ObservableProperty]
    private DateTime _updatedOn = DateTime.Now;

    public static ObservableItemList From(ItemList itemList)
    {
        return new ObservableItemList
        {
            Id = itemList.Id,
            Name = itemList.Name,
            AddedOn = itemList.AddedOn,
            UpdatedOn = itemList.UpdatedOn
        };
    }

    public override string ToString()
    {
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[ObservableItemList] '{Name}' ({Id}) with {Items.Count} items";
    }
}
