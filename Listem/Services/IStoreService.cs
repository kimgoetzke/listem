using Listem.Models;

namespace Listem.Services;

public interface IStoreService : IService
{
    ServiceType IService.Type => ServiceType.Store;
    Task<ConfigurableStore> GetDefaultStore();
    Task<List<ConfigurableStore>> GetAllAsync();
    Task CreateOrUpdateAsync(ConfigurableStore store);
    Task DeleteAsync(ConfigurableStore store);
    Task DeleteAllAsync();

    const string DefaultStoreName = "Anywhere";
}
