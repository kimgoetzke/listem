using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using ListType = Listem.Shared.Enums.ListType;

namespace Listem.Mobile.ViewModel;

[QueryProperty(nameof(Item), nameof(Item))]
public partial class DetailViewModel : ObservableObject
{
    [ObservableProperty]
    private IList<Category> _categories = [];

    [ObservableProperty]
    private Category _currentCategory;

    [ObservableProperty]
    private Item _item;

    public ListType ListType { get; }

    private readonly IItemService _itemService;

    public DetailViewModel(Item item, IServiceProvider serviceProvider)
    {
        _itemService = serviceProvider.GetService<IItemService>()!;
        Item = item;
        ListType = Enum.TryParse(item.List!.ListType, out ListType type) ? type : ListType.Standard;
        Categories = item.List!.Categories;
        CurrentCategory = Categories.First(c => c.Name == item.Category!.Name);
    }

    [RelayCommand]
    private async Task SaveAndBack()
    {
        await _itemService.UpdateAsync(Item, category: CurrentCategory);
        Back().SafeFireAndForget();
    }

    [RelayCommand]
    private static async Task Back()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
