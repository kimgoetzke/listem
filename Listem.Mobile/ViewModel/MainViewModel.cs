using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;
using Listem.Mobile.Views;
using Listem.Shared.Enums;
using Realms;

namespace Listem.Mobile.ViewModel;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private IQueryable<List> _lists = null!;

    [ObservableProperty]
    private ObservableCollection<ObservableTheme> _themes = [];

    [ObservableProperty]
    private ObservableTheme _currentTheme;

    [ObservableProperty]
    private string? _currentUserEmail;

    [ObservableProperty]
    private bool _isUserSignedIn;

    private readonly IServiceProvider _serviceProvider;
    private readonly IListService _listService;
    private readonly IItemService _itemService;
    private readonly Realm _realm = RealmService.GetMainThreadRealm();

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _listService = serviceProvider.GetService<IListService>()!;
        _itemService = serviceProvider.GetService<IItemService>()!;
        GetSortedLists();
        Themes = ThemeHandler.GetAllThemesAsCollection();
        CurrentTheme = Themes.First(t => t.Name == Settings.CurrentTheme);

        WeakReferenceMessenger.Default.Register<UserStatusChangedMessage>(
            this,
            (_, m) =>
            {
                Logger.Log(
                    $"[MainViewModel] Received message: Current user status has changed to: {m.Value}"
                );
                CurrentUserEmail = m.Value.EmailAddress;
                IsUserSignedIn = m.Value.IsSignedIn;
            }
        );
    }

    // TODO: Implement sorting and filtering
    private void GetSortedLists()
    {
        Lists = _realm.All<List>().OrderByDescending(l => l.UpdatedOn);
    }

    public void InitialiseUser()
    {
        var currentUser = RealmService.User;
        CurrentUserEmail = currentUser.EmailAddress;
        IsUserSignedIn = currentUser.IsSignedIn;
    }

    [RelayCommand]
    private async Task AddList(string name)
    {
        if (name.Length == 0)
            return;

        var newList = new List
        {
            Name = StringProcessor.TrimAndCapitalise(name),
            OwnedBy = RealmService.User.Id!,
            ListType = ListType.Standard.ToString(),
            UpdatedOn = DateTime.Now.ToUniversalTime()
        };
        await _listService.CreateAsync(newList);
        GetSortedLists();
    }

    [RelayCommand]
    private async Task RemoveList(List list)
    {
        if (!await IsDeletionConfirmedByUser(list.Name))
        {
            Logger.Log($"Cancelled action to delete: {list.ToLoggableString()}");
            return;
        }

        await _itemService.DeleteAllInListAsync(list);
        await _listService.DeleteAsync(list);
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
        await RealmService.SignOutAsync();
        IsUserSignedIn = false;
        await Shell.Current.Navigation.PopToRootAsync();
    }

    [RelayCommand]
    private async Task TapList(List list)
    {
        Logger.Log($"Opening list: {list.ToLoggableString()}");
        await Shell.Current.Navigation.PushAsync(new ListPage(list, _serviceProvider));
    }

    [RelayCommand]
    private async Task EditList(List list)
    {
        Logger.Log($"Editing list: {list.ToLoggableString()}");
        await Shell.Current.Navigation.PushModalAsync(new EditListPage(list, _serviceProvider));
    }
}
