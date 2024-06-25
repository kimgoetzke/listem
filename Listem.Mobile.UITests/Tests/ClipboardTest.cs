using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests.Tests;

public class ClipboardTest : BaseTest
{
  private readonly TestData.TestList _testList = TestData.ClipboardList;

  [OneTimeSetUp]
  public void SetUp()
  {
    Console.WriteLine($"[XXX] {AppiumSetup.AppName} is installed: {IsInstalled}");
    if (!IsInstalled)
    {
      Assert.Fail($"{AppiumSetup.AppName} is not installed");
    }
    Wait(15).Until(_ => Element(StartPage.SignInButton).Displayed);
    Act.OnStartPage.SignIn(_testList.Owner);
  }

  [Test]
  public async Task CopyToAndPasteFromClipboardTest()
  {
    // Create list and configure type & categories
    Act.OnMainPage.CreateList(_testList.Name);
    AwaitElement(MainPage.List.EditButton + _testList.Name);
    TakeScreenshot(nameof(CopyToAndPasteFromClipboardTest), "1-ListCreated");
    Element(MainPage.List.EditButton + _testList.Name).Click();
    AwaitElement(EditListPage.ListNameEntry);
    Act.OnEditListPage.AddListCategories(_testList.Categories);
    Act.OnEditListPage.ChangeListType(_testList.ListType);

    // Navigate to ListPage
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    AwaitElement(ListPage.AddButton);

    // Add items
    _testList.Items.ForEach(item =>
    {
      Act.OnListPage.AddItemToList(item);
      AssertThat.OnListPage.ItemIsCreated(item);
    });
    Act.HideKeyboard();
    TakeScreenshot(nameof(CopyToAndPasteFromClipboardTest), "2-ItemsAdded");

    // Copy to clipboard
    Element(ListPage.CopyToClipboardButton).Click();

    // Clear list again
    foreach (var item in _testList.Items)
    {
      Act.OnListPage.SwipeDeleteItem(item.Name);
      await Task.Delay(1000);
    }

    Act.NavigateBackAndAwait(MainPage.MenuButton, 10);
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    AwaitElement(ListPage.AddButton);
    TakeScreenshot(nameof(CopyToAndPasteFromClipboardTest), "3-ItemsDeletedAgain");
    _testList.Items.ForEach(AssertThat.OnListPage.ItemIsDeleted);

    // Paste from clipboard
    Element(ListPage.InsertFromClipboardButton).Click();
    TakeScreenshot(nameof(CopyToAndPasteFromClipboardTest), "4-DialogShown");
    AwaitElementXPath(Alert.Yes)!.Click();

    // Assert that items have been pasted correctly
    await Task.Delay(2000);
    TakeScreenshot(nameof(CopyToAndPasteFromClipboardTest), "5-ItemsPasted");
    _testList.Items.ForEach(AssertThat.OnListPage.ItemIsCreated);

    // Delete list again
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.DeleteButton + _testList.Name).Click();
    AwaitElementXPath(Alert.Yes)!.Click();
    TakeScreenshot(nameof(CopyToAndPasteFromClipboardTest), "6-ListDeleted");
    Assert.That(OptionalElement(MainPage.List.ListTitle + _testList.Name), Is.Null);
  }

  [OneTimeTearDown]
  public void CleanUp()
  {
    Act.OnMainPage.SignOut();
    Wait(5).Until(_ => Element(StartPage.SignInButton));
  }
}
