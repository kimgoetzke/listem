using System.Diagnostics.CodeAnalysis;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Realms;
using Realms.Sync;
using User = Listem.Mobile.Models.User;

namespace Listem.Mobile.Services;

public static class RealmService
{
    public static User User { get; private set; } = new();
    private static bool _serviceInitialised;
    private static Realms.Sync.App _app = null!;
    private static Realm? _mainThreadRealm;

    public static async Task Init()
    {
        if (_serviceInitialised)
            return;

        await InitialiseSyncApp();
        await InitialiseUser();
    }

    private static async Task InitialiseSyncApp()
    {
        var config = await JsonProcessor.NotNull(
            () => JsonProcessor.FromFile<AtlasConfig>("atlasConfig.json")
        );
        var appConfig = new AppConfiguration(config.AppId) { BaseUri = new Uri(config.BaseUrl) };

        _app = Realms.Sync.App.Create(appConfig);
        _serviceInitialised = true;
    }

    private static async Task InitialiseUser()
    {
        User = await JsonProcessor.FromSecureStorage<User>(Constants.User) ?? new User();
        if (_app.CurrentUser?.RefreshToken != null)
        {
            await RefreshToken();
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(User));
        }
        Logger.Log($"Initialised realm with: (see below)\n- RU: {_app.CurrentUser}\n- LU: {User}");
    }

    public static Realm GetMainThreadRealm()
    {
        var realm = _mainThreadRealm ??= GetRealm();
        if (realm.Subscriptions.Count == 0)
        {
            SetSubscription(realm, SubscriptionType.Mine).SafeFireAndForget();
        }
        return realm;
    }

    private static async Task RefreshToken()
    {
        Logger.Log("Refreshing token for current user...");
        await _app.CurrentUser!.RefreshCustomDataAsync();
        User.Update(_app.CurrentUser);
        JsonProcessor.ToSecureStorage(Constants.User, User).SafeFireAndForget();
        WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(User));
    }

    public static async Task SignUpAsync(string email, string password)
    {
        await _app.EmailPasswordAuth.RegisterUserAsync(email, password);
        User.SignUp(_app.CurrentUser!);
        JsonProcessor.ToSecureStorage(Constants.User, User).SafeFireAndForget();
        WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(User));
    }

    public static async Task SignInAsync(string email, string password)
    {
        await _app.LogInAsync(Credentials.EmailPassword(email, password));
        User.SignIn(_app.CurrentUser!);
        JsonProcessor.ToSecureStorage(Constants.User, User).SafeFireAndForget();
        WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(User));
        using var realm = GetRealm();
        await realm.Subscriptions.WaitForSynchronizationAsync();
    }

    public static async Task SignOutAsync()
    {
        if (_app.CurrentUser is not null)
            await _app.CurrentUser.LogOutAsync();

        User.SignOut();
        JsonProcessor.ToSecureStorage(Constants.User, User).SafeFireAndForget();
        WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(User));
        _mainThreadRealm?.Dispose();
        _mainThreadRealm = null;
    }

    public static async Task SetSubscription(Realm realm, SubscriptionType subType)
    {
        if (subType == CurrentSubscriptionType(realm))
            return;

        realm.Subscriptions.Update(() =>
        {
            realm.Subscriptions.RemoveAll(true);
            var (query, queryName) = GetQueryForSubscriptionType(realm, subType);
            realm.Subscriptions.Add(query, new SubscriptionOptions { Name = queryName });
        });

        if (realm.SyncSession.ConnectionState != ConnectionState.Disconnected)
        {
            await realm.Subscriptions.WaitForSynchronizationAsync();
        }
    }

    private static SubscriptionType? CurrentSubscriptionType(Realm realm)
    {
        return realm.Subscriptions.FirstOrDefault()?.Name switch
        {
            "accessible" => SubscriptionType.AccessibleToMe,
            "shared" => SubscriptionType.SharedWithMe,
            "mine" => SubscriptionType.Mine,
            _ => null
        };
    }

    private static (IQueryable<List> Query, string Name) GetQueryForSubscriptionType(
        Realm realm,
        SubscriptionType subType
    )
    {
        IQueryable<List>? query;
        string? queryName;
        switch (subType)
        {
            case SubscriptionType.Mine:
                query = realm.All<List>().Where(i => i.OwnedBy == User.Id);
                queryName = "mine";
                break;
            case SubscriptionType.AccessibleToMe:
                query = realm
                    .All<List>()
                    .Where(l => l.OwnedBy == User.Id || l.SharedWith.Contains(User.Id!));
                queryName = "accessible";
                break;
            case SubscriptionType.SharedWithMe:
                query = realm.All<List>().Where(l => l.SharedWith.Contains(User.Id!));
                queryName = "shared";
                break;
            default:
                throw new ArgumentException("Unknown subscription type");
        }
        return (query, queryName);
    }

    private static Realm GetRealm()
    {
        var config = new FlexibleSyncConfiguration(_app.CurrentUser!)
        {
            PopulateInitialSubscriptions = realm =>
            {
                var (query, queryName) = GetQueryForSubscriptionType(
                    realm,
                    SubscriptionType.AccessibleToMe
                );
                realm.Subscriptions.Add(query, new SubscriptionOptions { Name = queryName });
            }
        };

        return Realm.GetInstance(config);
    }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private record AtlasConfig
    {
        public string AppId { get; init; } = null!;
        public string BaseUrl { get; init; } = null!;
    }
}
