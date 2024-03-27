using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using ListType = Listem.Mobile.Models.ListType;

namespace Listem.Mobile.ViewModel;

[QueryProperty(nameof(Item), nameof(Item))]
public partial class DetailViewModel : BaseViewModel
{
  [ObservableProperty]
  private Item _item;

  [ObservableProperty]
  private Category _currentCategory;

  [ObservableProperty]
  private IList<Category> _categories = [];

  public ListType ListType { get; }

  private readonly IItemService _itemService;

  public DetailViewModel(Item item, IServiceProvider serviceProvider)
  {
    _itemService = serviceProvider.GetService<IItemService>()!;
    Item = item;
    Categories = item.List!.Categories;
    CurrentCategory = Categories.First(c => c.Name == item.Category!.Name);
    ListType = Enum.TryParse(item.List!.ListType, out ListType type) ? type : ListType.Standard;
  }

  [RelayCommand]
  private async Task SaveAndBack()
  {
    if (CurrentCategory.Name == Item.Category?.Name)
    {
      await Shell.Current.Navigation.PopModalAsync();
      return;
    }

    await _itemService.UpdateAsync(Item, category: CurrentCategory);
    await Shell.Current.Navigation.PopModalAsync();
  }
}
