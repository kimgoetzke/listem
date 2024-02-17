using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace UITests;

public abstract class BaseTest
{
    protected static AppiumDriver App => AppiumSetup.AppiumDriver;
    protected static string AppName => AppiumSetup.AppName;

    private readonly string _date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");

    protected static AppiumElement FindUiElement(string id)
    {
        return App.FindElement(
            App is WindowsDriver ? MobileBy.AccessibilityId(id) : MobileBy.Id(id)
        );
    }

    protected void TakeScreenshot(string name)
    {
        App.GetScreenshot().SaveAsFile($"{_date}-{name}.png");
        Console.WriteLine($"[XXX] Took screenshot: UITests\\bin\\Debug\\net8.0\\{_date}-{name}.png");
    }
}
