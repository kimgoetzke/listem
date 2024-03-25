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
        Realms.Logging.Logger.LogLevel = Realms.Logging.LogLevel.Debug;

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
        Logger.Log($"Initialised realm with: {User}");
    }

    public static Realm GetMainThreadRealm()
    {
        var realm = _mainThreadRealm ??= GetRealm();
        if (realm.Subscriptions.Count == 0)
        {
            throw new InvalidOperationException("No subscriptions set in main thread realm");
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
        using var realm = GetRealm();
        // await SetSubscriptions(realm); // Only for development to update subscriptions on app launch
        await realm.Subscriptions.WaitForSynchronizationAsync();
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

    private static Realm GetRealm()
    {
        var config = new FlexibleSyncConfiguration(_app.CurrentUser!)
        {
            PopulateInitialSubscriptions = realm =>
            {
                var (lqName, listQuery) = QueryForLists(realm);
                realm.Subscriptions.Add(listQuery, new SubscriptionOptions { Name = lqName });
                var (iqName, itemQuery) = QueryForItems(realm);
                realm.Subscriptions.Add(itemQuery, new SubscriptionOptions { Name = iqName });
            }
        };
        return Realm.GetInstance(config);
    }

    private static async Task SetSubscriptions(Realm realm)
    {
        RemoveAllSubscriptions(realm, true);
        realm.Subscriptions.Update(() =>
        {
            var (lqName, listQuery) = QueryForLists(realm);
            realm.Subscriptions.Add(listQuery, new SubscriptionOptions { Name = lqName });
            var (iqName, itemQuery) = QueryForItems(realm);
            realm.Subscriptions.Add(itemQuery, new SubscriptionOptions { Name = iqName });
        });
        Logger.Log("Default subscriptions set, syncing now...");
        if (realm.SyncSession.ConnectionState != ConnectionState.Disconnected)
        {
            await realm.Subscriptions.WaitForSynchronizationAsync();
        }
    }

    // It does not appear to be possible to filter items by referring to the linked List object, see
    // https://www.mongodb.com/docs/realm/sdk/dotnet/sync/flexible-sync/#flexible-sync-rql-requirements-and-limitations.
    // This is why we duplicate the OwnedBy and SharedWith fields in the Item object. :-(
    private static (string queryName, IQueryable<Item> query) QueryForItems(Realm realm)
    {
        var query = realm
            .All<Item>()
            .Filter("OwnedBy == $0 OR SharedWith CONTAINS $1", User.Id, User.Id)
            .OrderByDescending(x => x.UpdatedOn);
        return ("accessibleItems", query);
    }

    private static (string queryName, IQueryable<List> query) QueryForLists(Realm realm)
    {
        var query = realm
            .All<List>()
            .Filter("OwnedBy == $0 OR SharedWith CONTAINS $1", User.Id, User.Id)
            .OrderByDescending(x => x.UpdatedOn);
        return ("accessibleLists", query);
    }

    private static void RemoveAllSubscriptions(Realm realm, bool removeNamed)
    {
        Logger.Log($"Removing all subscriptions (removedNamed={removeNamed})");
        foreach (var subscription in realm.Subscriptions)
        {
            Logger.Log($"- {subscription.Name}: {subscription.Query}");
        }
        realm.Subscriptions.Update(() =>
        {
            var count = realm.Subscriptions.RemoveAll(removeNamed);
            Logger.Log($"Removed {count} subscription(s)");
        });
    }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private record AtlasConfig
    {
        public string AppId { get; init; } = null!;
        public string BaseUrl { get; init; } = null!;
    }
}
