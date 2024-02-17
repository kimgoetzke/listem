using Listem.Utilities;
using Listem;

namespace Listem;

public partial class App
{
    public App()
    {
        InitializeComponent();
        SetThemeToSystemThemeOnFirstRun();
        var currentTheme = Settings.CurrentTheme;
        Settings.LoadTheme(currentTheme);
        MainPage = new AppShell();
    }

    private static void SetThemeToSystemThemeOnFirstRun()
    {
        if (!Settings.FirstRun)
            return;

        Logger.Log("Setting current theme to system theme on first run");
        var systemTheme = Current?.RequestedTheme;
        Settings.SetCurrentThemeFromSystem(systemTheme);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.MinimumHeight = 400;
        window.MinimumWidth = 850;
        return window;
    }
}
