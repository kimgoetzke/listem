using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests.Tests;

[TestFixture]
public class OrderedBasicFeatureTest : BaseTest
{
  private readonly TestData.TestList _currentList = TestData.Lists[0];
  private const string EditedPrefix = "Edited";

  [Test]
  [Order(1)]
  public void CanStartApp()
  {
    Console.WriteLine($"[XXX] {AppiumSetup.AppName} is installed: {IsInstalled}");
    Wait(15).Until(_ => Element(StartPage.SignInButton).Displayed);
    TakeScreenshot(nameof(CanStartApp));
  }

  [Test]
  [Order(2)]
  public void CanSignIn()
  {
    Act.OnStartPage.SignIn(_currentList.Owner);
    TakeScreenshot(nameof(CanSignIn));
  }

  [Test]
  [Order(10)]
  public void CanCreateList()
  {
    Wait().Until(_ => Element(MainPage.AddListButton).Displayed);
    Act.OnMainPage.CreateList(_currentList.Name);
    Wait().Until(_ => Element(MainPage.List.ListTitle + _currentList.Name).Displayed);
    TakeScreenshot(nameof(CanCreateList));
  }

  [Test]
  [Order(11)]
  public void CanDeleteListAsOwner_PrivateList()
  {
    const string listName = "Delete-Me-List";
    Act.OnMainPage.CreateList(listName);
    Wait().Until(_ => Element(MainPage.List.ListTitle + listName).Displayed);
    Element(MainPage.List.DeleteButton + listName).Click();
    AwaitElementXPath(Alert.Yes)!.Click();
    Assert.That(OptionalElement(MainPage.List.ListTitle + listName), Is.Null);
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
    Act.OnEditListPage.ChangeListName(_currentList.Name, EditedPrefix);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + EditedPrefix + _currentList.Name).Click();
    Assert.That(
      Element(EditListPage.ListNameEntry).Text,
      Is.EqualTo(EditedPrefix + _currentList.Name)
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
  public void CanEditListCategories_AddCategories()
  {
    Act.OnEditListPage.AddListCategories(_currentList.Categories);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    AssertThat.OnEditListPage.Categories(true, _currentList.Categories);
  }

  [Test]
  [Order(23)]
  public void CanEditListCategories_ResetCategories()
  {
    var resetButton = Element(EditListPage.ResetCategoriesButton);
    Assert.That(resetButton.Displayed, Is.True);
    resetButton.Click();
    AwaitElementXPath(Alert.Yes)!.Click();
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    Wait().Until(_ => Element(EditListPage.AddCategoryButton).Displayed);
    AssertThat.OnEditListPage.Categories(false, _currentList.Categories);
    Assert.That(OptionalElement(EditListPage.ResetCategoriesButton), Is.Null);
  }

  [Test]
  [Order(24)]
  public async Task CanEditListCategories_RemoveCategories()
  {
    Act.OnEditListPage.AddListCategories(_currentList.Categories);
    await Task.Delay(300);
    foreach (var category in _currentList.Categories)
    {
      Act.OnEditListPage.SwipeDeleteCategory(category);
      await Task.Delay(300);
    }
    AssertThat.OnEditListPage.Categories(false, _currentList.Categories);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    AwaitElement(EditListPage.ListNameEntry);
    AssertThat.OnEditListPage.Categories(false, _currentList.Categories);
    Act.OnEditListPage.AddListCategories(_currentList.Categories);
  }

  [Test]
  [Order(25)]
  public void CanEditListCollaborators_ShareList()
  {
    Act.OnEditListPage.ShareList(_currentList.Collaborators[0]);
    // TODO: Unshare button should be displayed after sharing list - fix that, then set ignoreButton to true below
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Assert.Multiple(() =>
    {
      Assert.That(Element(MainPage.List.Tags.Shared + _currentList.Name).Displayed, Is.True);
      Assert.That(Element(MainPage.List.Tags.Owner + _currentList.Name).Displayed, Is.True);
    });
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    AssertThat.OnEditListPage.List(true, _currentList.Collaborators[0]);
  }

  [Test]
  [Order(26)]
  public void CanEditListCollaborators_MakeListPrivate()
  {
    var collaborator = _currentList.Collaborators[0];
    var unshareButton = Element(EditListPage.UnshareButton);
    Assert.That(unshareButton.Displayed, Is.True);
    unshareButton.Click();
    AwaitElementXPath(Alert.Yes)!.Click();
    AssertThat.OnEditListPage.List(false, collaborator);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    AssertThat.OnEditListPage.List(false, collaborator);
  }

  [Test]
  [Order(27)]
  public void CanEditListCollaborators_ShareWithOwnerFails()
  {
    Act.OnEditListPage.ShareList(_currentList.Owner);
    Task.Delay(500);
    AssertThat.OnEditListPage.List(false, _currentList.Owner, true);
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
    Act.NavigateBackAndAwait(MainPage.MenuButton, 5);
    Element(MainPage.List.ListTitle + _currentList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    AssertThat.OnListPage.ItemIsDeleted(_currentList.Items[0]);
    AssertThat.OnListPage.ItemIsDeleted(_currentList.Items[1]);
    TakeScreenshot(nameof(CanRemoveItems_TickAndLeaveList));
    // Add items back to list for future tests
    Act.OnListPage.AddItemToList(_currentList.Items[0]);
    Act.OnListPage.AddItemToList(_currentList.Items[1]);
  }

  [Test]
  [Order(38)]
  public void CanRemoveItems_SwipeComplete()
  {
    Act.HideKeyboard();
    AssertThat.OnListPage.ItemIsCreated(_currentList.Items[2]);
    AssertThat.OnListPage.ItemIsCreated(_currentList.Items[3]);
    Act.OnListPage.SwipeDeleteItem(_currentList.Items[2].Name);
    Act.OnListPage.SwipeDeleteItem(_currentList.Items[3].Name);
    AssertThat.OnListPage.ItemIsDeleted(_currentList.Items[2]);
    AssertThat.OnListPage.ItemIsDeleted(_currentList.Items[3]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + _currentList.Name).Click();
    AwaitElement(ListPage.AddButton);
    // Add items back to list for future tests
    Act.OnListPage.AddItemToList(_currentList.Items[2]);
    Act.OnListPage.AddItemToList(_currentList.Items[3]);
  }

  [Test]
  [Order(39)]
  public void CanNavigateToDetailPage()
  {
    Element(ListPage.Item.Label + _currentList.Items[5].Name).Click();
    Wait().Until(_ => Element(DetailPage.NameEntry).Displayed);
    TakeScreenshot(nameof(CanNavigateToDetailPage));
  }

  [Test]
  [Order(40)]
  public void CanEditItemName()
  {
    var currentItemName = _currentList.Items[5].Name;
    Act.OnDetailPage.ChangeItemName(currentItemName, EditedPrefix);
    Act.NavigateBackAndAwait(ListPage.AddButton);
    Element(ListPage.Item.Label + EditedPrefix + currentItemName).Click();
    var itemNameEntry = Element(DetailPage.NameEntry);
    Assert.That(itemNameEntry.Text, Is.EqualTo(EditedPrefix + currentItemName));
    Act.OnDetailPage.ChangeItemName(currentItemName);
  }

  [Test]
  [Order(41)]
  public void CanEditItemCategory()
  {
    var currentItem = _currentList.Items[5];
    Act.OnDetailPage.ChangeItemCategory(_currentList.Categories[0]);
    Assert.That(Element(DetailPage.CategoryPicker).Text, Is.EqualTo(_currentList.Categories[0]));
    Act.NavigateBackAndAwait(ListPage.AddButton);
    AssertThat.OnListPage.CategoryTagIsCorrect(currentItem.Name, _currentList.Categories[0]);
    Element(ListPage.Item.Label + currentItem.Name).Click();
    Assert.That(Element(DetailPage.CategoryPicker).Text, Is.EqualTo(_currentList.Categories[0]));
    Act.OnDetailPage.ChangeItemCategory(currentItem.Category);
  }

  [Test]
  [Order(42)]
  public void CanEditItemImportance()
  {
    var currentItem = _currentList.Items[5];
    Act.OnDetailPage.SetItemIsImportant(!currentItem.IsImportant);
    Act.NavigateBackAndAwait(ListPage.AddButton);
    Assert.That(Element(ListPage.Item.IsImportantIcon + currentItem.Name).Displayed, Is.True);
    Element(ListPage.Item.Label + currentItem.Name).Click();
    AssertThat.OnDetailPage.ItemIsImportant(!currentItem.IsImportant);
    Act.OnDetailPage.SetItemIsImportant(currentItem.IsImportant);
    AssertThat.OnDetailPage.ItemIsImportant(currentItem.IsImportant);
  }

  [Test]
  [Order(43)]
  public void CanEditItemQuantity()
  {
    var currentItem = _currentList.Items[5];
    Act.OnDetailPage.ChangeItemQuantity(2);
    Act.NavigateBackAndAwait(ListPage.AddButton);
    AssertThat.OnListPage.QuantityIsCorrect(currentItem.Name, 3);
    Element(ListPage.Item.Label + currentItem.Name).Click();
    AssertThat.OnDetailPage.QuantityIsCorrect(3);
    Act.OnDetailPage.ChangeItemQuantity(-5);
    Act.NavigateBackAndAwait(ListPage.AddButton);
    AssertThat.OnListPage.QuantityIsCorrect(currentItem.Name, 1);
    Element(ListPage.Item.Label + currentItem.Name).Click();
    AssertThat.OnDetailPage.QuantityIsCorrect(currentItem.Quantity);
  }

  [Test]
  [Order(49)]
  public void CanNavigateFromDetailPageToMainPage()
  {
    Act.NavigateBackAndAwait(ListPage.BackButton);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    TakeScreenshot(nameof(CanNavigateFromDetailPageToMainPage));
  }

  [OneTimeTearDown]
  public void CleanUp()
  {
    Act.OnMainPage.SignOut();
    Wait(5).Until(_ => Element(StartPage.SignInButton));
  }
}
