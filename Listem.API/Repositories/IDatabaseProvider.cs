using SQLite;

namespace Listem.API.Repositories;

public interface IDatabaseProvider
{
    Task<SQLiteAsyncConnection> GetConnection();
}
