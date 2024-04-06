using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Support.UI;

namespace Listem.Mobile.UITests;

public abstract class BaseTest
{
    protected static AppiumDriver App => AppiumSetup.AppiumDriver;
    protected static string AppName => AppiumSetup.AppName;
    private readonly string _date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");

    protected static WebDriverWait Wait(int seconds = 2, int interval = 500)
    {
        return new WebDriverWait(App, TimeSpan.FromSeconds(seconds))
        {
            PollingInterval = TimeSpan.FromMilliseconds(interval)
        };
    }

    protected static AppiumElement Element(string id)
    {
        return App.FindElement(Id(id));
    }

    protected static AppiumElement ElementXPath(string id)
    {
        return App.FindElement(By.XPath(id));
    }

    protected static By Id(string id)
    {
        return App is WindowsDriver ? MobileBy.AccessibilityId(id) : MobileBy.Id(id);
    }

    protected static AppiumElement? AwaitElement(string id, int seconds = 2)
    {
        return Wait(seconds)
            .Until(_ =>
            {
                var label = Element(id);
                return label.Displayed ? label : null;
            });
    }

    protected static AppiumElement? AwaitElementXPath(string id, int seconds = 2)
    {
        return Wait(seconds)
            .Until(_ =>
            {
                var label = ElementXPath(id);
                return label.Displayed ? label : null;
            });
    }

    protected static void LogDisplayStatus(string id)
    {
        try
        {
            Element(id);
        }
        catch (Exception _)
        {
            Console.WriteLine($"[XXX] Element '{id}' not found");
            return;
        }
        Console.WriteLine($"[XXX] Element '{id}' is displayed");
    }

    protected void TakeScreenshot(string name)
    {
        App.GetScreenshot().SaveAsFile($"{_date}-{name}.png");
        Console.WriteLine(
            $@"[XXX] Took screenshot: Listem.Mobile.UITests\bin\Debug\net8.0\{_date}-{name}.png"
        );
    }
}
