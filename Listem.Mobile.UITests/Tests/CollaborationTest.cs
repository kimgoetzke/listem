﻿using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests.Tests;

public class CollaborationTest : BaseTest
{
  private readonly TestData.TestList _testList = TestData.CollaborationList;
  private const string NewPrefix = "New";

  [Test]
  public async Task BasicCollaborationTest()
  {
    // Can start app
    Console.WriteLine($"[XXX] {AppiumSetup.AppName} is installed: {IsInstalled}");
    Wait(15).Until(_ => Element(StartPage.SignInButton).Displayed);

    // Can sign in as any user
    Act.OnStartPage.SignIn(_testList.Collaborators[0]);
    TakeScreenshot(nameof(BasicCollaborationTest), "LogInAsAnyone");

    // Can change user to owner user
    Act.OnMainPage.SignOut();
    Act.OnStartPage.SignIn(_testList.Owner);
    TakeScreenshot(nameof(BasicCollaborationTest), "LogInAsOwner");

    // Can write to realm after changing user
    Act.OnMainPage.CreateList(_testList.Name);
    AssertThat.OnMainPage.ListTagsAreCorrect(_testList.Name, false);
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    Wait().Until(_ => Element(ListPage.AddButton).Displayed);
    Act.OnListPage.AddItemToList(_testList.Items[0]);
    AssertThat.OnListPage.ItemIsCreated(_testList.Items[0]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.EditButton + _testList.Name).Click();
    Act.OnEditListPage.ShareList(_testList.Collaborators[0]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    AssertThat.OnMainPage.ListTagsAreCorrect(_testList.Name, true, true);

    // Change user to collaborator
    Act.OnMainPage.SignOut();
    await Task.Delay(1000);
    Act.OnStartPage.SignIn(_testList.Collaborators[0]);
    await Task.Delay(1000);
    TakeScreenshot(nameof(BasicCollaborationTest), "ChangeToCollaborator");

    // Collaborator can see shared list
    var list = AwaitElement(MainPage.List.ListTitle + _testList.Name, 7);
    Assert.That(list!.Displayed, Is.True);
    AssertThat.OnMainPage.ListTagsAreCorrect(_testList.Name, true);
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    AssertThat.OnListPage.ItemIsCreated(_testList.Items[0]);

    // Collaborator can add items to shared list
    Act.OnListPage.AddItemToList(_testList.Items[1]);
    AssertThat.OnListPage.ItemIsCreated(_testList.Items[1]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    Wait().Until(_ => Element(ListPage.AddButton).Displayed);
    AssertThat.OnListPage.ItemIsCreated(_testList.Items[1]);

    // Collaborator can delete items from shared list
    Act.OnListPage.SwipeDeleteItem(_testList.Items[1].Name);
    Act.NavigateBackAndAwait(MainPage.MenuButton);
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    Wait(5).Until(_ => Element(ListPage.AddButton).Displayed);
    AssertThat.OnListPage.ItemIsDeleted(_testList.Items[1]);
    Act.OnListPage.AddItemToList(_testList.Items[1]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);

    // Change user to owner
    Act.OnMainPage.SignOut();
    Act.OnStartPage.SignIn(_testList.Owner);
    TakeScreenshot(nameof(BasicCollaborationTest), "ChangeToOwner");

    // Owner can see collaborator's changes
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    AssertThat.OnListPage.ItemIsCreated(_testList.Items[0]);
    AssertThat.OnListPage.ItemIsCreated(_testList.Items[1]);
    Act.NavigateBackAndAwait(MainPage.MenuButton);

    // Owner can delete list
    Element(MainPage.List.DeleteButton + _testList.Name).Click();
    AwaitElementXPath(Alert.Yes)!.Click();
    Assert.That(OptionalElement(MainPage.List.ListTitle + _testList.Name), Is.Null);

    // Change user to collaborator again
    Act.OnMainPage.SignOut();
    Act.OnStartPage.SignIn(_testList.Collaborators[0]);
    TakeScreenshot(nameof(BasicCollaborationTest), "ChangeToCollaborator");

    // Deleted list disappears for collaborator
    Assert.That(OptionalElement(MainPage.List.ListTitle + _testList.Name), Is.Null);
  }

  // TODO: Implement the missing collaboration test cases
  // [Test]
  // public void CanCollaborate_EditItemsOwnedBySomeoneElse()
  // {
  //   var item = _currentList.Items[0];
  //   Element(ListPage.Item.Label + item.Name).Click();
  //   Act.OnDetailPage.ChangeItemName(item.Name, EditedPrefix);
  //   Act.OnDetailPage.ChangeItemQuantity(2);
  //   Act.OnDetailPage.SetItemIsImportant(true);
  //   Act.OnDetailPage.ChangeItemCategory(_currentList.Categories[1]);
  //   Act.NavigateBackAndAwait(ListPage.AddButton);
  //   AssertThat.OnListPage.ItemIsCreated(
  //     EditedPrefix + item.Name,
  //     _currentList.Categories[1],
  //     3,
  //     true
  //   );
  //   Element(ListPage.Item.Label + EditedPrefix + item.Name).Click();
  //   Act.OnDetailPage.ChangeItemName(item.Name);
  //   Act.OnDetailPage.ChangeItemQuantity(-2);
  //   Act.OnDetailPage.SetItemIsImportant(item.IsImportant);
  //   Act.OnDetailPage.ChangeItemCategory(item.Category);
  //   Act.NavigateBackAndAwait(ListPage.AddButton);
  // }
  //
  // [Test]
  // public void CanCollaborate_ExitSharedList()
  // {
  //   Element(MainPage.List.ExitButton + _currentList.Name).Click();
  //   AwaitElementXPath(Alert.Yes)!.Click();
  //   var list = OptionalElement(MainPage.List.ListTitle + _currentList.Name);
  //   Assert.That(list, Is.Null);
  // }
  //
  // [Test]
  // public void CanCollaborate_CollaboratorItemsRemainOnListAfterExit()
  // {
  //   Act.OnMainPage.SignOut();
  //   Act.OnStartPage.SignIn(_currentList.Owner);
  //   var element = AwaitElement(MainPage.List.ListTitle + _currentList.Name);
  //   Assert.That(element!.Displayed, Is.True);
  //   AssertThat.OnMainPage.ListTagsAreCorrect(_currentList.Name, false);
  //   element.Click();
  //   var i4 = _currentList.Items[4];
  //   var i5 = _currentList.Items[5];
  //   AssertThat.OnListPage.ItemIsCreated(
  //     NewPrefix + i4.Name,
  //     i4.Category,
  //     i4.Quantity,
  //     i4.IsImportant
  //   );
  //   AssertThat.OnListPage.ItemIsCreated(
  //     NewPrefix + i5.Name,
  //     i5.Category,
  //     i5.Quantity,
  //     i5.IsImportant
  //   );
  // }
  //
  // [Test]
  // public void CanCollaborate_OwnerCanEditCollaboratorsItems()
  // {
  //   var i4 = _currentList.Items[4];
  //   var i5 = _currentList.Items[5];
  //   Element(ListPage.Item.Label + NewPrefix + i4.Name).Click();
  //   Act.OnDetailPage.ChangeItemName(i4.Name);
  //   Act.NavigateBackAndAwait(ListPage.AddButton);
  //   Element(ListPage.Item.Label + NewPrefix + i5.Name).Click();
  //   Act.OnDetailPage.ChangeItemName(i5.Name);
  //   Act.NavigateBackAndAwait(ListPage.AddButton);
  //   AssertThat.OnListPage.ItemIsCreated(i4.Name, i4.Category, i4.Quantity, i4.IsImportant);
  //   AssertThat.OnListPage.ItemIsCreated(i5.Name, i5.Category, i5.Quantity, i5.IsImportant);
  //   Act.NavigateBackAndAwait(MainPage.MenuButton);
  // }

  [OneTimeTearDown]
  public void CleanUp()
  {
    Act.OnMainPage.SignOut();
    Wait(5).Until(_ => Element(StartPage.SignInButton));
  }
}
