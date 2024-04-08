using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests;

[TestFixture]
public class OrderedTest : BaseTest
{
  private readonly TestData.TestList _currentList = TestData.Lists[0];

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
    Wait().Until(_ => Element(MainPage.AddListButton).Displayed);
    Element(MainPage.AddListButton).Click();
    Element(StickyEntry.EntryField).SendKeys(_currentList.Name);
    Element(StickyEntry.SubmitButton).Click();
    Wait().Until(_ => Element(MainPage.List.ListTitle + _currentList.Name).Displayed);
    TakeScreenshot(nameof(CanCreateList));
  }

  [Test]
  [Order(19)]
  public void CanNavigateToEditListPage()
  {
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    Wait().Until(_ => Element(EditListPage.ListNameEntry).Displayed);
    TakeScreenshot(nameof(CanNavigateToEditListPage));
  }

  [Test]
  [Order(20)]
  public void CanEditListName()
  {
    const string listNamePrefix = "Edited";
    Act.OnEditListPage.ChangeListName(_currentList.Name, listNamePrefix);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + listNamePrefix + _currentList.Name).Click();
    Assert.That(
      Element(EditListPage.ListNameEntry).Text,
      Is.EqualTo(listNamePrefix + _currentList.Name)
    );
    Act.OnEditListPage.ChangeListName(_currentList.Name);
  }

  [Test]
  [Order(21)]
  public void CanEditListType()
  {
    Act.OnEditListPage.ChangeListType(_currentList.ListType);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    Assert.That(Element(EditListPage.ListTypePicker).Text, Is.EqualTo(TestData.Lists[0].ListType));
  }

  [Test]
  [Order(22)]
  public void CanAddCategories()
  {
    Act.OnEditListPage.AddListCategories(_currentList.Categories);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    AssertThat.OnEditListPage.Categories(true, _currentList.Categories);
  }

  [Test]
  [Order(23)]
  public void CanResetCategories()
  {
    var resetButton = Element(EditListPage.ResetCategoriesButton);
    Assert.That(resetButton.Displayed, Is.True);
    resetButton.Click();
    ElementXPath(YesButton()).Click();
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    AssertThat.OnEditListPage.Categories(false, _currentList.Categories);
    Assert.That(OptionalElement(EditListPage.ResetCategoriesButton), Is.Null);
  }

  [Test]
  [Order(24)]
  public void CanRemoveCategories()
  {
    // TODO: Add swipe-removing categories test
    Act.OnEditListPage.AddListCategories(_currentList.Categories);
  }

  [Test]
  [Order(25)]
  public void CanShareList()
  {
    var otherUser = TestData.Users[1];
    Element(EditListPage.ShareButton).Click();
    Element(StickyEntry.EntryField).SendKeys(otherUser.Email);
    Element(StickyEntry.SubmitButton).Click();
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Assert.Multiple(() =>
    {
      Assert.That(Element(MainPage.List.Tags.Shared + _currentList.Name).Displayed, Is.True);
      Assert.That(Element(MainPage.List.Tags.Owner + _currentList.Name).Displayed, Is.True);
    });
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    AssertThat.OnEditListPage.List(true, otherUser);
  }

  [Test]
  [Order(26)]
  public void CanMakeListPrivate()
  {
    var otherUser = TestData.Users[1];
    var unshareButton = Element(EditListPage.UnshareButton);
    Assert.That(unshareButton.Displayed, Is.True);
    unshareButton.Click();
    ElementXPath(YesButton()).Click();
    AssertThat.OnEditListPage.List(false, otherUser);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    AssertThat.OnEditListPage.List(false, otherUser);
  }

  [Test]
  [Order(29)]
  public void CanNavigateToListPage()
  {
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + TestData.Lists[0].Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    TakeScreenshot(nameof(CanNavigateToListPage));
  }

  [Test]
  [Order(30)]
  public void CanAddItemsToList()
  {
    _currentList.Items.ForEach(i =>
    {
      Act.OnListPage.AddItemToList(i.Name, i.Category, i.Quantity, i.IsImportant);
      AssertThat.OnListPage.ItemIsCreated(i.Name, i.Category, i.Quantity, i.IsImportant);
    });
    TakeScreenshot(nameof(CanAddItemsToList));
  }

  [Test]
  [Order(37)]
  public void CanRemoveItems_TickAndLeaveList()
  {
    Element(ListPage.Item.DoneBox + _currentList.Items[0].Name).Click();
    Element(ListPage.Item.DoneBox + _currentList.Items[1].Name).Click();
    Element(ListPage.BackButton).Click();
    Wait(5).Until(_ => Element(MainPage.MenuButton).Displayed);
    Element(MainPage.List.ListTitle + _currentList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    var item0 = OptionalElement(ListPage.Item.Label + _currentList.Items[0].Name);
    var item1 = OptionalElement(ListPage.Item.Label + _currentList.Items[1].Name);
    Assert.Multiple(() =>
    {
      Assert.That(item0, Is.Null);
      Assert.That(item1, Is.Null);
    });
  }

  [Test]
  [Order(38)]
  public void CanRemoveItems_SwipeComplete()
  {
    // TODO: Add swipe-to-complete test
    // var item2 = OptionalElement("Label_" + TestData.Items[2].Name);
    // var item3 = OptionalElement("Label_" + TestData.Items[3].Name);
    // Assert.Multiple(() =>
    // {
    //   Assert.That(item2, Is.Null);
    //   Assert.That(item3, Is.Null);
    // });
  }

  [Test]
  [Order(39)]
  public void CanNavigateToDetailPage()
  {
    // TODO: Add DetailPage tests
  }

  // TODO: Add switching user tests with adding and removing shared items
  // TODO: Add theme tests (optional)

  [Test]
  [Order(89)]
  public void CanNavigateBackToMainPage()
  {
    Act.NavigateBackAndAwait(ListPage.BackButton);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
  }

  [Test]
  [Order(90)]
  [Ignore("Temporarily disabled")]
  public void CanDeleteList()
  {
    Element(MainPage.List.DeleteButton + _currentList.Name).Click();
    var no = ElementXPath(NoButton());
    no.Click();
    Assert.That(OptionalElement(MainPage.List.ListTitle + _currentList.Name), Is.Not.Null);

    Element(MainPage.List.DeleteButton + _currentList.Name).Click();
    var yes = ElementXPath(YesButton());
    yes.Click();
    Assert.That(OptionalElement(MainPage.List.ListTitle + _currentList.Name), Is.Null);
  }

  [Test]
  [Order(91)]
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
