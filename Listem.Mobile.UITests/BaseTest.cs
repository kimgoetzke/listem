using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Support.UI;

namespace Listem.Mobile.UITests;

public abstract class BaseTest
{
  protected static bool IsInstalled => App.IsAppInstalled(AppiumSetup.AppName);
  private static AppiumDriver App => AppiumSetup.AppiumDriver;
  private readonly string _date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
  protected const int DefaultWaitSec = 2;
  private const int DefaultIntervalMs = 500;

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

  protected static WebDriverWait Wait(
    int seconds = DefaultWaitSec,
    int interval = DefaultIntervalMs
  )
  {
    return new WebDriverWait(App, TimeSpan.FromSeconds(seconds))
    {
      PollingInterval = TimeSpan.FromMilliseconds(interval)
    };
  }

  protected static AppiumElement? AwaitElement(
    string id,
    int seconds = DefaultWaitSec,
    int interval = DefaultIntervalMs
  )
  {
    return Wait(seconds, interval)
      .Until(_ =>
      {
        var label = Element(id);
        return label.Displayed ? label : null;
      });
  }

  protected static AppiumElement? AwaitElementXPath(
    string id,
    int seconds = DefaultWaitSec,
    int interval = DefaultIntervalMs
  )
  {
    return Wait(seconds, interval)
      .Until(_ =>
      {
        var label = ElementXPath(id);
        return label.Displayed ? label : null;
      });
  }

  protected static AppiumElement? OptionalElement(string id)
  {
    try
    {
      return Element(id);
    }
    catch (Exception)
    {
      return null;
    }
  }

  protected void TakeScreenshot(string name)
  {
    App.GetScreenshot().SaveAsFile($"{_date}-{name}.png");
    Console.WriteLine(
      $@"[XXX] Took screenshot and saved as: Listem.Mobile.UITests\bin\Debug\net8.0\{_date}-{name}.png"
    );
  }

  [SuppressMessage("ReSharper", "UnusedMember.Global")]
  protected static void LogDisplayStatus(string id)
  {
    try
    {
      Element(id);
    }
    catch (Exception)
    {
      Console.WriteLine($"[XXX] Element '{id}' not found");
      return;
    }
    Console.WriteLine($"[XXX] Element '{id}' is displayed");
  }
}
