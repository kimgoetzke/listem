using Listem.Mobile.Models;
using MongoDB.Bson;

namespace Listem.Mobile.Services;

public interface IItemService
{
    Task CreateOrUpdateAsync(Item item);
    Task DeleteAsync(Item item);
    Task DeleteAllByListIdAsync(ObjectId listId);
    Task UpdateAllToDefaultCategoryAsync(ObjectId listId);
    Task UpdateAllToCategoryAsync(string categoryName, ObjectId listId);
}
