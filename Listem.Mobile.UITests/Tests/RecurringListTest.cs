using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests.Tests;

[TestFixture]
public class RecurringListTest : BaseTest
{
  private static List<TestData.TestItem> StandardListItems { get; } =
    [
      new("Item-0", DefaultCategoryName, 1, false),
      new("Item-1", DefaultCategoryName, 1, true),
      new("Item-2", DefaultCategoryName, 1, false)
    ];

  private static readonly TestData.TestList TestList =
    new("Recurring", StandardListItems, DefaultListType, []);

  [OneTimeSetUp]
  public void SetUp()
  {
    // App is installed
    Console.WriteLine($"[XXX] {AppiumSetup.AppName} is installed: {IsInstalled}");
    if (!IsInstalled)
    {
      Assert.Fail($"{AppiumSetup.AppName} is not installed");
    }

    // Delete any existing data
    Act.OnMainPage.OpenMenu();
    Element(MainPage.Menu.DeleteDataButton).Click();
    AwaitElementXPath(Alert.Yes)!.Click();
    AssertThat.ElementDoesNotExist(MainPage.List.ListTitle + TestList.Name);

    // Create list
    Wait().Until(_ => Element(MainPage.AddListButton).Displayed);
    Act.OnMainPage.CreateList(TestList.Name);
    Wait().Until(_ => Element(MainPage.List.ListTitle + TestList.Name).Displayed);

    // Add items to list
    Element(MainPage.List.ListTitle + TestList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    TestList.Items.ForEach(item =>
    {
      Act.OnListPage.AddItemToList(item);
      AssertThat.OnListPage.ItemIsCreated(item);
    });

    // Navigate back to main page
    Act.NavigateBackAndAwait(MainPage.MenuButton);
  }

  [Test]
  [Order(1)]
  public void CanEnableRecurringList()
  {
    Element(MainPage.List.EditButton + TestList.Name).Click();
    Wait().Until(_ => Element(EditListPage.ListNameEntry).Displayed);
    Act.OnEditListPage.ToggleRecurringList(true);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + TestList.Name).Click();
    Wait().Until(_ => Element(EditListPage.ListNameEntry).Displayed);
    AssertThat.OnEditListPage.RecurringListIsEnabled(true);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + TestList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
  }

  [Test]
  [Order(2)]
  public void CanRetainItems_TickAndLeaveRecurringList()
  {
    Element(ListPage.Item.DoneBox + TestList.Items[0].Name).Click();
    Element(ListPage.Item.DoneBox + TestList.Items[1].Name).Click();
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + TestList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    AssertThat.OnListPage.ItemIsCreated(TestList.Items[0], false);
    AssertThat.OnListPage.ItemIsCreated(TestList.Items[1], false);
  }

  [Test]
  [Order(3)]
  public void CanEditItemActivityOnDetailPage()
  {
    Element(ListPage.Item.Label + TestList.Items[2].Name).Click();
    Wait().Until(_ => Element(DetailPage.NameEntry).Displayed);
    Assert.That(OptionalElement(DetailPage.IsImportantSwitch), Is.Null);
    Assert.That(Element(DetailPage.IsActiveSwitch), Is.Not.Null);
    AssertThat.OnDetailPage.ItemIsActive(true);
    Act.OnDetailPage.SetItemIsActive(false);
    AssertThat.OnDetailPage.ItemIsActive(false);
    Act.NavigateBackAndAwait(ListPage.AddButton);
    Element(ListPage.Item.Label + TestList.Items[2].Name).Click();
    Wait().Until(_ => Element(DetailPage.NameEntry).Displayed);
    AssertThat.OnDetailPage.ItemIsActive(false);
    Act.OnDetailPage.SetItemIsActive(true);
    Act.NavigateBackAndAwait(ListPage.AddButton);
  }

  [Test]
  [Order(4)]
  public async Task CanStillRemoveItems_SwipeComplete()
  {
    Act.OnListPage.SwipeDeleteItem(TestList.Items[2].Name);
    await Task.Delay(1000);
    AssertThat.OnListPage.ItemIsDeleted(TestList.Items[2]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + TestList.Name).Click();
    AwaitElement(ListPage.AddButton);
    AssertThat.OnListPage.ItemIsDeleted(TestList.Items[2]);
  }

  [Test]
  [Order(99)]
  public void CanNavigateFromListPageToMainPage()
  {
    Act.NavigateBackAndAwait(MainPage.MenuButton);
  }

  [OneTimeTearDown]
  public void CleanUp()
  {
    Act.OnMainPage.OpenMenu();
    Element(MainPage.Menu.DeleteDataButton).Click();
    AwaitElementXPath(Alert.Yes)!.Click();
    AssertThat.ElementDoesNotExist(MainPage.List.ListTitle + TestList.Name);
  }
}
