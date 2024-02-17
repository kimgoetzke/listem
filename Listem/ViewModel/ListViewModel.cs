using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;
using Listem.Views;
using static Listem.Services.IService;
using StringProcessor = Listem.Utilities.StringProcessor;

namespace Listem.ViewModel;

public partial class ListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    [ObservableProperty]
    private ObservableCollection<ConfigurableStore> _stores = [];

    [ObservableProperty]
    private ItemList _itemList;

    [ObservableProperty]
    private Item _newItem;

    [ObservableProperty]
    private ConfigurableStore? _currentStore;

    [ObservableProperty]
    private ObservableCollection<Theme> _themes = [];

    [ObservableProperty]
    private Theme _currentTheme;

    private readonly IStoreService _storeService;
    private readonly IItemService _itemService;
    private readonly IClipboardService _clipboardService;

    public ListViewModel(List<IService> services, ItemList itemList)
    {
        _storeService = (services.First(s => s.Type == ServiceType.Store) as IStoreService)!;
        _itemService = (services.First(s => s.Type == ServiceType.Item) as IItemService)!;
        _clipboardService = (
            services.First(s => s.Type == ServiceType.Clipboard) as IClipboardService
        )!;
        ItemList = itemList;
        NewItem = new Item();
        Themes = Settings.GetAllThemesAsCollection();
        CurrentTheme = Themes.First(t => t.Name == Settings.CurrentTheme);
    }

    [RelayCommand]
    private async Task AddItem()
    {
        Logger.Log($"Adding item: {NewItem.ToLoggableString()}");

        // Don't add empty items
        if (string.IsNullOrWhiteSpace(NewItem.Title))
            return;

        // Pre-process item
        NewItem.Title = StringProcessor.TrimAndCapitaliseFirstChar(NewItem.Title);
        NewItem.StoreName =
            CurrentStore != null ? CurrentStore.Name : IStoreService.DefaultStoreName;

        // Add to list and database
        await _itemService.CreateOrUpdateAsync(NewItem);
        await Application.Current!.Dispatcher.DispatchAsync(() => Items.Add(NewItem));
        Notifier.ShowToast($"Added: {NewItem.Title}");
        Logger.Log($"Added item: {NewItem.ToLoggableString()}");

        // Make sure the UI is reset/updated
        NewItem = new Item();
        SortItems();
        OnPropertyChanged(nameof(NewItem));
    }

    [RelayCommand]
    public async Task RemoveItem(Item i)
    {
        await Application.Current!.Dispatcher.DispatchAsync(() => Items.Remove(i));
        await _itemService.DeleteAsync(i);
    }

    [RelayCommand]
    private async Task RemoveAllItems()
    {
        if (!await IsRequestConfirmedByUser())
            return;
        Items.Clear();
        await _itemService.DeleteAllAsync();
        Notifier.ShowToast("Removed all items from list");
    }

    private static async Task<bool> IsRequestConfirmedByUser()
    {
        return await Shell.Current.DisplayAlert(
            "Clear list",
            $"This will remove all items from your list. Are you sure you want to continue?",
            "Yes",
            "No"
        );
    }

    [RelayCommand]
    private async Task TogglePriority(Item i)
    {
        i.IsImportant = !i.IsImportant;
        await _itemService.CreateOrUpdateAsync(i);
        SortItems();
    }

    [RelayCommand]
    private static async Task TapItem(Item i)
    {
        await Shell.Current.Navigation.PushAsync(new DetailPage(i));
    }

    [RelayCommand]
    private async Task ManageStores()
    {
        await Shell.Current.GoToAsync(nameof(StoresPage), true);
    }

    [RelayCommand]
    internal void CopyToClipboard()
    {
        _clipboardService.CopyToClipboard(Items, Stores);
    }

    [RelayCommand]
    internal void InsertFromClipboard()
    {
        _clipboardService.InsertFromClipboardAsync(Stores, Items);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    [RelayCommand]
    private async Task ChangeTheme(Theme? theme)
    {
        if (theme == null)
        {
            return;
        }

        Logger.Log($"Changing theme to: {theme}");
        Settings.LoadTheme(theme.Name);
        CurrentTheme = theme;
        OnPropertyChanged(nameof(CurrentTheme));

#if __ANDROID__ || __IOS__
        // The below is only necessary until GradientStops support DynamicResource which is a known problem.
        // However, attempting to start the process is optional and is unlikely to work on most devices.
        if (await IsRestartConfirmed())
        {
            Logger.Log("Restarting app");
            var currentProcess = Process.GetCurrentProcess();
            var filename = currentProcess.MainModule?.FileName;
            Logger.Log("Main module name: " + filename);
#if __IOS__
            Application.Current?.Quit();
#else
            if (filename != null)
            {
                Process.Start(filename);
            }

            currentProcess.Kill();
#endif
        }
#endif
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    // ReSharper disable once UnusedMember.Local
    private static async Task<bool> IsRestartConfirmed()
    {
        return await Shell.Current.DisplayAlert(
            "Restart required",
            $"For the theme change to take full effect, you'll need to restart the application. Would you like to close the application now or later?",
            "Now",
            "Later"
        );
    }

    private void SortItems()
    {
        Items = new ObservableCollection<Item>(
            Items.OrderBy(i => i.StoreName).ThenByDescending(i => i.AddedOn)
        );
        OnPropertyChanged(nameof(Items));
    }

    public async Task LoadItemsFromDatabase()
    {
        var loadedItems = await _itemService.GetAsync();
        Items = new ObservableCollection<Item>(loadedItems);
        SortItems();
        Logger.Log($"Loaded {loadedItems.Count} items, new collection size: {Items.Count}");
    }

    public async Task LoadStoresFromDatabase()
    {
        var loadedStores = await _storeService.GetAllAsync();
        Stores = new ObservableCollection<ConfigurableStore>(loadedStores);
        Logger.Log($"Loaded {loadedStores.Count} stores, new collection size: {Stores.Count}");
        foreach (var store in Stores)
        {
            if (store.Name != IStoreService.DefaultStoreName)
                continue;

            CurrentStore = store;
            OnPropertyChanged(nameof(CurrentStore));
        }

        OnPropertyChanged(nameof(Stores));
        Logger.Log("Current store set to: " + CurrentStore?.Name);
    }
}
