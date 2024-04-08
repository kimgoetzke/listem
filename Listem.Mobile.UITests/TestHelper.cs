using static Listem.Mobile.UITests.AutomationIdModel;

namespace Listem.Mobile.UITests;

public abstract class TestHelper : BaseTest
{
  public static class Act
  {
    public static void NavigateBackAndAwait(string elementId)
    {
      Element("BackButton").Click();
      Wait().Until(_ => Element(elementId).Displayed);
    }

    public static class OnEditListPage
    {
      public static void ChangeListName(string name, string prefix = "")
      {
        var listName = Element(EditListPage.ListNameEntry);
        listName.Clear();
        listName.SendKeys(prefix + name);
      }

      public static void ChangeListType(string listType)
      {
        Element(EditListPage.ListTypePicker).Click();
        AwaitElementXPath(Alert.DropDownItem(listType))?.Click();
      }

      public static void AddListCategories(List<string> categories)
      {
        categories.ForEach(category =>
        {
          Element(EditListPage.AddCategoryButton).Click();
          Element(StickyEntry.EntryField).SendKeys(category);
          Element(StickyEntry.SubmitButton).Click();
        });
      }
    }

    public static class OnListPage
    {
      public static void AddItemToList(
        string itemName,
        string categoryName,
        int quantity,
        bool isImportant
      )
      {
        // Name
        Element(ListPage.NameEntry).SendKeys(itemName);

        // Category
        var categoryPicker = Element(ListPage.CategoryPicker);
        categoryPicker.Click();
        AwaitElementXPath(Alert.DropDownItem(categoryName))?.Click();

        // Quantity
        if (quantity > 1)
        {
          for (var i = 1; i < quantity; i++)
          {
            AwaitElementXPath(Stepper.Increase)!.Click();
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
    }

    public static class OnDetailPage
    {
      public static void ChangeItemName(string name, string prefix = "")
      {
        var itemName = Element(DetailPage.NameEntry);
        itemName.Clear();
        itemName.SendKeys(prefix + name);
      }

      public static void ChangeItemCategory(string category)
      {
        Element(DetailPage.CategoryPicker).Click();
        AwaitElementXPath(Alert.DropDownItem(category))?.Click();
      }

      public static void SetItemIsImportant(bool isImportant)
      {
        var element = Element(DetailPage.IsImportantSwitch);
        if (!element.GetAttribute("checked").ToLower().Equals(isImportant.ToString().ToLower()))
        {
          element.Click();
        }
      }
    }
  }

  public static class AssertThat
  {
    public static class OnEditListPage
    {
      public static void Categories(bool areVisible, List<string> categories)
      {
        Assert.Multiple(() =>
        {
          categories.ForEach(category =>
          {
            var swipeItem = OptionalElement(EditListPage.Categories.Label + category);
            if (areVisible)
            {
              Assert.That(swipeItem, Is.Not.Null);
              Assert.That(swipeItem!.Text, Is.EqualTo(category));
            }
            else
            {
              Assert.That(swipeItem, Is.Null);
            }
          });
        });
      }

      public static void List(bool isShared, TestData.TestUser user)
      {
        Assert.Multiple(() =>
        {
          if (isShared)
          {
            Assert.That(AwaitElement(EditListPage.UnshareButton)!.Displayed, Is.True);
            Assert.That(Element(EditListPage.Collaborators.Label + user.id).Displayed, Is.True);
          }
          else
          {
            Assert.That(OptionalElement(EditListPage.UnshareButton), Is.Null);
            Assert.That(OptionalElement(EditListPage.Collaborators.Label + user.id), Is.Null);
          }
        });
      }
    }

    public static class OnListPage
    {
      public static void ItemIsCreated(
        string itemName,
        string categoryName,
        int quantity,
        bool isImportant
      )
      {
        var label = AwaitElement(ListPage.Item.Label + itemName);

        var quantityLabel = OptionalElement(ListPage.Item.QuantityLabel + itemName);
        var isImportantIcon = OptionalElement(ListPage.Item.IsImportantIcon + itemName);

        Assert.Multiple(() =>
        {
          // Name
          Assert.That(label?.Text, Is.EqualTo(itemName));

          // Category
          CategoryTagIsCorrect(itemName, categoryName);

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

      public static void CategoryTagIsCorrect(string itemName, string categoryName)
      {
        var categoryTag = OptionalElement(ListPage.Item.CategoryTag + itemName);
        if (categoryName != DefaultCategoryName)
        {
          Assert.Multiple(() =>
          {
            Assert.That(categoryTag!.Displayed, Is.True);
            Assert.That(categoryTag.FindElement(Id(Tag.Label))!.Text, Is.EqualTo(categoryName));
          });
        }
        else
        {
          Assert.That(categoryTag, Is.Null);
        }
      }
    }

    public static class OnDetailPage
    {
      public static void ItemIsImportant(bool isImportant)
      {
        var isImportantSwitch = Element(DetailPage.IsImportantSwitch);
        Assert.That(
          isImportantSwitch.GetAttribute("checked").ToLower(),
          Is.EqualTo(isImportant.ToString().ToLower())
        );
      }
    }
  }
}
