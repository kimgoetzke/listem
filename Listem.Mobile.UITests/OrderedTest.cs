using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests;

[TestFixture]
public class OrderedTest : BaseTest
{
  private readonly TestData.TestList _currentList = TestData.Lists[0];
  private const string EditedPrefix = "Edited";
  private const string NewPrefix = "New";

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
    ElementXPath(Alert.Yes).Click();
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
    ElementXPath(Alert.Yes).Click();
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    Wait().Until(_ => Element(EditListPage.AddCategoryButton).Displayed);
    AssertThat.OnEditListPage.Categories(false, _currentList.Categories);
    Assert.That(OptionalElement(EditListPage.ResetCategoriesButton), Is.Null);
  }

  [Test]
  [Order(24)]
  public void CanEditListCategories_RemoveCategories()
  {
    // TODO: Add swipe-removing categories test
    Act.OnEditListPage.AddListCategories(_currentList.Categories);
  }

  [Test]
  [Order(25)]
  public void CanEditListCollaborators_ShareList()
  {
    Act.OnEditListPage.ShareList(_currentList.Collaborators[0]);
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
    ElementXPath(Alert.Yes).Click();
    AssertThat.OnEditListPage.List(false, collaborator);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _currentList.Name).Click();
    AssertThat.OnEditListPage.List(false, collaborator);
  }

  [Test]
  [Order(27)]
  public void CanEditListCollaborators_ShareWithOwnerFails()
  {
    Element(EditListPage.ShareButton).Click();
    Element(StickyEntry.EntryField).SendKeys(_currentList.Owner.Email);
    Element(StickyEntry.SubmitButton).Click();
    var collaborator = _currentList.Collaborators[0];
    Element(EditListPage.ShareButton).Click();
    Element(StickyEntry.EntryField).SendKeys(collaborator.Email);
    Element(StickyEntry.SubmitButton).Click();
    AssertThat.OnEditListPage.List(false, _currentList.Owner, true);
    // TODO: Unshare button should be displayed after sharing list - fix that, then set ignoreButton to true below
    AssertThat.OnEditListPage.List(true, _currentList.Collaborators[0], true);
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
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + _currentList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    AssertThat.OnListPage.ItemIsDeleted(_currentList.Items[0]);
    AssertThat.OnListPage.ItemIsDeleted(_currentList.Items[1]);
    TakeScreenshot(nameof(CanRemoveItems_TickAndLeaveList));
    // Add items back to list for future tests
    var i1 = _currentList.Items[0];
    var i2 = _currentList.Items[1];
    Act.OnListPage.AddItemToList(i1.Name, i1.Category, i1.Quantity, i1.IsImportant);
    Act.OnListPage.AddItemToList(i2.Name, i2.Category, i2.Quantity, i2.IsImportant);
  }

  [Test]
  [Order(38)]
  public void CanRemoveItems_SwipeComplete()
  {
    // TODO: Add swipe-to-complete test for items 2-3
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

  [Test]
  [Order(50)]
  public void CanSignOutFromMainPage()
  {
    Wait().Until(_ => Element(MainPage.MenuButton).Displayed);
    Act.OnMainPage.SignOut();
    TakeScreenshot(nameof(CanSignOutFromMainPage));
  }

  [Test]
  [Order(51)]
  public void CanSignInWithDifferentUser()
  {
    Act.OnStartPage.SignIn(_currentList.Collaborators[0]);
    TakeScreenshot(nameof(CanSignInWithDifferentUser));
  }

  [Test]
  [Order(52)]
  public void CanWriteToRealmAfterChangingUser()
  {
    var otherList = TestData.Lists[1];
    Act.OnMainPage.CreateList(otherList.Name);
    AssertThat.OnMainPage.ListTagsAreCorrect(otherList.Name, false);
    Element(MainPage.List.ListTitle + otherList.Name).Click();
    Wait().Until(_ => Element(ListPage.AddButton).Displayed);
    Act.OnListPage.AddItemToList(otherList.Items[0]);
    AssertThat.OnListPage.ItemIsCreated(otherList.Items[0]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + otherList.Name).Click();
    Act.OnEditListPage.ShareList(otherList.Collaborators[0]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    AssertThat.OnMainPage.ListTagsAreCorrect(otherList.Name, true, true);
  }

  [Test]
  [Order(60)]
  public void CanCollaborate_SeeSharedList()
  {
    // BUG: Need to swipe to see the shared list first
    var element = Element(MainPage.List.ListTitle + _currentList.Name);
    Assert.That(element.Displayed, Is.True);
    AssertThat.OnMainPage.ListTagsAreCorrect(_currentList.Name, true);
    element.Click();
    _currentList.Items.ForEach(i =>
      AssertThat.OnListPage.ItemIsCreated(i.Name, i.Category, i.Quantity, i.IsImportant)
    );
  }

  [Test]
  [Order(61)]
  public void CanCollaborate_DeleteItemsFromSharedList()
  {
    Element(ListPage.Item.DoneBox + _currentList.Items[4].Name).Click();
    Element(ListPage.Item.DoneBox + _currentList.Items[5].Name).Click();
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + _currentList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    AssertThat.OnListPage.ItemIsDeleted(_currentList.Items[4]);
    AssertThat.OnListPage.ItemIsDeleted(_currentList.Items[5]);
  }

  [Test]
  [Order(62)]
  public void CanCollaborate_AddItemsToSharedList()
  {
    var i4 = _currentList.Items[4];
    var i5 = _currentList.Items[5];
    Act.OnListPage.AddItemToList(NewPrefix + i4.Name, i4.Category, i4.Quantity, i4.IsImportant);
    Act.OnListPage.AddItemToList(NewPrefix + i5.Name, i5.Category, i5.Quantity, i5.IsImportant);
    AssertThat.OnListPage.ItemIsCreated(
      NewPrefix + i4.Name,
      i4.Category,
      i4.Quantity,
      i4.IsImportant
    );
    AssertThat.OnListPage.ItemIsCreated(
      NewPrefix + i5.Name,
      i5.Category,
      i5.Quantity,
      i5.IsImportant
    );
    Act.NavigateBackAndAwait(MainPage.MenuButton);
  }

  [Test]
  [Order(63)]
  public void CanCollaborate_EditItemsOwnedBySomeoneElse()
  {
    var item = _currentList.Items[0];
    Element(ListPage.Item.Label + item.Name).Click();
    Act.OnDetailPage.ChangeItemName(item.Name, EditedPrefix);
    Act.OnDetailPage.ChangeItemQuantity(2);
    Act.OnDetailPage.SetItemIsImportant(true);
    Act.OnDetailPage.ChangeItemCategory(_currentList.Categories[1]);
    Act.NavigateBackAndAwait(ListPage.AddButton);
    AssertThat.OnListPage.ItemIsCreated(
      EditedPrefix + item.Name,
      _currentList.Categories[1],
      3,
      true
    );
    Element(ListPage.Item.Label + EditedPrefix + item.Name).Click();
    Act.OnDetailPage.ChangeItemName(item.Name);
    Act.OnDetailPage.ChangeItemQuantity(-2);
    Act.OnDetailPage.SetItemIsImportant(item.IsImportant);
    Act.OnDetailPage.ChangeItemCategory(item.Category);
    Act.NavigateBackAndAwait(ListPage.AddButton);
  }

  [Test]
  [Order(64)]
  public void CanCollaborate_ExitSharedList()
  {
    Element(MainPage.List.ExitButton + _currentList.Name).Click();
    ElementXPath(Alert.Yes).Click();
    var list = OptionalElement(MainPage.List.ListTitle + _currentList.Name);
    Assert.That(list, Is.Null);
  }

  [Test]
  [Order(65)]
  public void CanCollaborate_CollaboratorItemsRemainOnListAfterExit()
  {
    Act.OnMainPage.SignOut();
    Act.OnStartPage.SignIn(_currentList.Owner);
    var element = AwaitElement(MainPage.List.ListTitle + _currentList.Name);
    Assert.That(element!.Displayed, Is.True);
    AssertThat.OnMainPage.ListTagsAreCorrect(_currentList.Name, false);
    element.Click();
    var i4 = _currentList.Items[4];
    var i5 = _currentList.Items[5];
    AssertThat.OnListPage.ItemIsCreated(
      NewPrefix + i4.Name,
      i4.Category,
      i4.Quantity,
      i4.IsImportant
    );
    AssertThat.OnListPage.ItemIsCreated(
      NewPrefix + i5.Name,
      i5.Category,
      i5.Quantity,
      i5.IsImportant
    );
  }

  [Test]
  [Order(66)]
  public void CanCollaborate_OwnerCanEditCollaboratorsItems()
  {
    var i4 = _currentList.Items[4];
    var i5 = _currentList.Items[5];
    Element(ListPage.Item.Label + NewPrefix + i4.Name).Click();
    Act.OnDetailPage.ChangeItemName(i4.Name);
    Act.NavigateBackAndAwait(ListPage.AddButton);
    Element(ListPage.Item.Label + NewPrefix + i5.Name).Click();
    Act.OnDetailPage.ChangeItemName(i5.Name);
    Act.NavigateBackAndAwait(ListPage.AddButton);
    AssertThat.OnListPage.ItemIsCreated(i4.Name, i4.Category, i4.Quantity, i4.IsImportant);
    AssertThat.OnListPage.ItemIsCreated(i5.Name, i5.Category, i5.Quantity, i5.IsImportant);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
  }

  [Test]
  [Order(67)]
  public void CanDeleteListAsOwner_SharedList()
  {
    var deleteButton = Element(MainPage.List.DeleteButton + _currentList.Name);
    deleteButton.Click();
    ElementXPath(Alert.No).Click();
    Assert.That(OptionalElement(MainPage.List.ListTitle + _currentList.Name), Is.Not.Null);
    deleteButton.Click();
    ElementXPath(Alert.Yes).Click();
    Assert.That(OptionalElement(MainPage.List.ListTitle + _currentList.Name), Is.Null);
  }

  [Test]
  [Order(68)]
  public void CanCollaborate_DeletedListDisappearsForCollaborator()
  {
    var otherList = TestData.Lists[1];

    // Add item as collaborator
    Element(MainPage.List.ListTitle + otherList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    Act.OnListPage.AddItemToList(otherList.Items[1]);
    AssertThat.OnListPage.ItemIsCreated(otherList.Items[1]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);

    // Change user and delete list
    Act.OnMainPage.SignOut();
    Act.OnStartPage.SignIn(otherList.Owner);
    Assert.That(OptionalElement(MainPage.List.ListTitle + otherList.Name), Is.Not.Null);
    Element(MainPage.List.DeleteButton + otherList.Name).Click();
    ElementXPath(Alert.Yes).Click();
    Assert.That(OptionalElement(MainPage.List.ListTitle + otherList.Name), Is.Null);

    // Change user again and make sure list is gone
    Act.OnMainPage.SignOut();
    Act.OnStartPage.SignIn(otherList.Collaborators[0]);
    Assert.That(OptionalElement(MainPage.List.ListTitle + otherList.Name), Is.Null);
  }

  [OneTimeTearDown]
  public void CleanUp()
  {
    // Act.OnMainPage.SignOut();
    // Wait(5).Until(_ => Element(StartPage.SignInButton));
  }
}
