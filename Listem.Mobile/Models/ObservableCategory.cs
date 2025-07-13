using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.Models;

public partial class ObservableCategory(string listId) : ObservableObject
{
  [ObservableProperty]
  private string? _id;

  [ObservableProperty]
  private string _name = string.Empty;

  [ObservableProperty]
  private string _listId = listId;

  public static ObservableCategory From(Category category)
  {
    return new ObservableCategory(category.Name) { Id = category.Id, Name = category.Name };
  }

  public Category ToCategory()
  {
    if (Id is null or "")
    {
      Id = IdProvider.NewId(nameof(Category));
    }
    return new Category
    {
      Id = Id,
      Name = Name,
      ListId = ListId
    };
  }

  public override string ToString()
  {
    return Name;
  }

  public string ToLoggableString()
  {
    return $"[ObservableCategory] '{Name}' {Id} in {ListId}";
  }
}
