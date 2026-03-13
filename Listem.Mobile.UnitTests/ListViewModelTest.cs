using Listem.Mobile.Models;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.UnitTests;

[TestFixture]
public class ListViewModelTest
{
  [Test]
  public void Sort_NonRecurring_UsesCategoryAndAddedOn()
  {
    var items = new[]
    {
      new ObservableItem("list-1")
      {
        Title = "Item-1",
        CategoryName = "B",
        AddedOn = new DateTime(2026, 3, 13, 1, 0, 0, DateTimeKind.Utc),
        IsActive = false
      },
      new ObservableItem("list-1")
      {
        Title = "Item-2",
        CategoryName = "A",
        AddedOn = new DateTime(2026, 3, 13, 2, 0, 0, DateTimeKind.Utc),
        IsActive = false
      },
      new ObservableItem("list-1")
      {
        Title = "Item-3",
        CategoryName = "A",
        AddedOn = new DateTime(2026, 3, 13, 3, 0, 0, DateTimeKind.Utc),
        IsActive = true
      }
    };

    var sorted = ItemSorter.Sort(items, false).ToList();

    Assert.That(
      sorted.Select(item => item.Title).ToArray(),
      Is.EqualTo(["Item-3", "Item-2", "Item-1"])
    );
  }

  [Test]
  public void Sort_Recurring_PutsInactiveItemsAtBottom()
  {
    var items = new[]
    {
      new ObservableItem("list-1")
      {
        Title = "Inactive-1",
        CategoryName = "A",
        AddedOn = new DateTime(2026, 3, 13, 3, 0, 0, DateTimeKind.Utc),
        IsActive = false
      },
      new ObservableItem("list-1")
      {
        Title = "Active-1",
        CategoryName = "B",
        AddedOn = new DateTime(2026, 3, 13, 1, 0, 0, DateTimeKind.Utc),
        IsActive = true
      },
      new ObservableItem("list-1")
      {
        Title = "Active-2",
        CategoryName = "A",
        AddedOn = new DateTime(2026, 3, 13, 2, 0, 0, DateTimeKind.Utc),
        IsActive = true
      }
    };

    var sorted = ItemSorter.Sort(items, true).ToList();

    Assert.That(
      sorted.Select(item => item.Title).ToArray(),
      Is.EqualTo(["Active-2", "Active-1", "Inactive-1"])
    );
  }
}
