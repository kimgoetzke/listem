namespace UITests;

public class UnitTest : BaseTest
{
    [Test]
    [Order(1)]
    public async Task CanStartApp()
    {
        var isInstalled = App.IsAppInstalled(AppName);
        Console.WriteLine($"[XXX] {AppName} is installed: {isInstalled}");
        await Task.Delay(TimeSpan.FromSeconds(5));
        FindUiElement("MainPageEntryField");
        TakeScreenshot(nameof(CanStartApp));
    }

    [Test]
    [Order(2)]
    public void CanAddItem()
    {
        FindUiElement("MainPageEntryField").SendKeys("Bread");
        FindUiElement("MainPageIsImportantCheckBox").Click();
        FindUiElement("MainPageAddButton").Click();
        TakeScreenshot(nameof(CanAddItem));
    }
}
