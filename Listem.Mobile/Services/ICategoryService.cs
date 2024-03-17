using Listem.Mobile.Models;

namespace Listem.Mobile.Services;

public interface ICategoryService
{
    Task<ObservableCategory> GetDefaultCategory(string listId);
    Task<List<ObservableCategory>> GetAllAsync();
    Task<List<ObservableCategory>> GetAllByListIdAsync(string listId);
    Task CreateOrUpdateAsync(ObservableCategory observableCategory);
    Task DeleteAsync(ObservableCategory observableCategory);
    Task DeleteAllByListIdAsync(string listId);

    const string DefaultCategoryName = "None";
}

public interface IOnlineCategoryService : ICategoryService { }

public interface IOfflineCategoryService : ICategoryService { }
