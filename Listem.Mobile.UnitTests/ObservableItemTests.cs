using Listem.Mobile.Models;

namespace Listem.Mobile.UnitTests;

[TestFixture]
public class ObservableItemTests
{
  [Test]
  public void NewItem_DefaultsToActive()
  {
    var item = new ObservableItem("list-1");
    Assert.That(item.IsActive, Is.True);
  }

  [Test]
  public void From_MapsIsActiveAndIsImportant()
  {
    var item = new Item
    {
      Id = "itm-1",
      ListId = "lst-1",
      Title = "Milk",
      Quantity = 2,
      IsImportant = true,
      IsActive = false,
      CategoryName = "Dairy",
      AddedOn = new DateTime(2026, 3, 13, 0, 0, 0, DateTimeKind.Utc)
    };

    var observableItem = ObservableItem.From(item);

    Assert.Multiple(() =>
    {
      Assert.That(observableItem.IsImportant, Is.True);
      Assert.That(observableItem.IsActive, Is.False);
      Assert.That(observableItem.Quantity, Is.EqualTo(2));
      Assert.That(observableItem.CategoryName, Is.EqualTo("Dairy"));
    });
  }

  [Test]
  public void ToItem_MapsIsActiveAndIsImportant()
  {
    var observableItem = new ObservableItem("lst-1")
    {
      Id = "itm-2",
      Title = "Bread",
      Quantity = 1,
      IsImportant = false,
      IsActive = true,
      CategoryName = "Bakery",
      AddedOn = new DateTime(2026, 3, 13, 0, 0, 0, DateTimeKind.Utc)
    };

    var item = observableItem.ToItem();

    Assert.Multiple(() =>
    {
      Assert.That(item.IsImportant, Is.False);
      Assert.That(item.IsActive, Is.True);
      Assert.That(item.Title, Is.EqualTo("Bread"));
      Assert.That(item.ListId, Is.EqualTo("lst-1"));
    });
  }
}
