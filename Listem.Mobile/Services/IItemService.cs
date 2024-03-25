using Listem.Mobile.Models;
using MongoDB.Bson;

namespace Listem.Mobile.Services;

public interface IItemService
{
    Task CreateAsync(Item item);
    Task UpdateAsync(
        Item item,
        string? name = null,
        string? ownedBy = null,
        ISet<string>? sharedWith = null,
        Category? category = null,
        int? quantity = null,
        bool? isImportant = null
    );
    Task DeleteAsync(Item item);
    Task DeleteAllInListAsync(List list);
    Task ResetAllToDefaultCategoryAsync(List list);
    Task ResetSelectedToDefaultCategoryAsync(List list, Category category);
}
