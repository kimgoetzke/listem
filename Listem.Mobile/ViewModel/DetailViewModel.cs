using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
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
  private ObservableCollection<ObservableCategory> _categories = [];

  [ObservableProperty]
  private ObservableCategory _currentCategory;

  [ObservableProperty]
  private ObservableItem _item;

  public ListType ListType { get; }

  private readonly IItemService _itemService;
  private readonly ICategoryService _categoryService;

  public DetailViewModel(ObservableItem item, ObservableList list, IServiceProvider sp)
    : base(sp.GetService<ILogger<DetailViewModel>>()!)
  {
    _itemService = sp.GetService<IItemService>()!;
    _categoryService = sp.GetService<ICategoryService>()!;
    Item = item;
    ListType = list.ListType;
    CurrentCategory = new ObservableCategory(item.ListId);
    SetCategories();
  }

  [RelayCommand]
  private async Task SaveAndBack()
  {
    Item.CategoryName = CurrentCategory.Name;
    await _itemService.CreateOrUpdateAsync(Item);
    Back().SafeFireAndForget();
  }

  [RelayCommand]
  private static async Task Back()
  {
    await Shell.Current.Navigation.PopModalAsync();
  }

  private async void SetCategories()
  {
    var loaded = await _categoryService.GetAllByListIdAsync(Item.ListId);
    Categories.Clear();
    foreach (var category in loaded)
    {
      Categories.Add(category);
      if (category.Name == Item.CategoryName)
      {
        CurrentCategory = category;
      }
    }
  }
}
