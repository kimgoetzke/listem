using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Events;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;
using Listem.Views;

namespace Listem.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ObservableItemList> _lists = [];

    [ObservableProperty]
    private ObservableItemList _newList;

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
    private readonly IItemListService _itemListService;
    private readonly IItemService _itemService;

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _itemListService = serviceProvider.GetService<IItemListService>()!;
        _itemService = serviceProvider.GetService<IItemService>()!;
        _authService = serviceProvider.GetService<AuthService>()!;
        NewList = new ObservableItemList();
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
                Logger.Log($"Received message: Setting current user email to '{m.Value}'");
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
            IsUserSignedIn = false;
            return;
        }
        var currentUser = await _authService.FetchExistingUser();
        if (currentUser != null)
        {
            CurrentUserEmail = currentUser.EmailAddress!;
            IsUserSignedIn = true;
        }
        Logger.Log($"Updated current user's email to: {CurrentUserEmail}");
    }

    public async Task LoadItemLists()
    {
        Logger.Log("Loading lists from database");
        var lists = await _itemListService.GetAllAsync();
        foreach (var list in lists)
        {
            var items = await _itemService.GetAllByListIdAsync(list.Id);
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

        NewList = new ObservableItemList
        {
            Name = name,
            AddedOn = DateTime.Now,
            UpdatedOn = DateTime.Now
        };

        NewList.Name = StringProcessor.TrimAndCapitalise(NewList.Name);
        await AddNewList(NewList);

        NewList = new ObservableItemList();
        OnPropertyChanged(nameof(NewList));
        OnPropertyChanged(nameof(Lists));
    }

    private async Task AddNewList(ObservableItemList newList)
    {
        await _itemListService.CreateOrUpdateAsync(newList);
        Lists.Add(newList);
        Notifier.ShowToast($"Added: {newList.Name}");
        Logger.Log($"Added list: {newList.ToLoggableString()}");
        SortLists();
    }

    private void SortLists()
    {
        Lists = new ObservableCollection<ObservableItemList>(
            Lists.OrderBy(l => l.UpdatedOn).Reverse()
        );
        OnPropertyChanged(nameof(Lists));
    }

    [RelayCommand]
    private async Task RemoveList(ObservableItemList list)
    {
        var isConfirmed = await IsDeletionConfirmedByUser(list.Name);

        if (!isConfirmed)
        {
            Logger.Log($"Cancelled action to delete: {list.ToLoggableString()}");
            return;
        }

        Logger.Log($"Removing list: {list.ToLoggableString()}");
        await _itemListService.DeleteAsync(list);
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
    private async Task SignInOrSignUp()
    {
        var signInPage = _serviceProvider.GetService<SignInPage>()!;
        await Shell.Current.Navigation.PushAsync(signInPage);
    }

    [RelayCommand]
    private void SignOut()
    {
        _authService.SignOut();
        IsUserSignedIn = false;
    }

    [RelayCommand]
    private static async Task TapList(ObservableItemList list)
    {
        Logger.Log($"Opening list: {list.ToLoggableString()}");
        await Shell.Current.Navigation.PushAsync(new ListPage(list));
    }

    [RelayCommand]
    private static async Task EditList(ObservableItemList list)
    {
        Logger.Log($"Editing list: {list.ToLoggableString()}");
        await Shell.Current.Navigation.PushModalAsync(new EditListPage(list));
    }
}
