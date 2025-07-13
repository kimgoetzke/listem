using Listem.Mobile.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace Listem.Mobile.Services;

public class DatabaseProvider(ILogger<DatabaseProvider> logger) : IDatabaseProvider
{
  private const SQLiteOpenFlags Flags =
    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

  private const string DatabaseFilename = "Listem.db3";

  private static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

  private SQLiteAsyncConnection? _connection;

  public async Task<SQLiteAsyncConnection> GetConnection()
  {
    await InitialiseDatabase();
    return _connection!;
  }

  private async ValueTask InitialiseDatabase()
  {
    if (_connection is not null)
      return;

    logger.Info("Initialising database");
    _connection = new SQLiteAsyncConnection(DatabasePath, Flags);
    logger.Info("Connected to: {Path}", DatabasePath);
    var itemListTable = InitialiseItemListTable(_connection);
    var itemTable = InitialiseItemTable(_connection);
    var categoryTable = InitialiseCategoryTable(_connection);
    await Task.WhenAll(itemListTable, itemTable, categoryTable).ConfigureAwait(false);
  }

  private Task<CreateTableResult> InitialiseItemListTable(SQLiteAsyncConnection connection)
  {
    var result = connection.CreateTableAsync<List>();
    LogResult(result, nameof(List));
    return result;
  }

  private Task<CreateTableResult> InitialiseItemTable(SQLiteAsyncConnection connection)
  {
    var result = connection.CreateTableAsync<Item>();
    LogResult(result, nameof(Item));
    return result;
  }

  private async Task InitialiseCategoryTable(SQLiteAsyncConnection connection)
  {
    var result = connection.CreateTableAsync<Category>();
    LogResult(result, nameof(Category));
    await result;
  }

  private void LogResult(Task<CreateTableResult> result, string name)
  {
    logger.Info("Table {Name} was {Result}", name, result.Result.ToString().ToLower());
  }
}
