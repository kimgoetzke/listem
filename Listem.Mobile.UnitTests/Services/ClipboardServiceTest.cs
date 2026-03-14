using System.Collections.ObjectModel;
using Listem.Mobile.Models;
using Listem.Mobile.Services;

namespace Listem.Mobile.UnitTests.Services;

[TestFixture]
public class ClipboardServiceTest
{
  [Test]
  public void ShouldIncludeInExport_NonRecurring_AlwaysTrue()
  {
    Assert.That(ClipboardService.ShouldIncludeInExport(false, true), Is.True);
    Assert.That(ClipboardService.ShouldIncludeInExport(false, false), Is.True);
  }

  [Test]
  public void ShouldIncludeInExport_Recurring_OnlyActiveItems()
  {
    Assert.That(ClipboardService.ShouldIncludeInExport(true, true), Is.True);
    Assert.That(ClipboardService.ShouldIncludeInExport(true, false), Is.False);
  }

  [Test]
  public void ResolveIsImportantOnImport_Recurring_IgnoresParsedImportantFlag()
  {
    Assert.That(ClipboardService.ResolveIsImportantOnImport(true, true), Is.False);
    Assert.That(ClipboardService.ResolveIsImportantOnImport(true, false), Is.False);
  }

  [Test]
  public void ResolveIsImportantOnImport_NonRecurring_UsesParsedImportantFlag()
  {
    Assert.That(ClipboardService.ResolveIsImportantOnImport(false, true), Is.True);
    Assert.That(ClipboardService.ResolveIsImportantOnImport(false, false), Is.False);
  }

  [Test]
  public void ResolveIsActiveOnImport_AlwaysTrue()
  {
    Assert.That(ClipboardService.ResolveIsActiveOnImport(), Is.True);
  }

  [Test]
  public void BuildClipboardTextForExport_NonRecurring_UsesQuantityAndImportantFormatting()
  {
    var categories = new ObservableCollection<ObservableCategory>
    {
      new("list-1") { Name = "Dairy" },
      new("list-1") { Name = "Bakery" }
    };
    var items = new ObservableCollection<ObservableItem>
    {
      new("list-1")
      {
        Title = "Milk",
        Quantity = 2,
        IsImportant = true,
        IsActive = true,
        CategoryName = "Dairy"
      },
      new("list-1")
      {
        Title = "Bread",
        Quantity = 1,
        IsImportant = false,
        IsActive = false,
        CategoryName = "Bakery"
      }
    };

    var export = ClipboardService.BuildStringFromList(items, categories, isRecurring: false);

    var expected = string.Join(
      Environment.NewLine,
      ["[Dairy]:", "Milk (2)!", string.Empty, "[Bakery]:", "Bread"]
    );
    Assert.That(export, Is.EqualTo(expected));
  }

  [Test]
  public void BuildClipboardTextForExport_Recurring_ExcludesInactiveAndImportantMarker()
  {
    var categories = new ObservableCollection<ObservableCategory>
    {
      new("list-1") { Name = "Dairy" }
    };
    var items = new ObservableCollection<ObservableItem>
    {
      new("list-1")
      {
        Title = "Yoghurt",
        Quantity = 1,
        IsImportant = true,
        IsActive = true,
        CategoryName = "Dairy"
      },
      new("list-1")
      {
        Title = "Cheese",
        Quantity = 1,
        IsImportant = true,
        IsActive = false,
        CategoryName = "Dairy"
      }
    };

    var export = ClipboardService.BuildStringFromList(items, categories, isRecurring: true);

    var expected = string.Join(Environment.NewLine, ["[Dairy]:", "Yoghurt"]);
    Assert.That(export, Is.EqualTo(expected));
  }

  [Test]
  public void BuildClipboardTextForExport_NoMatchingItems_ReturnsEmptyString()
  {
    var categories = new ObservableCollection<ObservableCategory>
    {
      new("list-1") { Name = "Dairy" }
    };
    var items = new ObservableCollection<ObservableItem>
    {
      new("list-1")
      {
        Title = "Banana",
        Quantity = 1,
        IsImportant = false,
        IsActive = true,
        CategoryName = "Fruit"
      }
    };

    var export = ClipboardService.BuildStringFromList(items, categories, isRecurring: false);

    Assert.That(export, Is.EqualTo(string.Empty));
  }
}
