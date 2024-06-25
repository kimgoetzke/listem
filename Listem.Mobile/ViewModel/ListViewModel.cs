using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;
using Microsoft.Extensions.Logging;
using Serilog;
using StringProcessor = Listem.Mobile.Utilities.StringProcessor;

namespace Listem.Mobile.ViewModel;

public partial class ListViewModel : BaseViewModel
{
  [ObservableProperty]
  private List _currentList;

  [ObservableProperty]
  private IQueryable<Item> _items = null!;

  [ObservableProperty]
  private ObservableCollection<Item> _observableItems = [];

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

  public ListViewModel(List list, IServiceProvider serviceProvider)
    : base(serviceProvider.GetService<ILogger<ListViewModel>>()!)
  {
    IsBusy = true;
    _itemService = serviceProvider.GetService<IItemService>()!;
    _listService = serviceProvider.GetService<IListService>()!;
    _clipboardService = serviceProvider.GetService<IClipboardService>()!;
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
    var previousStage = IsBusy;
    IsBusy = true;
    Logger.Info("RemoveItem - START");
    await _itemService.DeleteAsync(item); // TODO: Find out why it crashes here with the above being the last log
    Logger.Info("RemoveItem: await _itemService.DeleteAsync(item) - DONE");
    var result = ItemsToDelete.Remove(item);
    Logger.Info("RemoveItem: ItemsToDelete.Remove(item) with result={Result} - DONE", result);
    GetSortedItems();
    _listHasChanged = true;
    IsBusy = previousStage;
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
    Logger.Info("Opening item: {Item}", item.ToLog());
    await Shell.Current.Navigation.PushModalAsync(new DetailPage(item));
  }

  [RelayCommand]
  private void CopyToClipboard()
  {
    _clipboardService.CopyToClipboard(Items.ToList(), Categories.ToList());
  }

  [RelayCommand]
  private async Task InsertFromClipboard()
  {
    await IsBusyWhile(async () =>
    {
      await _clipboardService.InsertFromClipboardAsync(Categories.ToList(), CurrentList);
      _listHasChanged = true;
      OnPropertyChanged(nameof(Items));
      GetSortedItems();
    });
  }

  [RelayCommand]
  private async Task GoBack()
  {
    await IsBusyWhile(async () =>
    {
      // Temporarily disabled due to https://www.mongodb.com/community/forums/t/realminvalidobjectexception-in-release-but-not-in-debug-mode/285108
      // TODO: Find a fix for the seemingly random RealmInvalidObjectException that occurs sometimes
      await DeleteSelectedItemsIfAny();

      if (_listHasChanged)
        await _listService.MarkAsUpdatedAsync(CurrentList);
    });
    await Shell.Current.Navigation.PopAsync();
  }

  private async Task DeleteSelectedItemsIfAny()
  {
    if (ItemsToDelete.Count == 0)
      return;

    Logger.Info("DeleteSelectedItemsIfAny: Removing selected item(s)");
    var listCopy = new List<Item>(ItemsToDelete);
    foreach (var item in listCopy)
    {
      try
      {
        if (item?.Id == null)
        {
          throw new InvalidOperationException("Attempted to remove item that is null");
        }

        var indexOfCurrentItem = listCopy.IndexOf(item) + 1;
        Logger.Info(
          "DeleteSelectedItemsIfAny: Processing item {Number}/{Total}...",
          indexOfCurrentItem,
          listCopy.Count
        );
        await RemoveItem(item);
        Logger.Info(
          "DeleteSelectedItemsIfAny: Item {Number}/{Total} removed",
          indexOfCurrentItem,
          listCopy.Count
        );
      }
      catch (Exception ex)
      {
        Logger.Warn("Failed to remove selected item(s): {Message}", ex.Message);
      }
    }
    Logger.Info("DeleteSelectedItemsIfAny: Removed all selected item(s)");

    ItemsToDelete.Clear();
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
    Logger.Info("GetSortedItems: Fetched sorted items - DONE");
    OnPropertyChanged(nameof(Items));
    Logger.Info("GetSortedItems: OnPropertyChanged(nameof(Items)) - DONE");
    UpdateObservableItems();
    Logger.Info("GetSortedItems: UpdateObservableItems() - DONE");
  }

  private void UpdateObservableItems()
  {
    ObservableItems.Clear();
    foreach (var list in Items)
    {
      ObservableItems.Add(list);
    }
  }
}
