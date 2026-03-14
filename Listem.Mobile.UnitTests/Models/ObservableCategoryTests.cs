using Listem.Mobile.Models;

namespace Listem.Mobile.UnitTests.Models;

[TestFixture]
public class ObservableCategoryTests
{
  [Test]
  public void Constructor_SetsListId()
  {
    var category = new ObservableCategory("list-1");

    Assert.That(category.ListId, Is.EqualTo("list-1"));
  }

  [Test]
  public void From_MapsIdNameAndListId()
  {
    var category = new Category
    {
      Id = "cat-1",
      Name = "Bakery",
      ListId = "list-42"
    };

    var observableCategory = ObservableCategory.From(category);

    Assert.Multiple(() =>
    {
      Assert.That(observableCategory.Id, Is.EqualTo("cat-1"));
      Assert.That(observableCategory.Name, Is.EqualTo("Bakery"));
      Assert.That(observableCategory.ListId, Is.EqualTo("list-42"));
    });
  }

  [Test]
  public void ToCategory_WhenIdMissing_GeneratesIdAndMapsProperties()
  {
    var observableCategory = new ObservableCategory("list-1") { Name = "Fruit" };

    var category = observableCategory.ToCategory();

    Assert.Multiple(() =>
    {
      Assert.That(category.Id, Is.Not.Null.And.Contains("~"));
      Assert.That(category.Name, Is.EqualTo("Fruit"));
      Assert.That(category.ListId, Is.EqualTo("list-1"));
    });
  }

  [Test]
  public void ToLoggableString_ContainsNameAndListId()
  {
    var observableCategory = new ObservableCategory("list-1") { Name = "Dairy", Id = "cat-1" };

    var loggable = observableCategory.ToLoggableString();

    Assert.That(loggable, Does.Contain("'Dairy'").And.Contain("in list-1"));
  }
}
