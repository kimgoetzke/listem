using Listem.Utilities;

namespace Listem;

public partial class App
{
    public App()
    {
        InitializeComponent();
        SetThemeToSystemThemeOnFirstRun();
        var currentTheme = Settings.CurrentTheme;
        ThemeHandler.SetTheme(currentTheme);
        MainPage = new AppShell();
    }

    private static void SetThemeToSystemThemeOnFirstRun()
    {
        if (!Settings.FirstRun)
            return;

        Logger.Log("Setting current theme to system theme on first run");
        var systemTheme = Current?.RequestedTheme;
        ThemeHandler.SetCurrentThemeFromSystem(systemTheme);
    }

    /**
     * This method defines the minimum height and width of the application window for Windows.
     */
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.MinimumHeight = 400;
        window.MinimumWidth = 850;
        return window;
    }
}
