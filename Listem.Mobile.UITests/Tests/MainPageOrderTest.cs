using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests.Tests;

[TestFixture]
public class MainPageOrderTest : BaseTest
{
  private readonly TestData.TestList _testList1 = TestData.FeatureList;
  private readonly TestData.TestList _testList2 = TestData.StandardList;
  private const string EditedPrefix = "Edited";

  [SetUp]
  public void SetUp()
  {
    Console.WriteLine($"[XXX] {AppiumSetup.AppName} is installed: {IsInstalled}");
    if (!IsInstalled)
    {
      Assert.Fail($"{AppiumSetup.AppName} is not installed");
    }
    Act.OnStartPage.WaitForRedirect();
    Wait().Until(_ => Element(MainPage.AddListButton).Displayed);
    Act.OnMainPage.CreateList(_testList1.Name);
    Wait().Until(_ => Element(MainPage.List.ListTitle + _testList1.Name).Displayed);
    Act.OnMainPage.CreateList(_testList2.Name);
    Wait().Until(_ => Element(MainPage.List.ListTitle + _testList2.Name).Displayed);
    TakeScreenshot(nameof(MainPageOrderTest), nameof(SetUp));
  }

  [Test]
  public void ListOrderIsCorrect_AfterCreation()
  {
    var list1 = AssertThat.OnMainPage.ListIsDisplayed(_testList1.Name);
    var list2 = AssertThat.OnMainPage.ListIsDisplayed(_testList2.Name);
    AssertThat.OnMainPage.ListOrderIsCorrect(list2, list1); // Because list2 was created last
  }

  [Test]
  public void ListOrderIsCorrect_AfterEditingListName()
  {
    var list1 = AssertThat.OnMainPage.ListIsDisplayed(_testList1.Name);
    var list2 = AssertThat.OnMainPage.ListIsDisplayed(_testList2.Name);
    AssertThat.OnMainPage.ListOrderIsCorrect(list2, list1);

    Act.OnMainPage.SwipeRight(MainPage.List.ListTitle + _testList2.Name);
    NavigateToEditListPage(_testList1.Name);
    Act.OnEditListPage.ChangeListName(_testList1.Name, EditedPrefix);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    list1 = Element(MainPage.List.ListTitle + EditedPrefix + _testList1.Name);
    list2 = Element(MainPage.List.ListTitle + _testList2.Name);
    AssertThat.OnMainPage.ListOrderIsCorrect(list1, list2); // Because list1 was last modified
  }

  [Test]
  public void ListOrderIsCorrect_AfterEditingListType()
  {
    var list1 = AssertThat.OnMainPage.ListIsDisplayed(_testList1.Name);
    var list2 = AssertThat.OnMainPage.ListIsDisplayed(_testList2.Name);
    AssertThat.OnMainPage.ListOrderIsCorrect(list2, list1);

    Act.OnMainPage.SwipeRight(MainPage.List.ListTitle + _testList2.Name);
    NavigateToEditListPage(_testList1.Name);
    Act.OnEditListPage.ChangeListType(_testList1.ListType);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    list1 = Element(MainPage.List.ListTitle + _testList1.Name);
    list2 = Element(MainPage.List.ListTitle + _testList2.Name);
    AssertThat.OnMainPage.ListOrderIsCorrect(list1, list2); // Because list1 was last modified
  }

  [Test]
  public void ListOrderIsCorrect_AfterAddingListItem()
  {
    var list1 = AssertThat.OnMainPage.ListIsDisplayed(_testList1.Name);
    var list2 = AssertThat.OnMainPage.ListIsDisplayed(_testList2.Name);
    AssertThat.OnMainPage.ListOrderIsCorrect(list2, list1);

    Act.OnMainPage.SwipeRight(MainPage.List.ListTitle + _testList2.Name);
    NavigateToListPage(_testList1.Name);
    Act.OnListPage.AddItemToList(_testList1.Items[0]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    list1 = Element(MainPage.List.ListTitle + _testList1.Name);
    list2 = Element(MainPage.List.ListTitle + _testList2.Name);
    AssertThat.OnMainPage.ListOrderIsCorrect(list1, list2); // Because list1 was last modified
  }

  private static void NavigateToEditListPage(string listName)
  {
    Element(MainPage.List.EditButton + listName).Click();
    Wait().Until(_ => Element(EditListPage.ListNameEntry).Displayed);
  }

  private static void NavigateToListPage(string listName)
  {
    Element(MainPage.List.ListTitle + listName).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
  }

  [TearDown]
  public void CleanUp()
  {
    Act.OnMainPage.OpenMenu();
    Element(MainPage.Menu.DeleteDataButton).Click();
    AwaitElementXPath(Alert.Yes)!.Click();
    AssertThat.ElementDoesNotExist(MainPage.List.ListTitle + _testList1.Name);
  }
}
