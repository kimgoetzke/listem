using Listem.Mobile.Models;
using Listem.Mobile.Services;
using SQLite;
using ModelList = Listem.Mobile.Models.List;

namespace Listem.Mobile.UnitTests.TestDoubles;

internal sealed class ServiceTestDatabaseProvider : IDatabaseProvider, IAsyncDisposable
{
  private readonly string _databasePath;
  private readonly SQLiteAsyncConnection _connection;

  private ServiceTestDatabaseProvider()
  {
    _databasePath = Path.Combine(Path.GetTempPath(), $"listem-tests-{Guid.NewGuid():N}.db3");
    _connection = new SQLiteAsyncConnection(
      _databasePath,
      SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.PrivateCache
    );
  }

  public static async Task<ServiceTestDatabaseProvider> CreateAsync()
  {
    var provider = new ServiceTestDatabaseProvider();
    await provider.InitialiseAsync();
    return provider;
  }

  public Task<SQLiteAsyncConnection> GetConnection()
  {
    return Task.FromResult(_connection);
  }

  private async Task InitialiseAsync()
  {
    await _connection.CreateTableAsync<ModelList>();
    await _connection.CreateTableAsync<Item>();
    await _connection.CreateTableAsync<Category>();
  }

  public async ValueTask DisposeAsync()
  {
    await _connection.CloseAsync();
    if (File.Exists(_databasePath))
      File.Delete(_databasePath);
  }
}
