using static Listem.Mobile.UITests.AutomationIdModel;

namespace Listem.Mobile.UITests;

public abstract class TestHelper : BaseTest
{
  // ------------------------------ Act ------------------------------

  public static void UpdateListSettings(string listNamePrefix, TestData.TestList currentList)
  {
    var listName = Element(EditListPage.ListNameEntry);
    listName.Clear();
    listName.SendKeys(listNamePrefix + currentList.Name);
    Element(EditListPage.ListTypePicker).Click();
    AwaitElementXPath(DropDownItemName(currentList.ListType))?.Click();
    currentList.Categories.ForEach(category =>
    {
      Element(EditListPage.AddCategoryButton).Click();
      Element(StickyEntry.EntryField).SendKeys(category);
      Element(StickyEntry.SubmitButton).Click();
    });
  }

  public static void AddItemToList(
    string itemName,
    string categoryName,
    int quantity,
    bool isImportant
  )
  {
    // Name
    Element(ListPage.EntryField).SendKeys(itemName);

    // Category
    var categoryPicker = Element(ListPage.CategoryPicker);
    categoryPicker.Click();
    AwaitElementXPath(DropDownItemName(categoryName))?.Click();

    // Quantity
    if (quantity > 1)
    {
      for (var i = 1; i < quantity; i++)
      {
        AwaitElementXPath(StepperIncrease())!.Click();
      }
    }

    // Importance
    var isImportantSwitch = Element(ListPage.IsImportantSwitch);
    if (isImportantSwitch.GetAttribute("checked").Equals("false") && isImportant)
    {
      isImportantSwitch.Click();
    }

    Element(ListPage.AddButton).Click();
    Assert.That(categoryPicker.Text, Is.EqualTo(categoryName));
  }

  // ------------------------------ Assert ------------------------------

  public static void AssertThatListIsUpdated(string prefix, TestData.TestList currentList)
  {
    var listName = AwaitElement(EditListPage.ListNameEntry)!;
    var listCategory = Element(EditListPage.ListTypePicker);

    Assert.Multiple(() =>
    {
      Assert.That(listName.Text, Is.EqualTo(prefix + currentList.Name));
      Assert.That(listCategory.Text, Is.EqualTo(TestData.Lists[0].ListType));

      TestData
        .Lists[0]
        .Categories.ForEach(category =>
        {
          var swipeItem = Element(EditListPage.Categories.Label + category);
          Assert.That(swipeItem.Displayed);
          Assert.That(swipeItem.Text, Is.EqualTo(category));
        });
    });
  }

  public static void AssertThatItemIsCreated(
    string itemName,
    string categoryName,
    int quantity,
    bool isImportant
  )
  {
    var label = AwaitElement(ListPage.Item.Label + itemName);
    var categoryTag = OptionalElement(ListPage.Item.CategoryTag + itemName);
    var quantityLabel = OptionalElement(ListPage.Item.QuantityLabel + itemName);
    var isImportantIcon = OptionalElement(ListPage.Item.IsImportantIcon + itemName);

    Assert.Multiple(() =>
    {
      // Name
      Assert.That(label?.Text, Is.EqualTo(itemName));

      // Category
      if (categoryName != DefaultCategoryName)
      {
        Assert.That(categoryTag!.Displayed, Is.True);
        Assert.That(categoryTag.FindElement(Id(Tag.Label))!.Text, Is.EqualTo(categoryName));
      }
      else
      {
        Assert.That(categoryTag, Is.Null);
      }

      // Quantity
      switch (quantity)
      {
        case > 1:
        {
          Assert.That(quantityLabel!.Text, Is.EqualTo($" ({quantity.ToString()})"));
          break;
        }
        case 1:
        {
          Assert.That(quantityLabel, Is.Null);
          break;
        }
      }

      // Importance
      if (isImportant)
      {
        Assert.That(isImportantIcon!.Displayed, Is.True);
      }
      else
      {
        Assert.That(isImportantIcon, Is.Null);
      }
    });
  }
}
