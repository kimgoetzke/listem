using static Listem.Mobile.UITests.AutomationIdModel;
using static Listem.Mobile.UITests.TestHelper;

namespace Listem.Mobile.UITests.Tests;

public class UserTest : BaseTest
{
  private readonly TestData.TestList _testList = TestData.UserList;

  [OneTimeSetUp]
  public void SetUp()
  {
    Console.WriteLine($"[XXX] {AppiumSetup.AppName} is installed: {IsInstalled}");
    if (!IsInstalled)
    {
      Assert.Fail($"{AppiumSetup.AppName} is not installed");
    }
  }

  [Test]
  public void RegisterAndDeleteUserTest()
  {
    // Navigate to sign up page
    Wait(15).Until(_ => Element(StartPage.SignInButton).Displayed);
    Element(StartPage.SignInButton).Click();
    AwaitElement(SignInPage.SignUpButton)!.Click();
    AwaitElement(SignUpPage.SignUpButton);

    // Register new user
    Element(SignUpPage.EmailEntry).SendKeys(_testList.Owner.Email);
    Element(SignUpPage.PasswordEntry).SendKeys(_testList.Owner.Password);
    Element(SignUpPage.PasswordConfirmedEntry).SendKeys(_testList.Owner.Password);
    Element(SignUpPage.SignUpButton).Click();

    // Sign in with new user
    AwaitElement(SignInPage.SignInButton, 7);
    TakeScreenshot(nameof(RegisterAndDeleteUserTest), "1-Registered");
    Assert.That(Element(SignInPage.EmailEntry).Text, Is.EqualTo(_testList.Owner.Email));
    Element(SignInPage.PasswordEntry).SendKeys(_testList.Owner.Password);
    Element(SignInPage.SignInButton).Click();
    Wait(8).Until(_ => Element(MainPage.MenuButton).Displayed);

    // Create list
    Act.OnMainPage.CreateList(_testList.Name);
    TakeScreenshot(nameof(RegisterAndDeleteUserTest), "2-CreatedList");

    // Navigate to ListPage
    Element(MainPage.List.ListTitle + _testList.Name).Click();
    AwaitElement(ListPage.AddButton);
    Act.OnListPage.AddItemToList(_testList.Items[0]);
    AssertThat.OnListPage.ItemIsCreated(_testList.Items[0]);
    TakeScreenshot(nameof(RegisterAndDeleteUserTest), "3-ItemCreated");

    // Navigate back
    Act.NavigateBackAndAwait(MainPage.MenuButton, 5);

    // Delete user and all data
    Element(MainPage.MenuButton).Click();
    AwaitElement(MainPage.Menu.DeleteAccountButton)!.Click();
    AwaitElementXPath(Alert.Yes, 5);
    TakeScreenshot(nameof(RegisterAndDeleteUserTest), "4-DeletePrompt");
    ElementXPath(Alert.Yes).Click();
    AwaitElement(StartPage.SignInButton, 10);
    TakeScreenshot(nameof(RegisterAndDeleteUserTest), "5-UserDeleted");
  }
}
