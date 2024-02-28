using System.Collections.ObjectModel;
using Listem.Models;
using Listem.Resources.Styles;
#if __ANDROID__
using Android.OS;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using AndroidPlatform = Microsoft.Maui.ApplicationModel.Platform;
#endif

namespace Listem.Utilities;

public static class ThemeHandler
{
    public static void SetCurrentThemeFromSystem(AppTheme? systemTheme)
    {
        Settings.CurrentTheme = systemTheme switch
        {
            AppTheme.Light => Settings.CurrentTheme = Theme.Light,
            AppTheme.Dark => Settings.CurrentTheme = Theme.Dark,
            _ => Settings.CurrentTheme = Theme.Light
        };
    }

    public enum Theme
    {
        Light,
        Dark
    }

    public static ObservableCollection<ObservableTheme> GetAllThemesAsCollection()
    {
        var themes = new ObservableCollection<ObservableTheme>
        {
            new() { Name = Theme.Light },
            new() { Name = Theme.Dark }
        };
        return themes;
    }

    public static void SetTheme(Theme theme)
    {
        var application = Application.Current!;
        ArgumentNullException.ThrowIfNull(application);
        UpdateDictionaries(application, theme);
        SetStatusBarColorOnAndroid(application);
        Settings.CurrentTheme = theme;
        Logger.Log($"Current app theme is: {Settings.CurrentTheme}");
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

    // ReSharper disable once UnusedParameter.Local
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
