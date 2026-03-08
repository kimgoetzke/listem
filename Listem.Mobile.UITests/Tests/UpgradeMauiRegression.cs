using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests.Tests;

/**
 * This test was created to understand and resolve a regression after upgrading the MAUI and Community Tool to
 * version 10.x.
 */
public class UpgradeMauiRegression : BaseTest
{
  private const string ListName = "Bug";

  [OneTimeSetUp]
  public void SetUp()
  {
    Console.WriteLine($"[XXX] {AppiumSetup.AppName} is installed: {IsInstalled}");
    if (!IsInstalled)
    {
      Assert.Fail($"{AppiumSetup.AppName} is not installed");
    }

    Act.OnStartPage.WaitForRedirect();

    // Delete list if it exists (happens if the test crashed last time it was run)
    var bugList = OptionalElement(MainPage.List.DeleteButton + ListName);
    if (bugList != null)
    {
      Console.WriteLine($"List [{ListName}] already exists, deleting it before testing...");
      bugList.Click();
      AwaitElementXPath(Alert.Yes)!.Click();
    }
  }

  [Test]
  public async Task PasteFromEmptyClipboardTest()
  {
    for (var i = 0; i < 2; i++)
    {
      Act.OnMainPage.CreateList(ListName);
      AwaitElement(MainPage.List.EditButton + ListName);

      // Navigate to ListPage
      Element(MainPage.List.ListTitle + ListName).Click();
      AwaitElement(ListPage.AddButton);

      for (var j = 0; j < 5; j++)
      {
        // Copy empty list to clipboard (effectively clearing clipboard content)
        Element(ListPage.CopyToClipboardButton).Click();
        await Task.Delay(100);

        // Paste empty list from clipboard
        Element(ListPage.InsertFromClipboardButton).Click();
        // IMPORTANT: The issue occurs right after the click ^
        //   - The screen dims, the app becomes unresponsive
        //   - Shortly after I see the "Nothing to import" toast
        //   - However, even after the toast disappears the app remains unresponsive and I have to force close it

        // Wait a bit to ensure that any clipboard operations have completed
        await Task.Delay(500);
      }

      // Delete list again
      Act.NavigateBackAndAwait(MainPage.MenuButton);
      await DeleteList(ListName);
    }
  }

  [Test]
  public async Task CreateAndDeleteLists()
  {
    for (var i = 0; i < 5; i++)
    {
      // Create list
      Act.OnMainPage.CreateList(ListName);
      AwaitElement(MainPage.List.EditButton + ListName);

      // Navigate to ListPage
      Element(MainPage.List.ListTitle + ListName).Click();
      AwaitElement(ListPage.AddButton);
      await Task.Delay(100);

      // Navigate back and delete the list again
      Act.NavigateBackAndAwait(MainPage.MenuButton);
      await DeleteList(ListName);
    }
  }

  [Test]
  public async Task SwipeCompleteItems()
  {
    // Create list
    Act.OnMainPage.CreateList(ListName);
    AwaitElement(MainPage.List.EditButton + ListName);

    // Navigate to ListPage
    Element(MainPage.List.ListTitle + ListName).Click();
    AwaitElement(ListPage.AddButton);
    await Task.Delay(100);

    // Create a bunch of items
    var itemList = new List<TestData.TestItem>();
    for (var i = 1; i < 10; i++)
    {
      var item = new TestData.TestItem("Item-" + i, "None", 1, false);
      Act.OnListPage.AddItemToList(item);
      AssertThat.OnListPage.ItemIsCreated(item);
      itemList.Add(item);
    }

    // Swipe to complete all items
    itemList.Reverse();
    foreach (var item in itemList)
    {
      Console.WriteLine($"Removing: {item}");
      Act.OnListPage.SwipeDeleteItem(item.Name);
    }

    // Navigate back and delete the list again
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    await DeleteList(ListName);
  }

  private static async Task DeleteList(string listName)
  {
    Element(MainPage.List.DeleteButton + listName).Click();
    await Task.Delay(50);
    AwaitElementXPath(Alert.Yes)!.Click();
    await Task.Delay(100);
    Assert.That(OptionalElement(MainPage.List.ListTitle + listName), Is.Null);
  }

  [OneTimeTearDown]
  public void CleanUp()
  {
    // No op
  }
}
