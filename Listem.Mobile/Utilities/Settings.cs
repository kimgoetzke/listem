namespace Listem.Mobile.Utilities;

public static class Settings
{
    public static bool FirstRun
    {
        get => Preferences.Get(nameof(FirstRun), true);
        set => Preferences.Set(nameof(FirstRun), value);
    }

    public static ThemeHandler.Theme CurrentTheme
    {
        get =>
            (ThemeHandler.Theme)
                Preferences.Get(nameof(CurrentTheme), (int)ThemeHandler.Theme.Light);
        set => Preferences.Set(nameof(CurrentTheme), (int)value);
    }
}
