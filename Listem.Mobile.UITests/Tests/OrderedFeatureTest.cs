using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests.Tests;

[TestFixture]
public class OrderedFeatureTest : BaseTest
{
  private readonly TestData.TestList _testList = TestData.FeatureList;
  private const string EditedPrefix = "Edited";

  [OneTimeSetUp]
  public void SetUp()
  {
    Console.WriteLine($"[XXX] {AppiumSetup.AppName} is installed: {IsInstalled}");
    if (!IsInstalled)
    {
      Assert.Fail($"{AppiumSetup.AppName} is not installed");
    }
    Wait(15).Until(_ => Element(StartPage.SignInButton).Displayed);
    TakeScreenshot(nameof(OrderedFeatureTest), "CanStartApp");
  }

  [Test]
  [Order(1)]
  public void CanSignIn()
  {
    Act.OnStartPage.SignIn(_testList.Owner);
    TakeScreenshot(nameof(OrderedFeatureTest), nameof(CanSignIn));
  }

  [Test]
  [Order(10)]
  public void CanCreateList()
  {
    Wait().Until(_ => Element(MainPage.AddListButton).Displayed);
    Act.OnMainPage.CreateList(_testList.Name);
    Wait().Until(_ => Element(MainPage.List.ListTitle + _testList.Name).Displayed);
    TakeScreenshot(nameof(OrderedFeatureTest), nameof(CanCreateList));
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
    Element(MainPage.List.EditButton + _testList.Name).Click();
    Wait().Until(_ => Element(EditListPage.ListNameEntry).Displayed);
    TakeScreenshot(nameof(OrderedFeatureTest), nameof(CanNavigateToEditListPage));
  }

  [Test]
  [Order(20)]
  public void CanEditListName()
  {
    Act.OnEditListPage.ChangeListName(_testList.Name, EditedPrefix);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + EditedPrefix + _testList.Name).Click();
    Assert.That(
      Element(EditListPage.ListNameEntry).Text,
      Is.EqualTo(EditedPrefix + _testList.Name)
    );
    Act.OnEditListPage.ChangeListName(_testList.Name);
  }

  [Test]
  [Order(21)]
  public void CanEditListType()
  {
    Act.OnEditListPage.ChangeListType(_testList.ListType);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _testList.Name).Click();
    Assert.That(Element(EditListPage.ListTypePicker).Text, Is.EqualTo(_testList.ListType));
  }

  [Test]
  [Order(22)]
  public void CanEditListCategories_AddCategories()
  {
    Act.OnEditListPage.AddListCategories(_testList.Categories);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _testList.Name).Click();
    AssertThat.OnEditListPage.Categories(true, _testList.Categories);
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
    Element(MainPage.List.EditButton + _testList.Name).Click();
    Wait().Until(_ => Element(EditListPage.AddCategoryButton).Displayed);
    AssertThat.OnEditListPage.Categories(false, _testList.Categories);
    Assert.That(OptionalElement(EditListPage.ResetCategoriesButton), Is.Null);
  }

  [Test]
  [Order(24)]
  public async Task CanEditListCategories_RemoveCategories()
  {
    Act.OnEditListPage.AddListCategories(_testList.Categories);
    await Task.Delay(300);
    foreach (var category in _testList.Categories)
    {
      Act.OnEditListPage.SwipeDeleteCategory(category);
      await Task.Delay(300);
    }
    AssertThat.OnEditListPage.Categories(false, _testList.Categories);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _testList.Name).Click();
    AwaitElement(EditListPage.ListNameEntry);
    AssertThat.OnEditListPage.Categories(false, _testList.Categories);
    Act.OnEditListPage.AddListCategories(_testList.Categories);
  }

  [Test]
  [Order(25)]
  public void CanEditListCollaborators_ShareList()
  {
    Act.OnEditListPage.ShareList(_testList.Collaborators[0]);
    // TODO: Unshare button should be displayed after sharing list - fix that, then set ignoreButton to true below
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Assert.Multiple(() =>
    {
      Assert.That(Element(MainPage.List.Tags.Shared + _testList.Name).Displayed, Is.True);
      Assert.That(Element(MainPage.List.Tags.Owner + _testList.Name).Displayed, Is.True);
    });
    Element(MainPage.List.EditButton + _testList.Name).Click();
    AssertThat.OnEditListPage.List(true, _testList.Collaborators[0]);
  }

  [Test]
  [Order(26)]
  public void CanEditListCollaborators_MakeListPrivate()
  {
    var collaborator = _testList.Collaborators[0];
    var unshareButton = Element(EditListPage.UnshareButton);
    Assert.That(unshareButton.Displayed, Is.True);
    unshareButton.Click();
    AwaitElementXPath(Alert.Yes)!.Click();
    AssertThat.OnEditListPage.List(false, collaborator);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _testList.Name).Click();
    AssertThat.OnEditListPage.List(false, collaborator);
  }

  [Test]
  [Order(27)]
  public void CanEditListCollaborators_ShareWithOwnerFails()
  {
    Act.OnEditListPage.ShareList(_testList.Owner);
    Task.Delay(500);
    AssertThat.OnEditListPage.List(false, _testList.Owner, true);
  }

  [Test]
  [Order(29)]
  public void CanNavigateToListPage()
  {
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    TakeScreenshot(nameof(OrderedFeatureTest), nameof(CanNavigateToListPage));
  }

  [Test]
  [Order(30)]
  public void CanAddItemsToList()
  {
    _testList.Items.ForEach(item =>
    {
      Act.OnListPage.AddItemToList(item);
      AssertThat.OnListPage.ItemIsCreated(item);
    });
    TakeScreenshot(nameof(OrderedFeatureTest), nameof(CanAddItemsToList));
  }

  [Test]
  [Order(37)]
  public void CanRemoveItems_TickAndLeaveList()
  {
    Element(ListPage.Item.DoneBox + _testList.Items[0].Name).Click();
    Element(ListPage.Item.DoneBox + _testList.Items[1].Name).Click();
    Act.NavigateBackAndAwait(MainPage.MenuButton, 5);
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    AssertThat.OnListPage.ItemIsDeleted(_testList.Items[0]);
    AssertThat.OnListPage.ItemIsDeleted(_testList.Items[1]);
    TakeScreenshot(nameof(OrderedFeatureTest), nameof(CanRemoveItems_TickAndLeaveList));
    // Add items back to list for future tests
    Act.OnListPage.AddItemToList(_testList.Items[0]);
    Act.OnListPage.AddItemToList(_testList.Items[1]);
  }

  [Test]
  [Order(38)]
  public async Task CanRemoveItems_SwipeComplete()
  {
    Act.HideKeyboard();
    AssertThat.OnListPage.ItemIsCreated(_testList.Items[2]);
    AssertThat.OnListPage.ItemIsCreated(_testList.Items[3]);
    Act.OnListPage.SwipeDeleteItem(_testList.Items[2].Name);
    await Task.Delay(1000);
    Act.OnListPage.SwipeDeleteItem(_testList.Items[3].Name);
    await Task.Delay(1000);
    AssertThat.OnListPage.ItemIsDeleted(_testList.Items[2]);
    AssertThat.OnListPage.ItemIsDeleted(_testList.Items[3]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    AwaitElement(ListPage.AddButton);
    // Add items back to list for future tests
    Act.OnListPage.AddItemToList(_testList.Items[2]);
    Act.OnListPage.AddItemToList(_testList.Items[3]);
  }

  [Test]
  [Order(39)]
  public void CanNavigateToDetailPage()
  {
    Element(ListPage.Item.Label + _testList.Items[5].Name).Click();
    Wait().Until(_ => Element(DetailPage.NameEntry).Displayed);
    TakeScreenshot(nameof(OrderedFeatureTest), nameof(CanNavigateToDetailPage));
  }

  [Test]
  [Order(40)]
  public void CanEditItemName()
  {
    var currentItemName = _testList.Items[5].Name;
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
    var currentItem = _testList.Items[5];
    Act.OnDetailPage.ChangeItemCategory(_testList.Categories[0]);
    Assert.That(Element(DetailPage.CategoryPicker).Text, Is.EqualTo(_testList.Categories[0]));
    Act.NavigateBackAndAwait(ListPage.AddButton);
    AssertThat.OnListPage.CategoryTagIsCorrect(currentItem.Name, _testList.Categories[0]);
    Element(ListPage.Item.Label + currentItem.Name).Click();
    Assert.That(Element(DetailPage.CategoryPicker).Text, Is.EqualTo(_testList.Categories[0]));
    Act.OnDetailPage.ChangeItemCategory(currentItem.Category);
  }

  [Test]
  [Order(42)]
  public void CanEditItemImportance()
  {
    var currentItem = _testList.Items[5];
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
    var currentItem = _testList.Items[5];
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
    TakeScreenshot(nameof(OrderedFeatureTest), nameof(CanNavigateFromDetailPageToMainPage));
  }

  [OneTimeTearDown]
  public void CleanUp()
  {
    Act.OnMainPage.SignOut();
    Wait(5).Until(_ => Element(StartPage.SignInButton));
  }
}
