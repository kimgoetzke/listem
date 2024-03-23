namespace Listem.Mobile.UITests;

public class UnitTest : BaseTest
{
    [Test]
    [Order(1)]
    public async Task CanStartApp()
    {
        var isInstalled = App.IsAppInstalled(AppName);
        Console.WriteLine($"[XXX] {AppName} is installed: {isInstalled}");
        await Task.Delay(TimeSpan.FromSeconds(5));
        FindUiElement("MenuButton");
        TakeScreenshot(nameof(CanStartApp));
    }

    [Test]
    [Order(2)]
    public void CanCreateList()
    {
        FindUiElement("AddListButton").Click();
        FindUiElement("StickyEntryField").SendKeys("Test List");
        FindUiElement("StickyEntrySubmit").Click();
        TakeScreenshot(nameof(CanCreateList));
    }
}
