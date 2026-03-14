using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.UnitTests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using ModelItem = Listem.Mobile.Models.Item;

namespace Listem.Mobile.UnitTests.Services;

[TestFixture]
public class ItemServiceTests
{
  [Test]
  public async Task CreateOrUpdateAsync_InsertThenUpdate_UsesSingleRow()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new ItemService(database, NullLogger<ItemService>.Instance);
    var observableItem = new ObservableItem("list-1")
    {
      Title = "Milk",
      Quantity = 1,
      IsImportant = false,
      IsActive = true,
      CategoryName = Constants.DefaultCategoryName
    };
    await service.CreateOrUpdateAsync(observableItem);
    observableItem.Quantity = 3;

    await service.CreateOrUpdateAsync(observableItem);

    var connection = await database.GetConnection();
    var stored = await connection.Table<ModelItem>().ToListAsync();
    Assert.Multiple(() =>
    {
      Assert.That(stored, Has.Count.EqualTo(1));
      Assert.That(stored[0].Quantity, Is.EqualTo(3));
    });
  }

  [Test]
  public async Task GetAllByListIdAsync_ReturnsOnlyMatchingListItems()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new ItemService(database, NullLogger<ItemService>.Instance);
    await service.CreateOrUpdateAsync(
      new ObservableItem("list-1") { Title = "Milk", CategoryName = Constants.DefaultCategoryName }
    );
    await service.CreateOrUpdateAsync(
      new ObservableItem("list-2") { Title = "Bread", CategoryName = Constants.DefaultCategoryName }
    );

    var items = await service.GetAllByListIdAsync("list-1");

    Assert.That(items.Select(item => item.Title).ToArray(), Is.EqualTo(["Milk"]));
  }

  [Test]
  public async Task DeleteAsync_RemovesItem()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new ItemService(database, NullLogger<ItemService>.Instance);
    var item = new ObservableItem("list-1")
    {
      Title = "Cheese",
      CategoryName = Constants.DefaultCategoryName
    };
    await service.CreateOrUpdateAsync(item);

    await service.DeleteAsync(item);

    var connection = await database.GetConnection();
    var stored = await connection.Table<ModelItem>().ToListAsync();
    Assert.That(stored, Is.Empty);
  }

  [Test]
  public async Task UpdateAllToDefaultCategoryAsync_UpdatesNonDefaultCategories()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new ItemService(database, NullLogger<ItemService>.Instance);
    await service.CreateOrUpdateAsync(
      new ObservableItem("list-1") { Title = "Yoghurt", CategoryName = "Dairy" }
    );
    await service.CreateOrUpdateAsync(
      new ObservableItem("list-1") { Title = "Bread", CategoryName = Constants.DefaultCategoryName }
    );

    await service.UpdateAllToDefaultCategoryAsync("list-1");

    var connection = await database.GetConnection();
    var stored = await connection
      .Table<ModelItem>()
      .Where(item => item.ListId == "list-1")
      .ToListAsync();
    Assert.That(stored.All(item => item.CategoryName == Constants.DefaultCategoryName), Is.True);
  }

  [Test]
  public async Task UpdateAllToCategoryAsync_UpdatesOnlySpecifiedCategory()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new ItemService(database, NullLogger<ItemService>.Instance);
    await service.CreateOrUpdateAsync(
      new ObservableItem("list-1") { Title = "Apple", CategoryName = "Fruit" }
    );
    await service.CreateOrUpdateAsync(
      new ObservableItem("list-1") { Title = "Milk", CategoryName = "Dairy" }
    );

    await service.UpdateAllToCategoryAsync("Fruit", "list-1");

    var connection = await database.GetConnection();
    var stored = await connection
      .Table<ModelItem>()
      .Where(item => item.ListId == "list-1")
      .ToListAsync();
    var apple = stored.First(item => item.Title == "Apple");
    var milk = stored.First(item => item.Title == "Milk");
    Assert.Multiple(() =>
    {
      Assert.That(apple.CategoryName, Is.EqualTo(Constants.DefaultCategoryName));
      Assert.That(milk.CategoryName, Is.EqualTo("Dairy"));
    });
  }
}
