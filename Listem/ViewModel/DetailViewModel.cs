using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Utilities;
using Listem.Services;

namespace Listem.ViewModel;

[QueryProperty("Item", "Item")]
public partial class DetailViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Category> _stores = [];

    [ObservableProperty]
    private Item _item;

    [ObservableProperty]
    private Category _currentStore;

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;

    public DetailViewModel(Item item, ICategoryService categoryService, IItemService itemService)
    {
        Item = item;
        CurrentStore = new Category();
        _categoryService = categoryService;
        _itemService = itemService;
        SetStoreOptions();
    }

    [RelayCommand]
    private async Task GoBack()
    {
        Item.CategoryName = CurrentStore.Name;
        await _itemService.CreateOrUpdateAsync(Item);
        Notifier.ShowToast($"Updated: {Item.Title}");
        await Shell.Current.GoToAsync("..", true);
    }

    private async void SetStoreOptions()
    {
        var loadedStores = await _categoryService.GetAllAsync();
        Stores.Clear();
        foreach (var s in loadedStores)
        {
            Stores.Add(s);
            if (s.Name == Item.CategoryName)
            {
                CurrentStore = s;
            }
        }
    }
}
