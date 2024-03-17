using SQLite;

namespace Listem.Mobile.Services;

public interface IDatabaseProvider
{
    Task<SQLiteAsyncConnection> GetConnection();
}
