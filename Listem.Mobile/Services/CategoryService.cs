using Listem.Mobile.Models;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.Services;

public class CategoryService : ILocalCategoryService
{
    public async Task<List<ObservableCategory>> GetAllByListIdAsync(string listId)
    {
        Logger.Log($"Getting all categories for list {listId}");
        return [];
    }

    public async Task CreateOrUpdateAsync(ObservableCategory observableCategory)
    {
        Logger.Log($"Added or updated category: {observableCategory.ToLoggableString()}");
    }

    public async Task DeleteAsync(ObservableCategory observableCategory)
    {
        Logger.Log($"Removing category: {observableCategory.ToLoggableString()}");
    }

    public async Task DeleteAllByListIdAsync(string listId)
    {
        Logger.Log($"Reset all categories for list {listId}");
    }
}
