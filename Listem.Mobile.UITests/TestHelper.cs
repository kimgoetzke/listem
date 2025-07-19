using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;
using static Listem.Mobile.UITests.AutomationIdModel;

namespace Listem.Mobile.UITests;

public abstract class TestHelper : BaseTest
{
  public static class Act
  {
    public static void NavigateBackAndAwait(string elementId, int seconds = DefaultWaitSec)
    {
      var backButton = AwaitElement("BackButton");
      if (backButton == null)
      {
        throw new NoSuchElementException(
          $"Failed to find 'BackButton' within the {seconds} seconds."
        );
      }
      backButton.Click();
      Wait(seconds).Until(_ => Element(elementId).Displayed);
    }

    public static void HideKeyboard()
    {
      AppiumSetup.AppiumDriver.HideKeyboard();
    }

    public static class OnStartPage
    {
      public static void WaitForRedirect()
      {
        Wait(8).Until(_ => Element(MainPage.MenuButton).Displayed);
      }
    }

    public static class OnMainPage
    {
      public static void CreateList(string listName)
      {
        Element(MainPage.AddListButton).Click();
        Element(StickyEntry.EntryField).SendKeys(listName);
        Element(StickyEntry.SubmitButton).Click();
      }

      public static void SwipeRight(string fromElementId)
      {
        var windowSize = AppiumSetup.AppiumDriver.Manage().Window.Size;
        var element = Element(fromElementId);
        new Actions(AppiumSetup.AppiumDriver)
          .MoveToElement(element)
          .Pause(TimeSpan.FromMilliseconds(200))
          .ClickAndHold()
          .Pause(TimeSpan.FromMilliseconds(200))
          .MoveToElement(element, -windowSize.Width / 2, 0)
          .Perform();
      }

      public static void OpenMenu()
      {
        Element(MainPage.MenuButton).Click();
        Wait().Until(_ => Element(MainPage.Menu.DeleteDataButton).Displayed);
      }
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
        categories.ForEach(AddListCategory);
      }

      private static void AddListCategory(string category)
      {
        Element(EditListPage.AddCategoryButton).Click();
        Element(StickyEntry.EntryField).SendKeys(category);
        Element(StickyEntry.SubmitButton).Click();
      }

      public static void SwipeDeleteCategory(string category)
      {
        var windowSize = AppiumSetup.AppiumDriver.Manage().Window.Size;
        new Actions(AppiumSetup.AppiumDriver)
          .MoveToElement(Element(EditListPage.Categories.Label + category))
          .Pause(TimeSpan.FromMilliseconds(200))
          .ClickAndHold()
          .Pause(TimeSpan.FromMilliseconds(200))
          .MoveToElement(
            Element(EditListPage.Categories.Label + category),
            -windowSize.Width / 2,
            0
          )
          .Perform();
        Task.Delay(200);
        Element(EditListPage.Categories.BinIcon + category).Click();
      }
    }

    public static class OnListPage
    {
      public static void AddItemToList(TestData.TestItem item)
      {
        AddItemToList(item.Name, item.Category, item.Quantity, item.IsImportant);
      }

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
        var categoryPicker = AwaitElement(ListPage.CategoryPicker);
        categoryPicker!.Click();
        AwaitElementXPath(Alert.DropDownItem(categoryName))?.Click();

        // Quantity
        if (quantity > 1)
        {
          for (var i = 1; i < quantity; i++)
          {
            AwaitElementXPath(ListPage.Stepper.Increase)!.Click();
          }
        }

        // Importance
        var isImportantSwitch = Element(ListPage.IsImportantSwitch);
        if (isImportantSwitch.GetAttribute("checked").Equals("false") && isImportant)
        {
          isImportantSwitch.Click();
        }

        Element(ListPage.AddButton).Click();
        // Assert.That(categoryPicker.Text, Is.EqualTo(categoryName));
      }

      public static void SwipeDeleteItem(string itemName)
      {
        var windowSize = AppiumSetup.AppiumDriver.Manage().Window.Size;
        var listName = Element(ListPage.ListName);
        new Actions(AppiumSetup.AppiumDriver)
          .MoveToElement(Element(ListPage.Item.Label + itemName))
          .Pause(TimeSpan.FromMilliseconds(200))
          .ClickAndHold()
          .Pause(TimeSpan.FromMilliseconds(200))
          .MoveToElement(Element(ListPage.Item.Label + itemName), windowSize.Width, 0)
          .Perform();
        Task.Delay(200);
        new Actions(AppiumSetup.AppiumDriver)
          .MoveToElement(listName)
          .Pause(TimeSpan.FromMilliseconds(200))
          .Click()
          .Pause(TimeSpan.FromMilliseconds(200))
          .Perform();
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

      public static void ChangeItemQuantity(int by)
      {
        if (by > 0)
        {
          for (var i = 0; i < by; i++)
          {
            ElementXPath(DetailPage.IncreaseQuantityButton).Click();
          }
        }
        else
        {
          for (var i = 0; i < -by; i++)
          {
            ElementXPath(DetailPage.DecreaseQuantityButton).Click();
          }
        }
      }
    }
  }

  public static class AssertThat
  {
    public static void ElementDoesNotExist(
      string id,
      int seconds = DefaultWaitSec,
      int interval = DefaultIntervalMs
    )
    {
      Wait(seconds, interval)
        .Until(_ =>
        {
          var element = OptionalElement(id);
          return element == null;
        });
    }

    public static class OnMainPage
    {
      public static AppiumElement ListIsDisplayed(string listName)
      {
        var appiumElement = Element(MainPage.List.ListTitle + listName);
        Assert.That(appiumElement, Is.Not.Null, $"List '{listName}' should be displayed");
        return appiumElement;
      }

      public static void ListOrderIsCorrect(AppiumElement leftList, AppiumElement rightList)
      {
        var leftListX = leftList.Location.X;
        var rightListX = rightList.Location.X;
        Assert.That(leftListX, Is.LessThan(rightListX), "Left list should be before right list");
      }
    }

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

      public static void CategoryIsDeleted(string category)
      {
        Assert.That(OptionalElement(EditListPage.Categories.Label + category), Is.Null);
      }
    }

    public static class OnListPage
    {
      public static void ItemIsCreated(TestData.TestItem item)
      {
        ItemIsCreated(item.Name, item.Category, item.Quantity, item.IsImportant);
      }

      public static void ItemIsCreated(
        string itemName,
        string categoryName,
        int quantity,
        bool isImportant
      )
      {
        var label = AwaitElement(ListPage.Item.Label + itemName);
        var isImportantIcon = OptionalElement(ListPage.Item.IsImportantIcon + itemName);

        Assert.Multiple(() =>
        {
          // Name
          Assert.That(label?.Text, Is.EqualTo(itemName));

          // Category
          CategoryTagIsCorrect(itemName, categoryName);

          // Quantity
          QuantityIsCorrect(itemName, quantity);

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

      public static void ItemIsDeleted(TestData.TestItem item)
      {
        Assert.That(OptionalElement(ListPage.Item.Label + item.Name), Is.Null);
      }

      public static void CategoryTagIsCorrect(string itemName, string expectedCategory)
      {
        var categoryTag = OptionalElement(ListPage.Item.CategoryTag + itemName);
        if (expectedCategory != DefaultCategoryName)
        {
          Assert.Multiple(() =>
          {
            Assert.That(categoryTag!.Displayed, Is.True);
            Assert.That(categoryTag.FindElement(Id(Tag.Label))!.Text, Is.EqualTo(expectedCategory));
          });
        }
        else
        {
          Assert.That(categoryTag, Is.Null);
        }
      }

      public static void QuantityIsCorrect(string itemName, int expectedQuantity)
      {
        var quantityLabel = OptionalElement(ListPage.Item.QuantityLabel + itemName);
        switch (expectedQuantity)
        {
          case > 1:
          {
            Assert.That(quantityLabel!.Text, Is.EqualTo($" ({expectedQuantity.ToString()})"));
            break;
          }
          case 1:
          {
            Assert.That(quantityLabel, Is.Null);
            break;
          }
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

      public static void QuantityIsCorrect(int quantity)
      {
        Assert.That(Element(DetailPage.QuantityLabel).Text, Is.EqualTo(quantity.ToString()));
      }
    }
  }
}
