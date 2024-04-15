using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Microsoft.Extensions.Logging;
using ListType = Listem.Mobile.Models.ListType;

namespace Listem.Mobile.ViewModel;

[QueryProperty(nameof(Item), nameof(Item))]
public partial class DetailViewModel : BaseViewModel
{
  [ObservableProperty]
  private Item _item;

  [ObservableProperty]
  private string _itemName;

  [ObservableProperty]
  private bool _itemIsImportant;

  [ObservableProperty]
  private int _itemQuantity;

  [ObservableProperty]
  private Category _currentCategory;

  [ObservableProperty]
  private IList<Category> _categories = [];

  public ListType ListType { get; }

  private readonly IItemService _itemService;

  public DetailViewModel(Item item, IServiceProvider serviceProvider)
    : base(serviceProvider.GetService<ILogger<DetailViewModel>>()!)
  {
    _itemService = serviceProvider.GetService<IItemService>()!;
    Item = item;
    ItemName = item.Name;
    ItemIsImportant = item.IsImportant;
    ItemQuantity = item.Quantity;
    Categories = item.List!.Categories;
    CurrentCategory = Categories.First(c => c.Name == item.Category!.Name);
    ListType = Enum.TryParse(item.List!.ListType, out ListType type) ? type : ListType.Standard;
  }

  [RelayCommand]
  private async Task SaveAndBack()
  {
    ItemName = ItemName.Trim();
    if (
      CurrentCategory.Name == Item.Category?.Name
      && ItemName == Item.Name
      && ItemQuantity == Item.Quantity
      && ItemIsImportant == Item.IsImportant
    )
    {
      await Shell.Current.Navigation.PopModalAsync();
      return;
    }

    await _itemService.UpdateAsync(
      Item,
      name: ItemName,
      category: CurrentCategory,
      quantity: ItemQuantity,
      isImportant: ItemIsImportant
    );
    await Shell.Current.Navigation.PopModalAsync();
  }
}
