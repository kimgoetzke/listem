using SQLite;

namespace Listem.Services;

public interface IDatabaseProvider
{
    Task<SQLiteAsyncConnection> GetConnection();
}
