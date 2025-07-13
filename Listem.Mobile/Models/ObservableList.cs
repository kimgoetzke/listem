using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Mobile.Utilities;

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

  public List ToItemList()
  {
    if (Id is null or "")
    {
      Id = IdProvider.NewId(nameof(List));
    }
    return new List
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
    return $"[ObservableItemList] '{Name}' {Id} of type '{ListType}' with {Items.Count} items";
  }
}
