using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
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
  private ObservableList _observableList;

  [ObservableProperty]
  private ObservableItem _newObservableItem;

  [ObservableProperty]
  private ObservableCollection<ObservableItem> _items = null!;

  [ObservableProperty]
  private List<ObservableItem> _itemsToDelete;

  [ObservableProperty]
  private ObservableCollection<ObservableCategory> _categories = null!;

  [ObservableProperty]
  private ObservableCategory? _currentCategory;

  private bool _listHasChanged;
  private readonly IItemService _itemService;
  private readonly IListService _listService;
  private readonly ICategoryService _categoryService;
  private readonly IClipboardService _clipboardService;

  public ListViewModel(ObservableList observableList, IServiceProvider serviceProvider)
    : base(serviceProvider.GetService<ILogger<ListViewModel>>()!)
  {
    IsBusy = true;
    _itemService = serviceProvider.GetService<IItemService>()!;
    _listService = serviceProvider.GetService<IListService>()!;
    _categoryService = serviceProvider.GetService<ICategoryService>()!;
    _clipboardService = serviceProvider.GetService<IClipboardService>()!;
    ObservableList = observableList;
    ItemsToDelete = [];
    Items = new ObservableCollection<ObservableItem>(observableList.Items);
    NewObservableItem = new ObservableItem(ObservableList.Id!);
    SortItems();
    LoadCategories().SafeFireAndForget();
    IsBusy = false;
  }

  private async Task LoadCategories()
  {
    var categories = await _categoryService.GetAllByListIdAsync(ObservableList.Id!);
    Categories = new ObservableCollection<ObservableCategory>(categories);
    CurrentCategory = Categories.FirstOrDefault(c => c.Name == Constants.DefaultCategoryName);
    OnPropertyChanged(nameof(Categories));
    OnPropertyChanged(nameof(CurrentCategory));
    Logger.Info(
      "Loaded {Count} categories for list {ListID} from the database",
      Categories.Count,
      ObservableList.Id
    );
  }

  [RelayCommand]
  private async Task AddItem()
  {
    if (string.IsNullOrWhiteSpace(NewObservableItem.Title))
      return;

    // Pre-process item
    NewObservableItem.Title = StringProcessor.TrimAndCapitalise(NewObservableItem.Title);
    NewObservableItem.CategoryName =
      CurrentCategory != null ? CurrentCategory.Name : Constants.DefaultCategoryName;

    // Add to list and database
    Logger.Info("Adding item: {Item}", NewObservableItem.ToLoggableString());
    await _itemService.CreateOrUpdateAsync(NewObservableItem);
    var value = new ItemChangedDto(ObservableList.Id!, NewObservableItem);
    WeakReferenceMessenger.Default.Send(new ItemAddedToListMessage(value));
    Items.Add(NewObservableItem);
    Notifier.ShowToast($"Added: {NewObservableItem.Title}");

    // Make sure the UI is reset/updated
    _listHasChanged = true;
    NewObservableItem = new ObservableItem(ObservableList.Id!);
    SortItems();
    OnPropertyChanged(nameof(NewObservableItem));
  }

  [RelayCommand]
  private async Task RemoveItem(ObservableItem i)
  {
    var previousStage = IsBusy;
    IsBusy = true;
    await _itemService.DeleteAsync(i);
    ObservableList.Items.Remove(i);
    Items.Remove(i);
    var value = new ItemChangedDto(ObservableList.Id!, i);
    WeakReferenceMessenger.Default.Send(new ItemRemovedFromListMessage(value));
    OnPropertyChanged(nameof(ObservableList));
    _listHasChanged = true;
    IsBusy = previousStage;
  }

  [RelayCommand]
  private async Task TogglePriority(ObservableItem i)
  {
    i.IsImportant = !i.IsImportant;
    await _itemService.CreateOrUpdateAsync(i);
    SortItems();
  }

  [RelayCommand]
  private async Task TapItem(ObservableItem item)
  {
    Logger.Info("Opening item: {Item}", item.ToLoggableString());
    await Shell.Current.Navigation.PushModalAsync(new DetailPage(item, ObservableList));
  }

  [RelayCommand]
  private void CopyToClipboard()
  {
    _clipboardService.CopyToClipboard(Items, Categories);
  }

  [RelayCommand]
  private async Task InsertFromClipboard()
  {
    await IsBusyWhile(async () =>
    {
      await _clipboardService.InsertFromClipboardAsync(Items, Categories, ObservableList.Id!);
      _listHasChanged = true;
      SortItems();
    });
  }

  [RelayCommand]
  private async Task GoBack()
  {
    await IsBusyWhile(async () =>
    {
      await DeleteSelectedItemsIfAny();

      if (_listHasChanged)
        await _listService.MarkAsUpdatedAsync(ObservableList);
    });
    await Shell.Current.Navigation.PopAsync();
  }

  private async Task DeleteSelectedItemsIfAny()
  {
    if (ItemsToDelete.Count == 0)
      return;

    foreach (var item in new List<ObservableItem>(ItemsToDelete))
    {
      await RemoveItem(item);
    }

    ItemsToDelete.Clear();
    _listHasChanged = true;
  }

  public string ProcessedObservableItemListName
  {
    get
    {
      if (ObservableList.Name.Length > 15)
      {
        return ObservableList.Name[..15].Trim() + "...";
      }

      return ObservableList.Name;
    }
  }

  public void SortItems()
  {
    Items = new ObservableCollection<ObservableItem>(
      Items.OrderBy(i => i.CategoryName).ThenByDescending(i => i.AddedOn)
    );
    OnPropertyChanged(nameof(Items));
  }
}
