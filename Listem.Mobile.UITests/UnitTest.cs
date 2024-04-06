namespace Listem.Mobile.UITests;

public class UnitTest : BaseTest
{
    private const string ListName1 = "List-1";
    private const string DefaultCategoryName = "None";

    [Test]
    [Order(1)]
    public void CanStartApp()
    {
        var isInstalled = App.IsAppInstalled(AppName);
        Console.WriteLine($"[XXX] {AppName} is installed: {isInstalled}");
        Wait(15).Until(_ => Element("SignInButton"));
        TakeScreenshot(nameof(CanStartApp));
    }

    [Test]
    [Order(2)]
    public void CanSignIn()
    {
        Element("SignInButton").Click();
        Wait().Until(_ => Element("EmailEntry").Displayed);
        Element("EmailEntry").SendKeys("someone@example");
        Element("PasswordEntry").SendKeys("Password1!");
        Element("SignInButton").Click();
        Wait(8).Until(_ => Element("MenuButton").Displayed);
        TakeScreenshot(nameof(CanSignIn));
    }

    [Test]
    [Order(10)]
    public void CanCreateList()
    {
        Wait().Until(_ => Element("AddListButton").Displayed);
        Element("AddListButton").Click();
        Element("StickyEntryField").SendKeys(ListName1);
        Element("StickyEntrySubmit").Click();
        Wait().Until(_ => Element("ListTitle_" + ListName1).Displayed);
        TakeScreenshot(nameof(CanCreateList));
    }

    [Test]
    [Order(11)]
    public void CanOpenNewList()
    {
        // LogDisplayStatus("ListTitle_" + ListName1);
        // LogDisplayStatus("SharedTag_" + ListName1);
        // LogDisplayStatus("CollaboratorTag_" + ListName1);
        // LogDisplayStatus("OwnerTag_" + ListName1);
        // LogDisplayStatus("EditList_" + ListName1);
        // LogDisplayStatus("EmptyListLabel_" + ListName1);
        // LogDisplayStatus("DeleteButton_" + ListName1);
        // LogDisplayStatus("ExitButton_" + ListName1);

        Element("ListTitle_" + ListName1).Click();
        Wait(5).Until(_ => Element("ListPageAddButton").Displayed);
        TakeScreenshot(nameof(CanCreateList));
    }

    [Test]
    [Order(11)]
    public void CanAddItemsToList()
    {
        // LogDisplayStatus("ListPageCategoryPicker");
        // LogDisplayStatus("ListPageAddButton");
        // LogDisplayStatus("ListPageEntryField");
        // LogDisplayStatus("ListPageQuantityStepper");
        // LogDisplayStatus("ListPageIsImportantSwitch");

        const string ItemName1 = "Item-1";
        const bool isImportant = true;
        const string categoryName = DefaultCategoryName;

        Element("ListPageEntryField").SendKeys(ItemName1);
        var isImportantSwitch = Element("ListPageIsImportantSwitch");
        if (!isImportantSwitch.Selected && isImportant)
        {
            isImportantSwitch.Click();
        }
        Assert.That(isImportantSwitch.Selected, Is.True);
        var categoryPicker = Element("ListPageCategoryPicker");
        categoryPicker.Click();
        AwaitElementXPath("//android.widget.TextView[@resource-id=\"android:id/text1\"]")?.Click();
        Element("ListPageAddButton").Click();
        Assert.That(categoryPicker.Text, Is.EqualTo(categoryName));

        var label = AwaitElement("Label_" + ItemName1);
        Assert.That(label?.Text, Is.EqualTo(ItemName1));

        // Once item is added:
        // LogDisplayStatus("DoneBox_" + ItemName1);
        // LogDisplayStatus("Label_" + ItemName1);
        // LogDisplayStatus("CategoryTag_" + ItemName1);
        // LogDisplayStatus("IsImportantIcon_" + ItemName1);
    }

    [Test]
    [Order(99)]
    [Ignore("This test is not ready yet")]
    public void CanSignOutFromMainPage()
    {
        Wait().Until(_ => Element("MenuButton").Displayed);
        Element("MenuButton").Click();
        Element("SignUpInOrOutButton").Click();
        Wait(5).Until(_ => Element("SignInButton"));
        TakeScreenshot(nameof(CanSignOutFromMainPage));
    }
}
