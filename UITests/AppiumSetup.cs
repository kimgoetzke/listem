using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;

namespace UITests;

[SetUpFixture]
public class AppiumSetup
{
    public static AppiumDriver AppiumDriver =>
        _driver ?? throw new NullReferenceException("AppiumDriver is null");

    public const string AppName = "io.kimgoetzke.listem";

    private const string MainActivityName = "crc647ac5fe0ad803fabb.MainActivity";
    private const string Avd = "Pixel_3a_API_34_extension_level_7_x86_64";
    private static AppiumDriver? _driver;

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        AppiumServerHelper.StartAppiumLocalServer();
        var androidOptions = new AppiumOptions
        {
            AutomationName = AutomationName.AndroidUIAutomator2,
            PlatformName = "Android",
            PlatformVersion = "14",
            DeviceName = "Android Emulator",

            // I am unable to install the app with Appium as it will crash right after launching. This may be an issue
            // with Appium - for example:
            // https://discuss.appium.io/t/maui-app-keeps-crashing-after-launching-app-using-appium-return-com-companyname-mauiapp1-crc64e632a077a20c694c-mainactivity-never-started-error/41509/16
            // Once it works, add this above:
            // var apk = Environment.GetEnvironmentVariable("SHOPPING_LIST_RELEASE_APK");
            // Then uncomment this:
            // App = apk + "/io.kimgoetzke.listem-Signed.apk",
        };
        androidOptions.AddAdditionalAppiumOption("appPackage", AppName);
        androidOptions.AddAdditionalAppiumOption("appActivity", MainActivityName);
        androidOptions.AddAdditionalAppiumOption("avd", Avd);
        androidOptions.AddAdditionalAppiumOption("noReset", true);
        // androidOptions.AddAdditionalAppiumOption("appium:appWaitActivity", "crc647ac5fe0ad803fabb.MainActivity");
        // androidOptions.AddAdditionalAppiumOption("appium:forceAppLaunch", true);
        _driver = new AndroidDriver(androidOptions);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        _driver?.Quit();
        AppiumServerHelper.DisposeAppiumLocalServer();
    }
}
