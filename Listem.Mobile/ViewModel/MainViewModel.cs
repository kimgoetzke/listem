using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;

namespace Listem.Mobile.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ObservableList> _lists = [];

    [ObservableProperty]
    private ObservableList _newList;

    [ObservableProperty]
    private ObservableCollection<ObservableTheme> _themes = [];

    [ObservableProperty]
    private ObservableTheme _currentTheme;

    [ObservableProperty]
    private string _currentUserEmail = "(Not signed in)";

    [ObservableProperty]
    private bool _isUserSignedIn;

    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;
    private readonly IListService _listService;
    private readonly IItemService _itemService;

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _listService = serviceProvider.GetService<IListService>()!;
        _itemService = serviceProvider.GetService<IItemService>()!;
        _authService = serviceProvider.GetService<AuthService>()!;
        NewList = new ObservableList();
        Lists = [];
        Themes = ThemeHandler.GetAllThemesAsCollection();
        CurrentTheme = Themes.First(t => t.Name == Settings.CurrentTheme);

        WeakReferenceMessenger.Default.Register<ItemRemovedFromListMessage>(
            this,
            (_, m) =>
            {
                Logger.Log(
                    $"Received message: Removing '{m.Value.Item.Title}' from {m.Value.ListId}"
                );
                Lists.First(l => l.Id == m.Value.ListId).Items.Remove(m.Value.Item);
            }
        );

        WeakReferenceMessenger.Default.Register<ItemAddedToListMessage>(
            this,
            (_, m) =>
            {
                Logger.Log($"Received message: Adding '{m.Value.Item.Title}' to {m.Value.ListId}");
                Lists.First(l => l.Id == m.Value.ListId).Items.Add(m.Value.Item);
            }
        );

        WeakReferenceMessenger.Default.Register<UserEmailSetMessage>(
            this,
            (_, m) =>
            {
                Logger.Log(
                    $"Received message: Setting current user email to '{m.Value}' in MainViewModel"
                );
                CurrentUserEmail = m.Value;
                IsUserSignedIn = true;
            }
        );
    }

    public async Task SetUserIfKnown()
    {
        if (!_authService.IsOnline())
        {
            Notifier.ShowToast("No internet connection - you're in offline mode");
        }
        var currentUser = await _authService.GetCurrentUser();
        if (currentUser.EmailAddress is { } email)
        {
            CurrentUserEmail = email;
            IsUserSignedIn = currentUser.IsSignedIn;
        }
        Logger.Log($"Updated current user's email to: {CurrentUserEmail}");
    }

    public async Task LoadLists()
    {
        Logger.Log("Loading lists from database");
        var lists = await _listService.GetAllAsync();
        foreach (var list in lists)
        {
            var items = await _itemService.GetAllByListIdAsync(list.Id!);
            foreach (var item in items)
            {
                list.Items.Add(item);
            }

            Lists.Add(list);
        }
        SortLists();
    }

    [RelayCommand]
    private async Task AddList(string name)
    {
        if (name.Length == 0)
            return;

        NewList = new ObservableList
        {
            Name = name,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };

        NewList.Name = StringProcessor.TrimAndCapitalise(NewList.Name);
        await AddNewList(NewList);

        NewList = new ObservableList();
        OnPropertyChanged(nameof(NewList));
        OnPropertyChanged(nameof(Lists));
    }

    private async Task AddNewList(ObservableList newList)
    {
        await _listService.CreateOrUpdateAsync(newList);
        Lists.Add(newList);
        Notifier.ShowToast($"Added: {newList.Name}");
        Logger.Log($"Added list: {newList.ToLoggableString()}");
        SortLists();
    }

    private void SortLists()
    {
        Lists = new ObservableCollection<ObservableList>(Lists.OrderBy(l => l.UpdatedOn).Reverse());
        OnPropertyChanged(nameof(Lists));
    }

    [RelayCommand]
    private async Task RemoveList(ObservableList list)
    {
        var isConfirmed = await IsDeletionConfirmedByUser(list.Name);

        if (!isConfirmed)
        {
            Logger.Log($"Cancelled action to delete: {list.ToLoggableString()}");
            return;
        }

        Logger.Log($"Removing list: {list.ToLoggableString()}");
        await _listService.DeleteAsync(list);
        Lists.Remove(list);
    }

    private static async Task<bool> IsDeletionConfirmedByUser(string listName)
    {
        return await Shell.Current.DisplayAlert(
            "Delete list",
            $"This will delete the list '{listName}' and all of its contents. It cannot be undone. Are you sure?",
            "Yes",
            "No"
        );
    }

    [RelayCommand]
    private Task SetTheme(ObservableTheme? theme)
    {
        if (theme == null)
            return Task.CompletedTask;

        Logger.Log($"Changing theme to: {theme}");
        ThemeHandler.SetTheme(theme.Name);
        CurrentTheme = theme;
        OnPropertyChanged(nameof(CurrentTheme));
        return Task.CompletedTask;
    }

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

    [RelayCommand]
    private async Task BackToStartPage()
    {
        _authService.SignOut();
        IsUserSignedIn = false;
        await Shell.Current.Navigation.PopToRootAsync();
    }

    [RelayCommand]
    private static async Task TapList(ObservableList list)
    {
        Logger.Log($"Opening list: {list.ToLoggableString()}");
        await Shell.Current.Navigation.PushAsync(new ListPage(list));
    }

    [RelayCommand]
    private static async Task EditList(ObservableList list)
    {
        Logger.Log($"Editing list: {list.ToLoggableString()}");
        await Shell.Current.Navigation.PushModalAsync(new EditListPage(list));
    }
}
