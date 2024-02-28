using static Listem.Utilities.ThemeHandler;

namespace Listem.Utilities;

public static class Settings
{
    public static bool FirstRun
    {
        get => Preferences.Get(nameof(FirstRun), true);
        set => Preferences.Set(nameof(FirstRun), value);
    }

    public static Theme CurrentTheme
    {
        get => (Theme)Preferences.Get(nameof(CurrentTheme), (int)Theme.Light);
        set => Preferences.Set(nameof(CurrentTheme), (int)value);
    }
}
