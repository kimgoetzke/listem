using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Utilities;
using Listem.Services;

namespace Listem.ViewModel;

public partial class StoresViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ConfigurableStore> _stores;

    [ObservableProperty]
    private ConfigurableStore _newStore;
    private readonly IStoreService _storeService;
    private readonly IItemService _itemService;
    public static string DefaultStoreName => IStoreService.DefaultStoreName;

    public StoresViewModel(IStoreService storeService, IItemService itemService)
    {
        Stores = [];
        NewStore = new ConfigurableStore();
        _storeService = storeService;
        _itemService = itemService;
        LoadStoresFromDatabase()
            .SafeFireAndForget<Exception>(ex => Logger.Log($"Failed to load stores: {ex}"));
    }

    private async Task LoadStoresFromDatabase()
    {
        var loadedStores = await _storeService.GetAllAsync().ConfigureAwait(false);
        Stores = new ObservableCollection<ConfigurableStore>(loadedStores);
        OnPropertyChanged(nameof(IsCollectionViewLargerThanThreshold));
        Logger.Log($"Loaded {loadedStores.Count} items, new collection size: {Stores.Count}");
    }

    [RelayCommand]
    private async Task AddStore(ITextInput view)
    {
        // Don't add empty items
        if (string.IsNullOrWhiteSpace(NewStore.Name))
            return;

        // Pre-process store
        NewStore.Name = StringProcessor.TrimAndCapitaliseFirstChar(NewStore.Name);

        // Only allow unique store names
        if (Enumerable.Any<ConfigurableStore>(Stores, store => store.Name == NewStore.Name))
        {
            Notifier.ShowToast($"Cannot add '{NewStore.Name}' - it already exists");
            return;
        }

        // Add to list and database
        Stores.Add(NewStore);
        await _storeService.CreateOrUpdateAsync(NewStore);

        // Make sure the UI is reset/updated
#if __ANDROID__ 
        var isKeyboardHidden = view.HideKeyboardAsync(CancellationToken.None);
        Logger.Log("Keyboard hidden: " + isKeyboardHidden);
#endif
        Notifier.ShowToast($"Added: {NewStore.Name}");
        NewStore = new ConfigurableStore();
        OnPropertyChanged(nameof(Listem.ViewModel.StoresViewModel.NewStore));
        OnPropertyChanged(nameof(IsCollectionViewLargerThanThreshold));
    }

    [RelayCommand]
    private async Task RemoveStore(ConfigurableStore s)
    {
        if (s.Name == IStoreService.DefaultStoreName)
        {
            Notifier.ShowToast("Cannot remove default store");
            return;
        }

        Stores.Remove(s);
        await _itemService.UpdateAllUsingStoreAsync(s.Name);
        await _storeService.DeleteAsync(s);
        OnPropertyChanged(nameof(IsCollectionViewLargerThanThreshold));
        Notifier.ShowToast($"Removed: {s.Name}");
    }

    [RelayCommand]
    private async Task ResetStores()
    {
        if (!await IsRequestConfirmedByUser())
            return;

        Notifier.ShowToast("Reset stores");
        await _itemService.UpdateAllToDefaultStoreAsync().ConfigureAwait(false);
        await _storeService.DeleteAllAsync().ConfigureAwait(false);
        await LoadStoresFromDatabase().ConfigureAwait(false);
    }

    private static Task<bool> IsRequestConfirmedByUser()
    {
        return Shell.Current.DisplayAlert(
            "Reset stores",
            $"This will remove all stores, except the 'Anywhere' store. Are you sure you want to continue?",
            "Yes",
            "No"
        );
    }

    [RelayCommand]
    private static async Task GoBack()
    {
        await Shell.Current.GoToAsync("..", true);
    }

    // Used to toggle on/off the line separator between stores list and buttons
    public bool IsCollectionViewLargerThanThreshold
    {
        get
        {
            const int itemHeight = 45; // As defined in Styles.XAML
            var currentHeight = Stores.Count * itemHeight;
            return currentHeight >= 270;
        }
    }
}
