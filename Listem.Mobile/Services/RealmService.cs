using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Mobile.Events;
using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Microsoft.Extensions.Logging;
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
  public static byte[]? ExistingEncryptionKey { get; set; }
  private static ILogger Logger => LoggerProvider.CreateLogger("RealmService");

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
    Logger.Info("Initialised realm with: {User}", User);
  }

  public static Realm GetMainThreadRealm()
  {
    var realm = _mainThreadRealm ??= GetRealm();
    if (realm.Subscriptions.Count == 0)
    {
      Logger.Warn("No subscriptions in main thread realm - requesting now but cannot await sync");
      SetSubscriptions(realm).SafeFireAndForget();
    }
    return realm;
  }

  private static async Task RefreshToken()
  {
    Logger.Info("Refreshing token for current user...");
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
    var encryptionKey = EncryptionKey();
    var config = new FlexibleSyncConfiguration(_app.CurrentUser!)
    {
      PopulateInitialSubscriptions = realm =>
      {
        var (lqName, listQuery) = Query<List>(realm);
        realm.Subscriptions.Add(listQuery, new SubscriptionOptions { Name = lqName });
        var (iqName, itemQuery) = Query<Item>(realm);
        realm.Subscriptions.Add(itemQuery, new SubscriptionOptions { Name = iqName });
      },
      EncryptionKey = encryptionKey
    };
    return Realm.GetInstance(config);
  }

  private static byte[] EncryptionKey()
  {
    if (ExistingEncryptionKey is not null)
    {
      Logger.Info("Using existing encryption key");
      return ExistingEncryptionKey;
    }

    var newEncryptionKey = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(newEncryptionKey);

    SecureStorage
      .Default.SetAsync(Constants.LocalEncryptionKey, Convert.ToBase64String(newEncryptionKey))
      .SafeFireAndForget();
    ExistingEncryptionKey = newEncryptionKey;
    Logger.Info("Using newly created encryption key");
    return newEncryptionKey;
  }

  [SuppressMessage("ReSharper", "UnusedMember.Local")]
  private static async Task SetSubscriptions(Realm realm)
  {
    RemoveAllSubscriptions(realm, true);
    realm.Subscriptions.Update(() =>
    {
      var (lqName, listQuery) = Query<List>(realm);
      realm.Subscriptions.Add(listQuery, new SubscriptionOptions { Name = lqName });
      var (iqName, itemQuery) = Query<Item>(realm);
      realm.Subscriptions.Add(itemQuery, new SubscriptionOptions { Name = iqName });
    });
    Logger.Info("Default subscriptions set, syncing now...");
    if (realm.SyncSession.ConnectionState != ConnectionState.Disconnected)
    {
      await realm.Subscriptions.WaitForSynchronizationAsync();
    }
  }

  // It does not appear to be possible to filter items by referring to the linked List object, see
  // https://www.mongodb.com/docs/realm/sdk/dotnet/sync/flexible-sync/#flexible-sync-rql-requirements-and-limitations.
  // This is why we duplicate the OwnedBy and SharedWith fields in the Item object. :-(
  private static (string queryName, IQueryable<T> query) Query<T>(Realm realm)
    where T : IRealmObject, IShareable
  {
    var query = realm
      .All<T>()
      .Filter("OwnedBy == $0 OR SharedWith CONTAINS $1", User.Id, User.EmailAddress);
    return (typeof(T).Name + "Query", query);
  }

  private static void RemoveAllSubscriptions(Realm realm, bool removeNamed)
  {
    Logger.Info("Removing all subscriptions (removedNamed={Value})", removeNamed);
    foreach (var subscription in realm.Subscriptions)
    {
      Logger.Info("- {Name}: {Query}", subscription.Name!, subscription.Query);
    }
    realm.Subscriptions.Update(() =>
    {
      var count = realm.Subscriptions.RemoveAll(removeNamed);
      Logger.Info("Removed {Count} subscription(s)", count);
    });
  }

  [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
  [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
  private record AtlasConfig
  {
    public string AppId { get; init; } = null!;
    public string BaseUrl { get; init; } = null!;
    public string SecretKey { get; init; } = null!;
  }
}
