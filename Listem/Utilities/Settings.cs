using System.Collections.ObjectModel;
using Listem.Resources.Styles;
#if __ANDROID__
using Android.OS;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using AndroidPlatform = Microsoft.Maui.ApplicationModel.Platform;
#endif

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

    public static void SetCurrentThemeFromSystem(AppTheme? systemTheme)
    {
        CurrentTheme = systemTheme switch
        {
            AppTheme.Light => CurrentTheme = Theme.Light,
            AppTheme.Dark => CurrentTheme = Theme.Dark,
            _ => CurrentTheme = Theme.Light
        };
    }

    public enum Theme
    {
        Light,
        Dark
    }

    public static ObservableCollection<Models.ObservableTheme> GetAllThemesAsCollection()
    {
        var themes = new ObservableCollection<Models.ObservableTheme>
        {
            new() { Name = Theme.Light },
            new() { Name = Theme.Dark }
        };
        return themes;
    }

    public static void LoadTheme(Theme theme)
    {
        var application = Application.Current!;
        ArgumentNullException.ThrowIfNull(application);
        UpdateDictionaries(application, theme);
        SetStatusBarColorOnAndroid(application);
        CurrentTheme = theme;
        Logger.Log($"Current app theme is: {CurrentTheme}");
    }

    private static void UpdateDictionaries(Application application, Theme theme)
    {
        var mergedDictionaries = application.Resources.MergedDictionaries;
        if (mergedDictionaries == null)
            return;
        mergedDictionaries.Clear();
        switch (theme)
        {
            case Theme.Dark:
                mergedDictionaries.Add(new DarkTheme());
                break;
            case Theme.Light:
            default:
                mergedDictionaries.Add(new LightTheme());
                break;
        }
        mergedDictionaries.Add(new Styles());
    }

    private static void SetStatusBarColorOnAndroid(Application application)
    {
#if __ANDROID__
        if (!application.Resources.TryGetValue("StatusBarColor", out var colorValue))
        {
            Logger.Log("StatusBarColor not found in MergedDictionaries");
            return;
        }
        var statusBarColor = (Color)colorValue;

        if (
            AndroidPlatform.CurrentActivity?.Window == null
            || Build.VERSION.SdkInt < BuildVersionCodes.O
        )
        {
            return;
        }
        AndroidPlatform.CurrentActivity.Window.SetStatusBarColor(statusBarColor.ToAndroid());
#endif
    }
}
