using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests;

[TestFixture]
public class OrderedTest : BaseTest
{
  [Test]
  [Order(1)]
  public void CanStartApp()
  {
    Console.WriteLine($"[XXX] {AppName} is installed: {App.IsAppInstalled(AppName)}");
    Wait(15).Until(_ => Element(StartPage.SignInButton).Displayed);
    TakeScreenshot(nameof(CanStartApp));
  }

  [Test]
  [Order(2)]
  public void CanSignIn()
  {
    var currentUser = TestData.Users[0];
    Element(StartPage.SignInButton).Click();
    Wait().Until(_ => Element(SignInPage.SignInButton).Displayed);
    Element(SignInPage.EmailEntry).SendKeys(currentUser.Email);
    Element(SignInPage.PasswordEntry).SendKeys(currentUser.Password);
    Element(SignInPage.SignInButton).Click();
    Wait(8).Until(_ => Element(MainPage.MenuButton).Displayed);
    TakeScreenshot(nameof(CanSignIn));
  }

  [Test]
  [Order(10)]
  public void CanCreateList()
  {
    var currentList = TestData.Lists[0];
    Wait().Until(_ => Element(MainPage.AddListButton).Displayed);
    Element(MainPage.AddListButton).Click();
    Element(StickyEntry.EntryField).SendKeys(currentList.Name);
    Element(StickyEntry.SubmitButton).Click();
    Wait().Until(_ => Element(MainPage.List.ListTitle + currentList.Name).Displayed);
    TakeScreenshot(nameof(CanCreateList));
  }

  [Test]
  [Order(11)]
  public void CanNavigateFromMainPageToEditListPage()
  {
    var currentList = TestData.Lists[0];
    Element(MainPage.List.EditButton + currentList.Name).Click();
    Wait().Until(_ => Element(EditListPage.ListNameEntry).Displayed);
  }

  [Test]
  [Order(12)]
  public void CanConfigureList()
  {
    const string listNamePrefix = "Edited";
    var currentList = TestData.Lists[0];
    UpdateListSettings(listNamePrefix, currentList);
    Element(EditListPage.BackButton).Click(); // Go back so that the settings are saved, then navigate back
    AwaitElement(MainPage.List.EditButton + listNamePrefix + currentList.Name)!.Click();
    AssertThatListIsUpdated(listNamePrefix, currentList);
    var listName = AwaitElement(EditListPage.ListNameEntry)!;
    listName.Clear();
    listName.SendKeys(currentList.Name); // Reset list name for the further tests and go back
    TakeScreenshot(nameof(CanConfigureList));
  }

  [Test]
  [Order(11)]
  public void CanNavigateFromEditListPageToMainPage()
  {
    Element(EditListPage.BackButton).Click();
    Wait().Until(_ => Element(MainPage.MenuButton).Displayed);
  }

  [Test]
  [Order(19)]
  public void CanNavigateFromMainPageToListPage()
  {
    Element(MainPage.List.ListTitle + TestData.Lists[0].Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
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
    Element(ListPage.Item.DoneBox + currentList.Items[0].Name).Click();
    Element(ListPage.Item.DoneBox + currentList.Items[1].Name).Click();
    Element(ListPage.BackButton).Click();
    Wait(5).Until(_ => Element(MainPage.MenuButton).Displayed);
    Element(MainPage.List.ListTitle + currentList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    var item0 = OptionalElement(ListPage.Item.Label + currentList.Items[0].Name);
    var item1 = OptionalElement(ListPage.Item.Label + currentList.Items[1].Name);
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
  public void CanNavigateFromListPageToMainPage()
  {
    Element(ListPage.BackButton).Click();
    Wait(5).Until(_ => Element(MainPage.MenuButton).Displayed);
  }

  [Test]
  [Order(30)]
  public void CanDeleteList()
  {
    var currentList = TestData.Lists[0];
    Element(MainPage.List.DeleteButton + currentList.Name).Click();
    var no = ElementXPath(NoButton());
    no.Click();
    Assert.That(OptionalElement(MainPage.List.ListTitle + currentList.Name), Is.Not.Null);

    Element(MainPage.List.DeleteButton + currentList.Name).Click();
    var yes = ElementXPath(YesButton());
    yes.Click();
    Assert.That(OptionalElement(MainPage.List.ListTitle + currentList.Name), Is.Null);
  }

  [Test]
  [Order(99)]
  [Ignore("Temporarily disabled")]
  public void CanSignOutFromMainPage()
  {
    Wait().Until(_ => Element(MainPage.MenuButton).Displayed);
    Element(MainPage.MenuButton).Click();
    Element(MainPage.Menu.SignOutButton).Click();
    Wait(5).Until(_ => Element(StartPage.SignInButton));
    TakeScreenshot(nameof(CanSignOutFromMainPage));
  }
}
