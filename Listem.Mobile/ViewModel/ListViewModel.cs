using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;
using Microsoft.Extensions.Logging;
using StringProcessor = Listem.Mobile.Utilities.StringProcessor;

namespace Listem.Mobile.ViewModel;

public partial class ListViewModel : BaseViewModel
{
  [ObservableProperty]
  private List _currentList;

  [ObservableProperty]
  private IQueryable<Item> _items = null!;

  [ObservableProperty]
  private List<Item> _itemsToDelete;

  [ObservableProperty]
  private IList<Category> _categories;

  [ObservableProperty]
  private string _newItemName = string.Empty;

  [ObservableProperty]
  private int _newItemQuantity = 1;

  [ObservableProperty]
  private bool _newItemIsImportant;

  [ObservableProperty]
  private Category _currentCategory;

  private bool _listHasChanged;
  private readonly IItemService _itemService;
  private readonly IListService _listService;
  private readonly IClipboardService _clipboardService;
  private readonly ILogger<ListViewModel> _logger;

  public ListViewModel(List list, IServiceProvider serviceProvider)
  {
    IsBusy = true;
    _itemService = serviceProvider.GetService<IItemService>()!;
    _listService = serviceProvider.GetService<IListService>()!;
    _clipboardService = serviceProvider.GetService<IClipboardService>()!;
    _logger = serviceProvider.GetService<ILogger<ListViewModel>>()!;
    CurrentList = list;
    ItemsToDelete = [];
    Categories = CurrentList.Categories;
    CurrentCategory = Categories.First(c => c.Name == Constants.DefaultCategoryName);
    GetSortedItems();
    IsBusy = false;
  }

  [RelayCommand]
  private async Task AddItem()
  {
    if (string.IsNullOrWhiteSpace(NewItemName))
      return;

    var newItem = new Item
    {
      Name = StringProcessor.TrimAndCapitalise(NewItemName),
      List = CurrentList,
      Category = new Category { Name = CurrentCategory.Name },
      Quantity = NewItemQuantity,
      IsImportant = NewItemIsImportant,
      UpdatedOn = DateTime.Now,
      OwnedBy = CurrentList.OwnedBy
    };
    foreach (var id in CurrentList.SharedWith)
    {
      newItem.SharedWith.Add(id);
    }

    await _itemService.CreateAsync(newItem);
    Notifier.ShowToast($"Added: {newItem.Name}");
    _listHasChanged = true;
    NewItemName = string.Empty;
    NewItemQuantity = 1;
    NewItemIsImportant = false;
    GetSortedItems();
  }

  [RelayCommand]
  private async Task RemoveItem(Item item)
  {
    IsBusy = true;
    await _itemService.DeleteAsync(item);
    _listHasChanged = true;
    IsBusy = false;
  }

  [RelayCommand]
  private async Task TogglePriority(Item item)
  {
    await _itemService.UpdateAsync(item, isImportant: !item.IsImportant);
    _listHasChanged = true;
    GetSortedItems();
  }

  [RelayCommand]
  private async Task TapItem(Item item)
  {
    _logger.Info("Opening item: {Item}", item.ToLog());
    await Shell.Current.Navigation.PushModalAsync(new DetailPage(item));
  }

  [RelayCommand]
  private void CopyToClipboard()
  {
    _clipboardService.CopyToClipboard(Items.ToList(), Categories.ToList());
  }

  [RelayCommand]
  private void InsertFromClipboard()
  {
    _clipboardService.InsertFromClipboardAsync(Items.ToList(), Categories.ToList(), CurrentList);
    _listHasChanged = true;
    GetSortedItems();
  }

  [RelayCommand]
  private async Task GoBack()
  {
    await DeleteSelectedItemsIfAny();

    if (_listHasChanged)
      await _listService.MarkAsUpdatedAsync(CurrentList);

    await Shell.Current.Navigation.PopAsync();
  }

  private async Task DeleteSelectedItemsIfAny()
  {
    if (ItemsToDelete.Count == 0)
      return;

    _logger.Debug("Removing selected item(s)");
    foreach (var item in ItemsToDelete)
    {
      await RemoveItemCommand.ExecuteAsync(item);
    }
    _listHasChanged = true;
  }

  public string ProcessedObservableItemListName
  {
    get
    {
      if (CurrentList.Name.Length > 15)
      {
        return CurrentList.Name[..15].Trim() + "...";
      }

      return CurrentList.Name;
    }
  }

  // TODO: Implement custom sorting and filtering using locally stored preferences
  // Note that listing default category items first or last may be difficult, see:
  // https://www.mongodb.com/community/forums/t/c-linq-condition-inside-orderby/149393
  public void GetSortedItems()
  {
    Items = RealmService
      .GetMainThreadRealm()
      .All<Item>()
      .Where(i => i.List == CurrentList)
      .OrderBy(i => i.Category!.Name)
      .ThenByDescending(i => i.UpdatedOn);
  }
}
