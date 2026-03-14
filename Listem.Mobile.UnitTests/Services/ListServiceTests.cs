using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.UnitTests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using ModelCategory = Listem.Mobile.Models.Category;
using ModelList = Listem.Mobile.Models.List;

namespace Listem.Mobile.UnitTests.Services;

[TestFixture]
public class ListServiceTests
{
  [Test]
  public async Task CreateOrUpdateAsync_NewList_CreatesListAndDefaultCategory()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new ListService(database, NullLogger<ListService>.Instance);
    var observableList = new ObservableList
    {
      Name = "Groceries",
      ListType = ListType.Shopping,
      AddedOn = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
      UpdatedOn = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
    };

    await service.CreateOrUpdateAsync(observableList);

    var connection = await database.GetConnection();
    var lists = await connection.Table<ModelList>().ToListAsync();
    var categories = await connection
      .Table<ModelCategory>()
      .Where(category => category.ListId == observableList.Id)
      .ToListAsync();

    Assert.Multiple(() =>
    {
      Assert.That(lists, Has.Count.EqualTo(1));
      Assert.That(lists[0].Name, Is.EqualTo("Groceries"));
      Assert.That(categories, Has.Count.EqualTo(1));
      Assert.That(categories[0].Name, Is.EqualTo(Constants.DefaultCategoryName));
    });
  }

  [Test]
  public async Task CreateOrUpdateAsync_ExistingList_UpdatesWithoutAddingDuplicateDefaultCategory()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new ListService(database, NullLogger<ListService>.Instance);
    var observableList = new ObservableList
    {
      Name = "Weekly",
      AddedOn = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
      UpdatedOn = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
    };
    await service.CreateOrUpdateAsync(observableList);
    observableList.Name = "Weekly Updated";

    await service.CreateOrUpdateAsync(observableList);

    var connection = await database.GetConnection();
    var lists = await connection.Table<ModelList>().ToListAsync();
    var categories = await connection
      .Table<ModelCategory>()
      .Where(category => category.ListId == observableList.Id)
      .ToListAsync();

    Assert.Multiple(() =>
    {
      Assert.That(lists, Has.Count.EqualTo(1));
      Assert.That(lists[0].Name, Is.EqualTo("Weekly Updated"));
      Assert.That(categories, Has.Count.EqualTo(1));
      Assert.That(categories[0].Name, Is.EqualTo(Constants.DefaultCategoryName));
    });
  }

  [Test]
  public async Task MarkAsUpdatedAsync_UpdatesTimestamp()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new ListService(database, NullLogger<ListService>.Instance);
    var oldTimestamp = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var observableList = new ObservableList
    {
      Name = "Daily",
      AddedOn = oldTimestamp,
      UpdatedOn = oldTimestamp
    };
    await service.CreateOrUpdateAsync(observableList);

    await service.MarkAsUpdatedAsync(observableList);

    var connection = await database.GetConnection();
    var reloaded = await connection
      .Table<ModelList>()
      .Where(list => list.Id == observableList.Id)
      .FirstAsync();
    Assert.That(reloaded.UpdatedOn, Is.GreaterThan(oldTimestamp));
  }

  [Test]
  public async Task DeleteAllAsync_RemovesAllLists()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new ListService(database, NullLogger<ListService>.Instance);
    await service.CreateOrUpdateAsync(new ObservableList { Name = "One" });
    await service.CreateOrUpdateAsync(new ObservableList { Name = "Two" });

    await service.DeleteAllAsync();

    var connection = await database.GetConnection();
    var lists = await connection.Table<ModelList>().ToListAsync();
    Assert.That(lists, Is.Empty);
  }
}
