using System.Collections.ObjectModel;
using System.Diagnostics;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
    private ObservableCollection<ObservableTheme> _themes = [];

    [ObservableProperty]
    private ObservableTheme _currentTheme;

    [ObservableProperty]
    private ObservableItemList _observableItemList;

    [ObservableProperty]
    private ObservableItem _newObservableItem;

    [ObservableProperty]
    private ObservableCollection<ObservableItem> _items = [];

    [ObservableProperty]
    private ObservableCollection<ObservableCategory> _categories = [];

    [ObservableProperty]
    private ObservableCategory? _currentCategory;

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IClipboardService _clipboardService;

    public ListViewModel(ObservableItemList observableItemList)
    {
        _categoryService = IPlatformApplication.Current!.Services.GetService<ICategoryService>()!;
        _itemService = IPlatformApplication.Current.Services.GetService<IItemService>()!;
        _clipboardService = IPlatformApplication.Current.Services.GetService<IClipboardService>()!;
        ObservableItemList = observableItemList;
        Items = new ObservableCollection<ObservableItem>(observableItemList.Items);
        NewObservableItem = new ObservableItem(ObservableItemList.Id);
        Themes = Settings.GetAllThemesAsCollection();
        CurrentTheme = Themes.First(t => t.Name == Settings.CurrentTheme);
        SortItems();
        LoadCategories().SafeFireAndForget();
    }

    private async Task LoadCategories()
    {
        var categories = await _categoryService.GetAllByListIdAsync(ObservableItemList.Id);
        Categories = new ObservableCollection<ObservableCategory>(categories);
        CurrentCategory = Categories.FirstOrDefault(c =>
            c.Name == ICategoryService.DefaultCategoryName
        );
        OnPropertyChanged(nameof(Categories));
        OnPropertyChanged(nameof(CurrentCategory));
        Logger.Log(
            $"Loaded {Categories.Count} categories for list {ObservableItemList.Id} from the database"
        );
    }

    [RelayCommand]
    private async Task AddItem()
    {
        // Don't add empty items
        if (string.IsNullOrWhiteSpace(NewObservableItem.Title))
            return;

        // Pre-process item
        NewObservableItem.Title = StringProcessor.TrimAndCapitalise(NewObservableItem.Title);
        NewObservableItem.CategoryName =
            CurrentCategory != null ? CurrentCategory.Name : ICategoryService.DefaultCategoryName;

        // Add to list and database
        Logger.Log($"Adding item: {NewObservableItem.ToLoggableString()}");
        var value = new ItemChangedDto(ObservableItemList.Id, NewObservableItem);
        WeakReferenceMessenger.Default.Send(new ItemAddedToListMessage(value));
        await _itemService.CreateOrUpdateAsync(NewObservableItem);
        Items.Add(NewObservableItem);
        Notifier.ShowToast($"Added: {NewObservableItem.Title}");

        // Make sure the UI is reset/updated
        NewObservableItem = new ObservableItem(ObservableItemList.Id);
        SortItems();
        OnPropertyChanged(nameof(NewObservableItem));
    }

    [RelayCommand]
    private async Task RemoveItem(ObservableItem i)
    {
        await _itemService.DeleteAsync(i);
        ObservableItemList.Items.Remove(i);
        Items.Remove(i);
        var value = new ItemChangedDto(ObservableItemList.Id, i);
        WeakReferenceMessenger.Default.Send(new ItemRemovedFromListMessage(value));
        OnPropertyChanged(nameof(ObservableItemList));
    }

    [RelayCommand]
    private async Task RemoveAllItems()
    {
        if (!await IsRequestConfirmedByUser())
            return;

        Items.Clear();
        await _itemService.DeleteAllByListIdAsync(ObservableItemList.Id);
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
    private async Task TogglePriority(ObservableItem i)
    {
        i.IsImportant = !i.IsImportant;
        await _itemService.CreateOrUpdateAsync(i);
        SortItems();
    }

    [RelayCommand]
    private static async Task TapItem(ObservableItem i)
    {
        await Shell.Current.Navigation.PushModalAsync(new DetailPage(i));
    }

    [RelayCommand]
    private async Task ManageCategories()
    {
        await Shell.Current.Navigation.PushAsync(new CategoryPage(ObservableItemList.Id));
    }

    [RelayCommand]
    private void CopyToClipboard()
    {
        _clipboardService.CopyToClipboard(Items, Categories);
    }

    [RelayCommand]
    private void InsertFromClipboard()
    {
        _clipboardService.InsertFromClipboardAsync(Items, Categories, ObservableItemList.Id);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    [RelayCommand]
    private async Task ChangeTheme(ObservableTheme? theme)
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
        Items = new ObservableCollection<ObservableItem>(
            Items.OrderBy(i => i.CategoryName).ThenByDescending(i => i.AddedOn)
        );
        OnPropertyChanged(nameof(Items));
    }
}
