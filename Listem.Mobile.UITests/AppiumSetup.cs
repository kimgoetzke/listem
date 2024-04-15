using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;

namespace Listem.Mobile.UITests;

[SetUpFixture]
[SuppressMessage(
  "Structure",
  "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method"
)] // This is because .Quit() is called in [OneTimeTearDown] and this includes .Dispose()
public class AppiumSetup
{
  public static AppiumDriver AppiumDriver =>
    _driver ?? throw new NullReferenceException("AppiumDriver is null");
  public const string AppName = "io.kimgoetzke.listem";
  private const string MainActivityName = "crc644cb8d77eec54dc0d.MainActivity";
  private const string Avd = "Pixel_3a_API_34_extension_level_7_x86_64";
  private static AppiumDriver? _driver;

  [OneTimeSetUp]
  public void RunBeforeAnyTests()
  {
    AppiumServerHelper.StartAppiumLocalServer();
    var apk = Environment.GetEnvironmentVariable("LISTEM_DEBUG_APK");
    var androidOptions = new AppiumOptions
    {
      AutomationName = AutomationName.AndroidUIAutomator2,
      PlatformName = "Android",
      PlatformVersion = "14",
      DeviceName = "Android Emulator",
      App = apk + "/io.kimgoetzke.listem-Signed.apk"
    };
    androidOptions.AddAdditionalAppiumOption("appPackage", AppName);
    androidOptions.AddAdditionalAppiumOption("appActivity", MainActivityName);
    androidOptions.AddAdditionalAppiumOption("avd", Avd);
    androidOptions.AddAdditionalAppiumOption("noReset", true);
    // androidOptions.AddAdditionalAppiumOption("appium:appWaitActivity", "crc644cb8d77eec54dc0d.MainActivity");
    // androidOptions.AddAdditionalAppiumOption("appium:forceAppLaunch", true);
    _driver = new AndroidDriver(androidOptions);
  }

  [OneTimeTearDown]
  public void RunAfterAnyTests()
  {
    // var result = _driver?.TerminateApp(AppName);
    // Console.WriteLine($"[XXX] [{AppName}] Terminated app: {result}");
    _driver?.Quit();
    AppiumServerHelper.DisposeAppiumLocalServer();
  }
}
