using System.Collections.ObjectModel;
using Listem.Mobile.Models;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.UnitTests.Utilities;

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

  [Test]
  public void FilterForPreview_RecurringList_ExcludesInactiveItems()
  {
    var items = new[]
    {
      new ObservableItem("list-1") { Title = "Active-1", IsActive = true },
      new ObservableItem("list-1") { Title = "Inactive-1", IsActive = false },
      new ObservableItem("list-1") { Title = "Active-2", IsActive = true },
      new ObservableItem("list-1") { Title = "Inactive-2", IsActive = false }
    };

    var filtered = ItemSorter.FilterForPreview(items, isRecurring: true).ToList();

    Assert.That(filtered.Select(i => i.Title).ToArray(), Is.EqualTo(new[] { "Active-1", "Active-2" }));
  }

  [Test]
  public void FilterForPreview_NonRecurringList_IncludesAllItems()
  {
    var items = new[]
    {
      new ObservableItem("list-1") { Title = "Item-1", IsActive = true },
      new ObservableItem("list-1") { Title = "Item-2", IsActive = false },
      new ObservableItem("list-1") { Title = "Item-3", IsActive = true }
    };

    var filtered = ItemSorter.FilterForPreview(items, isRecurring: false).ToList();

    Assert.That(filtered.Select(i => i.Title).ToArray(), Is.EqualTo(new[] { "Item-1", "Item-2", "Item-3" }));
  }

  [Test]
  public void SortInPlace_PreservesCollectionInstance_AndSortsCorrectly()
  {
    var items = new ObservableCollection<ObservableItem>(
    [
      new ObservableItem("list-1")
      {
        Title = "Item-1",
        CategoryName = "B",
        AddedOn = new DateTime(2026, 3, 13, 1, 0, 0, DateTimeKind.Utc)
      },
      new ObservableItem("list-1")
      {
        Title = "Item-2",
        CategoryName = "A",
        AddedOn = new DateTime(2026, 3, 13, 2, 0, 0, DateTimeKind.Utc)
      },
      new ObservableItem("list-1")
      {
        Title = "Item-3",
        CategoryName = "A",
        AddedOn = new DateTime(2026, 3, 13, 3, 0, 0, DateTimeKind.Utc)
      }
    ]);

    var originalReference = items;

    var sorted = ItemSorter.Sort(items, false).ToList();
    for (var i = 0; i < sorted.Count; i++)
    {
      var currentIndex = items.IndexOf(sorted[i]);
      if (currentIndex != i)
      {
        items.Move(currentIndex, i);
      }
    }

    Assert.That(items, Is.SameAs(originalReference), "Collection instance should be preserved");
    Assert.That(
      items.Select(item => item.Title).ToArray(),
      Is.EqualTo(["Item-3", "Item-2", "Item-1"])
    );
  }
}
