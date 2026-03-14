using Listem.Mobile.Models;

namespace Listem.Mobile.UnitTests.Models;

[TestFixture]
public class ObservableListTests
{
  [Test]
  public void NewObservableList_UsesExpectedDefaults()
  {
    var observableList = new ObservableList();

    Assert.Multiple(() =>
    {
      Assert.That(observableList.Name, Is.EqualTo(string.Empty));
      Assert.That(observableList.ListType, Is.EqualTo(ListType.Standard));
      Assert.That(observableList.Items, Is.Empty);
      Assert.That(observableList.IsRecurring, Is.False);
    });
  }

  [Test]
  public void From_MapsAllProperties()
  {
    var list = new List
    {
      Id = "lst-1",
      Name = "Groceries",
      ListType = ListType.Shopping,
      IsRecurring = true,
      AddedOn = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
      UpdatedOn = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc)
    };

    var observableList = ObservableList.From(list);

    Assert.Multiple(() =>
    {
      Assert.That(observableList.Id, Is.EqualTo("lst-1"));
      Assert.That(observableList.Name, Is.EqualTo("Groceries"));
      Assert.That(observableList.ListType, Is.EqualTo(ListType.Shopping));
      Assert.That(observableList.IsRecurring, Is.True);
      Assert.That(observableList.AddedOn, Is.EqualTo(list.AddedOn));
      Assert.That(observableList.UpdatedOn, Is.EqualTo(list.UpdatedOn));
    });
  }

  [Test]
  public void ToItemList_WhenIdMissing_GeneratesIdAndMapsProperties()
  {
    var observableList = new ObservableList
    {
      Name = "Weekly",
      ListType = ListType.Standard,
      IsRecurring = false,
      AddedOn = new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc),
      UpdatedOn = new DateTime(2026, 3, 4, 0, 0, 0, DateTimeKind.Utc)
    };

    var list = observableList.ToItemList();

    Assert.Multiple(() =>
    {
      Assert.That(list.Id, Is.Not.Null.And.Contains("~"));
      Assert.That(list.Name, Is.EqualTo("Weekly"));
      Assert.That(list.ListType, Is.EqualTo(ListType.Standard));
      Assert.That(list.IsRecurring, Is.False);
      Assert.That(list.AddedOn, Is.EqualTo(observableList.AddedOn));
      Assert.That(list.UpdatedOn, Is.EqualTo(observableList.UpdatedOn));
    });
  }

  [Test]
  public void ToLoggableString_ContainsNameAndItemCount()
  {
    var observableList = new ObservableList { Name = "Groceries", Id = "lst-1" };
    observableList.Items.Add(new ObservableItem("lst-1") { Title = "Milk" });

    var loggable = observableList.ToLoggableString();

    Assert.That(loggable, Does.Contain("'Groceries'").And.Contain("with 1 items"));
  }
}
