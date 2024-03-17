using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Shared.Contracts;
using Listem.Shared.Enums;
using Listem.Shared.Utilities;

namespace Listem.Mobile.Models;

public partial class ObservableList : ObservableObject
{
    [ObservableProperty]
    private string? _id;

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

    public static ObservableList From(List list)
    {
        return new ObservableList
        {
            Id = list.Id,
            Name = list.Name,
            ListType = list.ListType,
            AddedOn = list.AddedOn,
            UpdatedOn = list.UpdatedOn
        };
    }

    public static ObservableList From(ListResponse listResponse)
    {
        return new ObservableList
        {
            Id = listResponse.Id,
            Name = listResponse.Name,
            ListType = listResponse.ListType,
            AddedOn = listResponse.AddedOn,
            UpdatedOn = listResponse.UpdatedOn
        };
    }

    public List ToItemList()
    {
        return new List
        {
            Id = IdProvider.NewId(nameof(List)),
            Name = Name,
            ListType = ListType,
            AddedOn = AddedOn,
            UpdatedOn = UpdatedOn
        };
    }

    public ListRequest ToListRequest()
    {
        return new ListRequest { Name = Name, ListType = ListType };
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
