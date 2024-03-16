using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Contracts;

namespace Listem.Models;

public partial class ObservableItemList : ObservableObject
{
    [ObservableProperty]
    private string _id = "LST~" + Guid.NewGuid().ToString().Replace("-", "");

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ListType _listType = ListType.Standard;

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
            ListType = itemList.ListType,
            AddedOn = itemList.AddedOn,
            UpdatedOn = itemList.UpdatedOn
        };
    }

    public static ObservableItemList From(ListResponse listResponse)
    {
        return new ObservableItemList
        {
            Id = listResponse.Id,
            Name = listResponse.Name,
            ListType = listResponse.ListType,
            AddedOn = listResponse.AddedOn,
            UpdatedOn = listResponse.UpdatedOn
        };
    }

    public ItemList ToItemList()
    {
        return new ItemList
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
        return Name;
    }

    public string ToLoggableString()
    {
        return $"[ObservableItemList] '{Name}' ({Id}) of type '{ListType}' with {Items.Count} items";
    }
}
