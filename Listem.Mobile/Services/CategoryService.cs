using Listem.Mobile.Models;
using Microsoft.Extensions.Logging;
using SQLite;
using static Listem.Mobile.Constants;

namespace Listem.Mobile.Services;

public class CategoryService(IDatabaseProvider db, ILogger<CategoryService> logger)
  : ICategoryService
{
  private ObservableCategory? _defaultCategory;

  public async Task<ObservableCategory> GetDefaultCategory(string listId)
  {
    if (_defaultCategory == null)
    {
      var connection = await db.GetConnection();
      var loaded = await connection
        .Table<Category>()
        .FirstAsync(l => l.Name == DefaultCategoryName && l.ListId == listId);
      _defaultCategory = ObservableCategory.From(loaded);
    }

    if (_defaultCategory == null)
      throw new NullReferenceException("This list does not have default category");

    return _defaultCategory;
  }

  public async Task<List<ObservableCategory>> GetAllByListIdAsync(string listId)
  {
    var connection = await db.GetConnection();
    var categories = await connection
      .Table<Category>()
      .Where(c => c.ListId == listId)
      .ToListAsync();
    return ConvertToObservableItems(categories);
  }

  private static List<ObservableCategory> ConvertToObservableItems(List<Category> categories)
  {
    return categories.Select(ObservableCategory.From).ToList();
  }

  public async Task CreateOrUpdateAsync(ObservableCategory observableCategory)
  {
    var connection = await db.GetConnection();
    var category = observableCategory.ToCategory();
    var existingCategory = await connection
      .Table<Category>()
      .Where(c => c.Id == observableCategory.Id)
      .FirstOrDefaultAsync();
    if (existingCategory != null)
    {
      await connection.UpdateAsync(category);
      logger.Info("Updated category: {Category}", category.ToLoggableString());
      return;
    }

    await connection.InsertAsync(category);
    logger.Info("Added category: {Category}", category.ToLoggableString());
  }

  public async Task DeleteAsync(ObservableCategory observableCategory)
  {
    logger.Info("Removing category: {Category}", observableCategory.ToLoggableString());
    var connection = await db.GetConnection();
    var category = observableCategory.ToCategory();
    var result = await connection.DeleteAsync(category);
    if (result == 0)
    {
      logger.Warn("Failed to delete category: {Category}", observableCategory.ToLoggableString());
    }
  }

  public async Task DeleteAllByListIdAsync(string listId)
  {
    var connection = await db.GetConnection();
    await RemoveAllExceptDefaultCategory(connection, listId);
    logger.Info("Reset all categories for list {ID}", listId);
  }

  private static async Task RemoveAllExceptDefaultCategory(
    SQLiteAsyncConnection connection,
    string listId
  )
  {
    var categories = await connection
      .Table<Category>()
      .Where(c => c.ListId == listId)
      .ToListAsync();
    foreach (var category in categories.Where(c => c.Name != DefaultCategoryName))
    {
      await connection.DeleteAsync(category);
    }
  }
}
