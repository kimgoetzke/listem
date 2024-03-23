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
        var c = await JsonProcessor.ThrowIfNull(
            () => JsonProcessor.FromFile<RealmAppConfig>("atlasConfig.json")
        );
        var appConfiguration = new AppConfiguration(c.AppId) { BaseUri = new Uri(c.BaseUrl) };

        _app = Realms.Sync.App.Create(appConfiguration);
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
        Logger.Log(
            $"Initialised realm service with:\n- Realm: {_app.CurrentUser}\n- Listem: {User}"
        );
    }

    public static Realm GetMainThreadRealm()
    {
        return _mainThreadRealm ??= GetRealm();
    }

    private static async Task RefreshToken()
    {
        await _app.CurrentUser!.RefreshCustomDataAsync();
        User.Update(_app.CurrentUser);
        WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(User));
    }

    public static async Task SignUpAsync(string email, string password)
    {
        await _app.EmailPasswordAuth.RegisterUserAsync(email, password);
        User.SignUp(_app.CurrentUser!);
        WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(User));
    }

    public static async Task SignInAsync(string email, string password)
    {
        await _app.LogInAsync(Credentials.EmailPassword(email, password));
        User.SignIn(_app.CurrentUser!);
        WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(User));
        using var realm = GetRealm();
        // await realm.Subscriptions.WaitForSynchronizationAsync();
    }

    public static async Task SignOutAsync()
    {
        if (_app.CurrentUser is not null)
            await _app.CurrentUser.LogOutAsync();

        User.SignOut();
        WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(User));
        _mainThreadRealm?.Dispose();
        _mainThreadRealm = null;
    }

    public static async Task SetSubscription(Realm realm, SubscriptionType subType)
    {
        if (GetCurrentSubscriptionType(realm) == subType)
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

    public static SubscriptionType GetCurrentSubscriptionType(Realm realm)
    {
        var activeSubscription = realm.Subscriptions.FirstOrDefault()!;
        return activeSubscription.Name switch
        {
            "all" => SubscriptionType.All,
            "mine" => SubscriptionType.Mine,
            _ => throw new InvalidOperationException("Unknown subscription type")
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
                query = realm.All<List>().Where(i => i.OwnerId == User.Id);
                queryName = "mine";
                break;
            case SubscriptionType.All:
                query = realm.All<List>();
                queryName = "all";
                break;
            case SubscriptionType.Shared:
            default:
                throw new ArgumentException("Unknown subscription type");
        }
        return (query, queryName);
    }

    public static Realm GetRealm()
    {
        var config = new FlexibleSyncConfiguration(_app.CurrentUser!)
        {
            PopulateInitialSubscriptions = (realm) =>
            {
                var (query, queryName) = GetQueryForSubscriptionType(realm, SubscriptionType.Mine);
                realm.Subscriptions.Add(query, new SubscriptionOptions { Name = queryName });
            }
        };

        return Realm.GetInstance(config);
    }

    public enum SubscriptionType
    {
        Mine,
        Shared,
        All
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record RealmAppConfig
    {
        public string AppId { get; init; } = null!;
        public string BaseUrl { get; init; } = null!;
    }
}
