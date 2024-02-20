using Listem.Models;

namespace Listem.Services;

public interface IItemListService : IService
{
    ServiceType IService.Type => ServiceType.ItemList;
    Task<List<ObservableItemList>> GetAllAsync();
    Task CreateOrUpdateAsync(ObservableItemList observableItemList);
    Task DeleteAsync(ObservableItemList observableItemList);
    Task DeleteAllAsync();
}
