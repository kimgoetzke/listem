﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Microsoft.Extensions.Logging;
using ListType = Listem.Mobile.Models.ListType;

namespace Listem.Mobile.ViewModel;

public partial class EditListViewModel : BaseViewModel
{
  [ObservableProperty]
  private ObservableList _observableList;

  [ObservableProperty]
  private ObservableCollection<ListType> _listTypes = [];

  [ObservableProperty]
  private ObservableCollection<ObservableItem> _items = [];

  [ObservableProperty]
  private ObservableCategory _newObservableCategory;

  [ObservableProperty]
  private ObservableCollection<string> _categoryNames = [];

  private ObservableCollection<ObservableCategory> _categories = [];

  public ObservableCollection<ObservableCategory> Categories
  {
    get => _categories;
    set
    {
      _categories.CollectionChanged -= Categories_CollectionChanged;
      _categories = value;
      OnPropertyChanged();
      _categories.CollectionChanged += Categories_CollectionChanged;
      HasCustomCategories = _categories.Count > 1;
      OnPropertyChanged(nameof(HasCustomCategories));
    }
  }

  [ObservableProperty]
  private bool _hasCustomCategories;

  private bool _listHasChanged;
  private readonly string _originalListName;
  private readonly ListType _originalListType;
  private readonly ICategoryService _categoryService;
  private readonly IItemService _itemService;
  private readonly IListService _listService;

  public EditListViewModel(ObservableList list, IServiceProvider serviceProvider)
    : base(serviceProvider.GetService<ILogger<EditListViewModel>>()!)
  {
    _listService = serviceProvider.GetService<IListService>()!;
    _categoryService = serviceProvider.GetService<ICategoryService>()!;
    _itemService = serviceProvider.GetService<IItemService>()!;
    ObservableList = list;
    _originalListName = list.Name;
    _originalListType = list.ListType;
    ListTypes = new ObservableCollection<ListType>(
      Enum.GetValues(typeof(ListType)).Cast<ListType>()
    );
    Items = new ObservableCollection<ObservableItem>(list.Items);
    NewObservableCategory = new ObservableCategory(list.Id!);
    LoadCategories().SafeFireAndForget();
  }

  private async Task LoadCategories()
  {
    var categories = await _categoryService.GetAllByListIdAsync(ObservableList.Id!);
    Categories = new ObservableCollection<ObservableCategory>(categories);
    CategoryNames = new ObservableCollection<string>(categories.Select(c => c.Name));
    Logger.Info(
      "Loaded {Count} {HasCustomCategories} for list {ID} from database",
      Categories.Count,
      HasCustomCategories ? "categories" : "category",
      ObservableList.Id
    );
  }

  private void Categories_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    HasCustomCategories = Categories.Count > 1;
    OnPropertyChanged(nameof(HasCustomCategories));
    _listHasChanged = true;
  }

  [RelayCommand]
  private async Task AddCategory(string categoryName)
  {
    if (string.IsNullOrWhiteSpace(categoryName))
      return;

    // Pre-process
    NewObservableCategory.Name = StringProcessor.TrimAndCapitalise(categoryName);
    if (Categories.Any(category => category.Name == NewObservableCategory.Name))
    {
      Notifier.ShowToast($"Cannot add '{NewObservableCategory.Name}' - it already exists");
      return;
    }

    // Add to list and database
    Categories.Add(NewObservableCategory);
    CategoryNames.Add(NewObservableCategory.Name);
    await _categoryService.CreateOrUpdateAsync(NewObservableCategory);

    // Update/reset UI
    Notifier.ShowToast($"Added: {NewObservableCategory.Name}");
    NewObservableCategory = new ObservableCategory(ObservableList.Id!);
    OnPropertyChanged(nameof(NewObservableCategory));
    OnPropertyChanged(nameof(Categories));
  }

  [RelayCommand]
  private async Task RemoveCategory(string categoryName)
  {
    if (categoryName == Constants.DefaultCategoryName)
    {
      Notifier.ShowToast("Cannot remove default category");
      return;
    }

    var observableCategory = Categories.FirstOrDefault(c => c.Name == categoryName);
    if (observableCategory == null)
    {
      Logger.Error("Cannot remove category {Name} because it doesn't seem to exist", categoryName);
      return;
    }

    Categories.Remove(observableCategory);
    CategoryNames.Remove(observableCategory.Name);
    await _itemService.UpdateAllToCategoryAsync(observableCategory.Name, ObservableList.Id!);
    await _categoryService.DeleteAsync(observableCategory);
    var message = $"Removed: {observableCategory}";
    OnPropertyChanged(nameof(Categories));
    OnPropertyChanged(nameof(CategoryNames));
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

    Notifier.ShowToast("Reset categories");
    await _itemService.UpdateAllToDefaultCategoryAsync(ObservableList.Id!).ConfigureAwait(false);
    await _categoryService.DeleteAllByListIdAsync(ObservableList.Id!).ConfigureAwait(false);
    await LoadCategories().ConfigureAwait(false);
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

    Items.Clear();
    await _itemService.DeleteAllByListIdAsync(ObservableList.Id!);
    _listHasChanged = true;
    Notifier.ShowToast("Removed all items from list");
  }

  [RelayCommand]
  private async Task SaveAndBack()
  {
    if (_originalListName != ObservableList.Name || _originalListType != ObservableList.ListType)
    {
      Logger.Info("Name or type of list has changed, marking list as modified");
      _listHasChanged = true;
    }

    if (_listHasChanged)
    {
      await _listService.MarkAsUpdatedAsync(ObservableList);
      WeakReferenceMessenger.Default.Send(new ListModifiedMessage(ObservableList));
    }

    Back().SafeFireAndForget();
  }

  [RelayCommand]
  private static async Task Back()
  {
    await Shell.Current.Navigation.PopModalAsync();
  }
}
