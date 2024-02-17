using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Listem.Models;
using Listem.Utilities;
using Listem.Views;
using Random = System.Random;

namespace Listem.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ItemList> _lists = [];

    [ObservableProperty]
    private ItemList _newList;

    public MainViewModel()
    {
        NewList = new ItemList();
        Lists =
        [
            new ItemList
            {
                Name = "Shopping list",
                Items =
                [
                    new Item { Title = "This is a very long item name that should be cut off" },
                    new Item { Title = "Bread" },
                    new Item { Title = "Milk" },
                    new Item { Title = "Eggs" },
                    new Item { Title = "Butter" },
                    new Item { Title = "Cheese" },
                    new Item { Title = "Tomatoes" },
                    new Item { Title = "Potatoes" },
                    new Item { Title = "Carrots" },
                    new Item { Title = "Cucumber" },
                    new Item { Title = "Bananas" },
                    new Item { Title = "Apples" },
                    new Item { Title = "Oranges" },
                    new Item { Title = "Grapes" },
                    new Item { Title = "Pineapple" },
                    new Item { Title = "Peaches" },
                ],
                AddedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            },
            new ItemList
            {
                Name = "Todo list with a very, very long title that should be cut off",
                Items = [new Item { Title = "Do this" }, new Item { Title = "Do that" }],
                AddedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            }
        ];
    }

    [RelayCommand]
    private async Task AddList()
    {
        var random = new Random();
        NewList = new ItemList
        {
            Name = "List " + random.Next(100) + 1,
            Items = [],
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };
        if (string.IsNullOrWhiteSpace(NewList.Name))
            return;

        NewList.Name = StringProcessor.TrimAndCapitaliseFirstChar(NewList.Name);
        await Application.Current!.Dispatcher.DispatchAsync(() => Lists.Add(NewList));
        Notifier.ShowToast($"Added: {NewList.Name}");
        Logger.Log($"Added item: {NewList.ToLoggableString()}");

        // Make sure the UI is reset/updated
        NewList = new ItemList();
        OnPropertyChanged(nameof(NewList));
        OnPropertyChanged(nameof(Lists));
    }

    [RelayCommand]
    private async Task RemoveList(ItemList list)
    {
        Logger.Log($"Removing list: {list.ToLoggableString()}");
        await Application.Current!.Dispatcher.DispatchAsync(() => Lists.Remove(list));
    }

    [RelayCommand]
    private static async Task TapList(ItemList list)
    {
        Logger.Log($"Opening list: {list.ToLoggableString()}");
        await Shell.Current.Navigation.PushAsync(new ListPage(list));
    }
}
