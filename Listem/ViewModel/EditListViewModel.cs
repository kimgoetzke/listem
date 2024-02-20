using System.Collections.ObjectModel;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;
using Listem.Views;

namespace Listem.ViewModel;

public partial class EditListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableItemList _observableItemList;

    [ObservableProperty]
    private ObservableCollection<ObservableItem> _items = [];

    [ObservableProperty]
    private ObservableCollection<ObservableCategory> _categories = [];

    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IItemListService _itemListService;

    public EditListViewModel(ObservableItemList observableItemList)
    {
        _itemListService = IPlatformApplication.Current!.Services.GetService<IItemListService>()!;
        _categoryService = IPlatformApplication.Current.Services.GetService<ICategoryService>()!;
        _itemService = IPlatformApplication.Current.Services.GetService<IItemService>()!;
        ObservableItemList = observableItemList;
        Items = new ObservableCollection<ObservableItem>(observableItemList.Items);
        LoadCategories().SafeFireAndForget();
    }

    private async Task LoadCategories()
    {
        var categories = await _categoryService.GetAllByListIdAsync(ObservableItemList.Id);
        Categories = new ObservableCollection<ObservableCategory>(categories);
        OnPropertyChanged(nameof(Categories));
        Logger.Log(
            $"Loaded {Categories.Count} categories for list {ObservableItemList.Id} from the database"
        );
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
    private async Task ManageCategories()
    {
        await Shell.Current.Navigation.PushAsync(new CategoryPage(ObservableItemList.Id));
    }

    [RelayCommand]
    private async Task GoBack()
    {
        ObservableItemList.Name = StringProcessor.TrimAndCapitalise(ObservableItemList.Name);
        await _itemListService.CreateOrUpdateAsync(ObservableItemList);
        await Shell.Current.Navigation.PopModalAsync();
    }
}
