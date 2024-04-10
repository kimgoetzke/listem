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

    public static class OnStartPage
    {
      public static void SignIn(TestData.TestUser user)
      {
        Element(StartPage.SignInButton).Click();
        Wait().Until(_ => Element(SignInPage.SignInButton).Displayed);
        Element(SignInPage.EmailEntry).SendKeys(user.Email);
        Element(SignInPage.PasswordEntry).SendKeys(user.Password);
        Element(SignInPage.SignInButton).Click();
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

      public static void SignOut()
      {
        Element(MainPage.MenuButton).Click();
        AwaitElement(MainPage.Menu.SignOutButton)!.Click();
        Wait().Until(_ => Element(StartPage.SignInButton).Displayed);
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
    public static class OnMainPage
    {
      public static void ListTagsAreCorrect(string listName, bool isShared, bool isOwner = false)
      {
        Assert.Multiple(() =>
        {
          if (!isShared)
          {
            Assert.That(OptionalElement(MainPage.List.Tags.Shared + listName), Is.Null);
            Assert.That(OptionalElement(MainPage.List.Tags.Owner + listName), Is.Null);
            Assert.That(OptionalElement(MainPage.List.Tags.Collaborator + listName), Is.Null);
            return;
          }
          Assert.That(Element(MainPage.List.Tags.Shared + listName).Displayed, Is.True);
          Assert.That(
            isOwner
              ? Element(MainPage.List.Tags.Owner + listName).Displayed
              : Element(MainPage.List.Tags.Collaborator + listName).Displayed,
            Is.True
          );
        });
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

      public static void List(bool isShared, TestData.TestUser user, bool ignoreButton = false)
      {
        Assert.Multiple(() =>
        {
          if (isShared)
          {
            Assert.That(Element(EditListPage.Collaborators.Label + user.Id).Displayed, Is.True);
            if (!ignoreButton)
            {
              Assert.That(AwaitElement(EditListPage.UnshareButton)!.Displayed, Is.True);
            }
          }
          else
          {
            Assert.That(OptionalElement(EditListPage.Collaborators.Label + user.Id), Is.Null);
            if (!ignoreButton)
            {
              Assert.That(OptionalElement(EditListPage.UnshareButton), Is.Null);
            }
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
