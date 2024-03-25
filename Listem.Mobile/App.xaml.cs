using Listem.Mobile.Utilities;

namespace Listem.Mobile;

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

        var systemTheme = Current?.RequestedTheme;
        ThemeHandler.SetCurrentThemeFromSystem(systemTheme);
    }
}
