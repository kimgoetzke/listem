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
    private ObservableCollection<ConfigurableStore> _stores = [];

    [ObservableProperty]
    private Item _item;

    [ObservableProperty]
    private ConfigurableStore _currentStore;

    private readonly IStoreService _storeService;
    private readonly IItemService _itemService;

    public DetailViewModel(Item item, IStoreService storeService, IItemService itemService)
    {
        Item = item;
        CurrentStore = new ConfigurableStore();
        _storeService = storeService;
        _itemService = itemService;
        SetStoreOptions();
    }

    [RelayCommand]
    private async Task GoBack()
    {
        Item.StoreName = CurrentStore.Name;
        await _itemService.CreateOrUpdateAsync(Item);
        Notifier.ShowToast($"Updated: {Item.Title}");
        await Shell.Current.GoToAsync("..", true);
    }

    private async void SetStoreOptions()
    {
        var loadedStores = await _storeService.GetAllAsync();
        Stores.Clear();
        foreach (var s in loadedStores)
        {
            Stores.Add(s);
            if (s.Name == Item.StoreName)
            {
                CurrentStore = s;
            }
        }
    }
}
