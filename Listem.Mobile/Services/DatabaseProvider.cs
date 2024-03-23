using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using SQLite;

namespace Listem.Mobile.Services;

public class DatabaseProvider : IDatabaseProvider
{
    private const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

    private const string DatabaseFilename = "Listem.db3";

    private static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

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

        Logger.Log($"Initialising database");
        _connection = new SQLiteAsyncConnection(DatabasePath, Flags);
        Logger.Log($"Connected to: {DatabasePath}");
        var itemListTable = InitialiseItemListTable(_connection);
        var itemTable = InitialiseItemTable(_connection);
        var categoryTable = InitialiseCategoryTable(_connection);
        await Task.WhenAll(itemListTable, itemTable, categoryTable).ConfigureAwait(false);
    }

    private static Task<CreateTableResult> InitialiseItemListTable(SQLiteAsyncConnection connection)
    {
        return connection.CreateTableAsync<List>();
    }

    private static Task<CreateTableResult> InitialiseItemTable(SQLiteAsyncConnection connection)
    {
        return connection.CreateTableAsync<Item>();
    }

    private static async Task InitialiseCategoryTable(SQLiteAsyncConnection connection)
    {
        await connection.CreateTableAsync<Category>();
    }
}
