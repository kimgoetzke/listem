using Listem.Mobile.Models;
using Microsoft.Extensions.Logging;
using SQLite;
using Models_Category = Listem.Mobile.Models.Category;

namespace Listem.Mobile.Services;

public class ListService(IDatabaseProvider db, ILogger<ListService> logger) : IListService
{
  public async Task<List<ObservableList>> GetAllAsync()
  {
    var connection = await db.GetConnection();
    var lists = await connection.Table<List>().ToListAsync();
    return ConvertToObservableItemLists(lists);
  }

  private static List<ObservableList> ConvertToObservableItemLists(List<List> lists)
  {
    return lists.Select(ObservableList.From).ToList();
  }

  public async Task CreateOrUpdateAsync(ObservableList observableList)
  {
    var connection = await db.GetConnection();
    var list = observableList.ToItemList();
    var existingList = await connection
      .Table<List>()
      .Where(i => i.Id == observableList.Id)
      .FirstOrDefaultAsync();
    if (existingList != null)
    {
      await connection.UpdateAsync(list);
      logger.Info("Updated list: {List}", list.ToLoggableString());
      return;
    }

    await connection.InsertAsync(list);
    logger.Info("Created list: {List}", list.ToLoggableString());
    await CreateDefaultCategory(connection, list.Id);
  }

  private async Task CreateDefaultCategory(SQLiteAsyncConnection connection, string listId)
  {
    var existingDefaultCategory = await connection
      .Table<Models_Category>()
      .Where(i => i.ListId == listId && i.Name == Constants.DefaultCategoryName)
      .FirstOrDefaultAsync();

    if (existingDefaultCategory != null)
    {
      logger.Info("Default category already exists for list {ListID} - skipping creation", listId);
      return;
    }

    var observableCategory = new ObservableCategory(listId)
    {
      Name = Constants.DefaultCategoryName
    };
    var category = observableCategory.ToCategory();
    await connection.InsertAsync(category).ConfigureAwait(false);
    logger.Info(
      "Added category {DefaultCategory} to list {ListID}",
      Constants.DefaultCategoryName,
      listId
    );
  }

  public Task MarkAsUpdatedAsync(ObservableList observableList)
  {
    logger.Info(
      "Marking list as updated: {ListName} with ID {ListID}",
      observableList.Name,
      observableList.Id
    );
    observableList.UpdatedOn = DateTime.Now;
    return CreateOrUpdateAsync(observableList);
  }

  public async Task DeleteAsync(ObservableList observableList)
  {
    // TODO: Delete all categories and items associated with this list
    logger.Info(
      "Removing list: {ListName} with ID {ListID}",
      observableList.Name,
      observableList.Id
    );
    var connection = await db.GetConnection();
    var list = observableList.ToItemList();
    await connection.DeleteAsync(list);
  }

  public async Task DeleteAllAsync()
  {
    // TODO: Delete all categories and items associated with all lists
    var connection = await db.GetConnection();
    var allLists = await connection.Table<List>().ToListAsync();
    foreach (var list in allLists)
    {
      await connection.DeleteAsync(list);
    }

    logger.Info("Removed all lists");
  }
}
