using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests;

[TestFixture]
public class OrderedTest : BaseTest
{
  private TestHelper TestHelper => new();

  [Test]
  [Order(1)]
  public void CanStartApp()
  {
    var isInstalled = App.IsAppInstalled(AppName);
    Console.WriteLine($"[XXX] {AppName} is installed: {isInstalled}");
    Wait(15).Until(_ => Element("SignInButton"));
    TakeScreenshot(nameof(CanStartApp));
  }

  [Test]
  [Order(2)]
  public void CanSignIn()
  {
    Element("SignInButton").Click();
    Wait().Until(_ => Element("EmailEntry").Displayed);
    Element("EmailEntry").SendKeys("someone@example");
    Element("PasswordEntry").SendKeys("Password1!");
    Element("SignInButton").Click();
    Wait(8).Until(_ => Element("MenuButton").Displayed);
    TakeScreenshot(nameof(CanSignIn));
  }

  [Test]
  [Order(10)]
  public void CanCreateList()
  {
    var currentList = TestData.Lists[0];
    Wait().Until(_ => Element("AddListButton").Displayed);
    Element("AddListButton").Click();
    Element("StickyEntryField").SendKeys(currentList.Name);
    Element("StickyEntrySubmit").Click();
    Wait().Until(_ => Element("ListTitle_" + currentList.Name).Displayed);
    TakeScreenshot(nameof(CanCreateList));
  }

  [Test]
  [Order(10)]
  public void CanNavigateFromMainPageToEditListPage()
  {
    var currentList = TestData.Lists[0];
    Element("EditList_" + currentList.Name).Click();
    Wait().Until(_ => Element("ListNameEntry").Displayed);
  }

  [Test]
  [Order(11)]
  public void CanConfigureList()
  {
    var currentList = TestData.Lists[0];
    var listName = Element("ListNameEntry");
    listName.Clear();
    listName.SendKeys("Edited" + currentList.Name);
    Element("ListTypePicker").Click();
    AwaitElementXPath(DropDownItemName(currentList.ListType))?.Click();
    currentList.Categories.ForEach(category =>
    {
      Element("AddCategoryButton").Click();
      Element("StickyEntryField").SendKeys(category);
      Element("StickyEntrySubmit").Click();
    });
    Element("BackButton").Click();
    var editedListName = AwaitElement("EditList_Edited" + currentList.Name)!;
    editedListName.Click();

    // TODO: Add assertions here

    listName = Element("ListNameEntry");
    listName.Clear();
    listName.SendKeys(currentList.Name);
  }

  [Test]
  [Order(19)]
  public void CanNavigateFromMainPageToListPage()
  {
    Element("ListTitle_" + TestData.Lists[0].Name).Click();
    Wait(5).Until(_ => Element("ListPageAddButton").Displayed);
    TakeScreenshot(nameof(CanCreateList));
  }

  [Test]
  [Order(20)]
  public void CanAddItemsToList()
  {
    TestData
      .Lists[0]
      .Items.ForEach(item =>
      {
        AddItemToList(item.Name, item.Category, item.Quantity, item.IsImportant);
        AssertThatItemIsCreated(item.Name, item.Category, item.Quantity, item.IsImportant);
      });
    TakeScreenshot(nameof(CanAddItemsToList));
  }

  [Test]
  [Order(21)]
  public void CanRemoveItems_TickAndLeaveList()
  {
    var currentList = TestData.Lists[0];
    Element("DoneBox_" + currentList.Items[0].Name).Click();
    Element("DoneBox_" + currentList.Items[1].Name).Click();
    Element("BackButton").Click();
    Wait(5).Until(_ => Element("MenuButton").Displayed);
    Element("ListTitle_" + currentList.Name).Click();
    Wait(5).Until(_ => Element("ListPageAddButton").Displayed);
    var item0 = OptionalElement("Label_" + currentList.Items[0].Name);
    var item1 = OptionalElement("Label_" + currentList.Items[1].Name);
    Assert.Multiple(() =>
    {
      Assert.That(item0, Is.Null);
      Assert.That(item1, Is.Null);
    });
  }

  [Test]
  [Order(22)]
  public void CanRemoveItems_SwipeComplete()
  {
    // TODO: Add swipe to complete test
    // var item2 = OptionalElement("Label_" + TestData.Items[2].Name);
    // var item3 = OptionalElement("Label_" + TestData.Items[3].Name);
    // Assert.Multiple(() =>
    // {
    //   Assert.That(item2, Is.Null);
    //   Assert.That(item3, Is.Null);
    // });
  }

  [Test]
  [Order(29)]
  public void CanGoBackToMainPage()
  {
    Element("BackButton").Click();
    Wait(5).Until(_ => Element("MenuButton").Displayed);
  }

  [Test]
  [Order(30)]
  public void CanDeleteList()
  {
    var currentList = TestData.Lists[0];
    Element("DeleteButton_" + currentList.Name).Click();
    var no = ElementXPath(NoButton());
    no.Click();
    Assert.That(OptionalElement("ListTitle_" + currentList.Name), Is.Not.Null);

    Element("DeleteButton_" + currentList.Name).Click();
    var yes = ElementXPath(YesButton());
    yes.Click();
    Assert.That(OptionalElement("ListTitle_" + currentList.Name), Is.Null);
  }

  [Test]
  [Order(99)]
  [Ignore("Temporarily disabled")]
  public void CanSignOutFromMainPage()
  {
    Wait().Until(_ => Element("MenuButton").Displayed);
    Element("MenuButton").Click();
    Element("SignUpInOrOutButton").Click();
    Wait(5).Until(_ => Element("SignInButton"));
    TakeScreenshot(nameof(CanSignOutFromMainPage));
  }
}
