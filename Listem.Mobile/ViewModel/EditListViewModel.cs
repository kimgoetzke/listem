using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using ListType = Listem.Mobile.Models.ListType;

namespace Listem.Mobile.ViewModel;

public partial class EditListViewModel : BaseViewModel
{
  [ObservableProperty]
  private List _list;

  [ObservableProperty]
  private ObservableCollection<string> _categories;

  [ObservableProperty]
  private string _currentName;

  [ObservableProperty]
  private ListType _currentListType;

  [ObservableProperty]
  private ObservableCollection<ListType> _listTypes = [];

  public bool IsShared => List.SharedWith.Any();
  public bool HasCustomCategories => List.Categories.Count > 1;

  private readonly ICategoryService _categoryService;
  private readonly IItemService _itemService;
  private readonly IListService _listService;

  public EditListViewModel(List list, IServiceProvider serviceProvider)
  {
    _listService = serviceProvider.GetService<IListService>()!;
    _categoryService = serviceProvider.GetService<ICategoryService>()!;
    _itemService = serviceProvider.GetService<IItemService>()!;
    List = list;
    Categories = new ObservableCollection<string>(List.Categories.Select(c => c.Name));
    ListTypes = new ObservableCollection<ListType>(
      Enum.GetValues(typeof(ListType)).Cast<ListType>()
    );
    CurrentListType = Enum.TryParse(list.ListType, out ListType type) ? type : ListType.Standard;
    CurrentName = list.Name;
  }

  [RelayCommand]
  private async Task AddCategory(string categoryName)
  {
    if (string.IsNullOrWhiteSpace(categoryName))
      return;

    var category = new Category { Name = StringProcessor.TrimAndCapitalise(categoryName) };
    if (List.Categories.Any(c => c.Name == category.Name))
    {
      Notifier.ShowToast($"Cannot add '{category.Name}' - it already exists");
      return;
    }
    await _categoryService.CreateAsync(category, List);
    Categories.Add(category.Name);
    Notifier.ShowToast($"Added: {category.Name}");
  }

  [RelayCommand]
  private async Task RemoveCategory(string categoryName)
  {
    if (categoryName == Constants.DefaultCategoryName)
    {
      Notifier.ShowToast("Cannot remove default category");
      return;
    }

    var message = $"Removed: {categoryName}";
    var toDelete = List.Categories.First(c => c.Name == categoryName);
    await _itemService.ResetSelectedToDefaultCategoryAsync(List, category: toDelete);
    await _categoryService.DeleteAsync(toDelete);
    Categories.Remove(categoryName);
    Notifier.ShowToast(message);
  }

  [RelayCommand]
  private async Task ResetCategories()
  {
    if (
      !await Notifier.ShowConfirmationAlertAsync(
        "Reset categories",
        "This will remove all categories. Are you sure you want to continue?"
      )
    )
      return;

    await _itemService.ResetAllToDefaultCategoryAsync(List);
    await _categoryService.ResetAsync(List);
    Categories.Clear();
    Notifier.ShowToast("Reset categories");
  }

  [RelayCommand]
  private async Task Share(string userName)
  {
    if (string.IsNullOrWhiteSpace(userName))
      return;

    await _listService.ShareWith(List, userName);
    Notifier.ShowToast($"Shared list with: {userName}");
  }

  [RelayCommand]
  private async Task RevokeAccess(string userName)
  {
    if (string.IsNullOrWhiteSpace(userName))
      return;

    await _listService.RevokeAccess(List, userName);
    Notifier.ShowToast($"Revoked access of: {userName}");
  }

  [RelayCommand]
  private async Task MakePrivate()
  {
    if (
      !await Notifier.ShowConfirmationAlertAsync(
        "Make private",
        "This will remove all other collaborators from this list. Are you sure you want to continue?"
      )
    )
      return;

    await _listService.UpdateAsync(List, sharedWith: new HashSet<string>());
    Notifier.ShowToast("List is now private");
  }

  [RelayCommand]
  private async Task RemoveAllItems()
  {
    if (
      !await Notifier.ShowConfirmationAlertAsync(
        "Clear list",
        "This will remove all items from your list. Are you sure you want to continue?"
      )
    )
      return;

    await _itemService.DeleteAllInListAsync(List);
    Notifier.ShowToast("Removed all items from list");
  }

  [RelayCommand]
  private async Task SaveAndBack()
  {
    var name = StringProcessor.TrimAndCapitalise(CurrentName);
    if (name == List.Name && CurrentListType.ToString() == List.ListType)
    {
      await Shell.Current.Navigation.PopModalAsync();
      return;
    }

    await _listService.UpdateAsync(List, name: name, listType: CurrentListType.ToString());
    await Shell.Current.Navigation.PopModalAsync();
  }
}
