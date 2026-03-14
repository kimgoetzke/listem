using Listem.Mobile.Models;
using Listem.Mobile.Services;
using Listem.Mobile.UnitTests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Listem.Mobile.UnitTests.Services;

[TestFixture]
public class CategoryServiceTests
{
  [Test]
  public async Task CreateOrUpdateAsync_InsertThenUpdate_UsesSingleRow()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new CategoryService(database, NullLogger<CategoryService>.Instance);
    var category = new ObservableCategory("list-1") { Name = "Dairy" };
    await service.CreateOrUpdateAsync(category);
    category.Name = "Fresh Dairy";

    await service.CreateOrUpdateAsync(category);

    var connection = await database.GetConnection();
    var stored = await connection.Table<Category>().ToListAsync();
    Assert.Multiple(() =>
    {
      Assert.That(stored, Has.Count.EqualTo(1));
      Assert.That(stored[0].Name, Is.EqualTo("Fresh Dairy"));
    });
  }

  [Test]
  public async Task GetAllByListIdAsync_ReturnsOnlyMatchingCategories()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new CategoryService(database, NullLogger<CategoryService>.Instance);
    await service.CreateOrUpdateAsync(
      new ObservableCategory("list-1") { Name = Constants.DefaultCategoryName }
    );
    await service.CreateOrUpdateAsync(new ObservableCategory("list-1") { Name = "Dairy" });
    await service.CreateOrUpdateAsync(
      new ObservableCategory("list-2") { Name = Constants.DefaultCategoryName }
    );

    var categories = await service.GetAllByListIdAsync("list-1");

    Assert.That(
      categories.Select(category => category.Name).ToArray(),
      Is.EqualTo([Constants.DefaultCategoryName, "Dairy"])
    );
  }

  [Test]
  public async Task DeleteAsync_RemovesCategory()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new CategoryService(database, NullLogger<CategoryService>.Instance);
    var category = new ObservableCategory("list-1") { Name = "Dairy" };
    await service.CreateOrUpdateAsync(category);

    await service.DeleteAsync(category);

    var connection = await database.GetConnection();
    var stored = await connection.Table<Category>().ToListAsync();
    Assert.That(stored, Is.Empty);
  }

  [Test]
  public async Task DeleteAllByListIdAsync_LeavesDefaultCategoryOnly()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new CategoryService(database, NullLogger<CategoryService>.Instance);
    await service.CreateOrUpdateAsync(
      new ObservableCategory("list-1") { Name = Constants.DefaultCategoryName }
    );
    await service.CreateOrUpdateAsync(new ObservableCategory("list-1") { Name = "Dairy" });
    await service.CreateOrUpdateAsync(new ObservableCategory("list-1") { Name = "Bakery" });

    await service.DeleteAllByListIdAsync("list-1");

    var connection = await database.GetConnection();
    var stored = await connection
      .Table<Category>()
      .Where(category => category.ListId == "list-1")
      .ToListAsync();
    Assert.Multiple(() =>
    {
      Assert.That(stored, Has.Count.EqualTo(1));
      Assert.That(stored[0].Name, Is.EqualTo(Constants.DefaultCategoryName));
    });
  }

  [Test]
  public async Task GetDefaultCategory_TwoLists_ReturnsMatchingDefaultPerList()
  {
    await using var database = await ServiceTestDatabaseProvider.CreateAsync();
    var service = new CategoryService(database, NullLogger<CategoryService>.Instance);
    var connection = await database.GetConnection();
    await connection.InsertAsync(
      new Category
      {
        Id = "cat-1",
        Name = Constants.DefaultCategoryName,
        ListId = "list-1"
      }
    );
    await connection.InsertAsync(
      new Category
      {
        Id = "cat-2",
        Name = Constants.DefaultCategoryName,
        ListId = "list-2"
      }
    );

    var firstDefault = await service.GetDefaultCategory("list-1");
    var secondDefault = await service.GetDefaultCategory("list-2");

    Assert.Multiple(() =>
    {
      Assert.That(firstDefault.ListId, Is.EqualTo("list-1"));
      Assert.That(secondDefault.ListId, Is.EqualTo("list-2"));
    });
  }
}
