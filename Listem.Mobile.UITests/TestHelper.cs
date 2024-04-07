namespace Listem.Mobile.UITests;

public class TestHelper : BaseTest
{
  public static void AddItemToList(
    string itemName,
    string categoryName,
    int quantity,
    bool isImportant
  )
  {
    // Name
    Element("ListPageEntryField").SendKeys(itemName);

    // Category
    var categoryPicker = Element("ListPageCategoryPicker");
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
    var isImportantSwitch = Element("ListPageIsImportantSwitch");
    if (isImportantSwitch.GetAttribute("checked").Equals("false") && isImportant)
    {
      isImportantSwitch.Click();
    }

    Element("ListPageAddButton").Click();
    Assert.That(categoryPicker.Text, Is.EqualTo(categoryName));
  }

  public static void AssertThatItemIsCreated(
    string itemName,
    string categoryName,
    int quantity,
    bool isImportant
  )
  {
    var label = AwaitElement("Label_" + itemName);
    var categoryTag = OptionalElement("CategoryTag_" + itemName);
    var quantityLabel = OptionalElement("QuantityLabel_" + itemName);
    var isImportantIcon = OptionalElement("IsImportantIcon_" + itemName);

    Assert.Multiple(() =>
    {
      // Name
      Assert.That(label?.Text, Is.EqualTo(itemName));

      // Category
      if (categoryName != TestData.DefaultCategoryName)
      {
        Assert.That(categoryTag!.Displayed, Is.True);
        Assert.That(categoryTag.FindElement(Id("TagLabel"))!.Text, Is.EqualTo(categoryName));
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
