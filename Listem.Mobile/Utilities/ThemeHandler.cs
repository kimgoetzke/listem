using System.Collections.ObjectModel;
using Listem.Mobile.Models;
using Listem.Mobile.Resources.Styles;
using Microsoft.Extensions.Logging;
#if __ANDROID__
using Android.OS;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using AndroidPlatform = Microsoft.Maui.ApplicationModel.Platform;
#endif

namespace Listem.Mobile.Utilities;

public static class ThemeHandler
{
  private static ILogger Logger => LoggerProvider.CreateLogger("ThemeService");

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
    Logger.Info("Current app theme is: {Theme}", Settings.CurrentTheme);
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
      Logger.Warn("StatusBarColor not found in MergedDictionaries");
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
    if (!OperatingSystem.IsAndroidVersionAtLeast(35))
      AndroidPlatform.CurrentActivity.Window.SetStatusBarColor(statusBarColor.ToAndroid());
#endif
  }

  public static CommunityToolkit.Maui.Core.StatusBarStyle GetStatusBarStyleForCurrentTheme()
  {
    return Settings.CurrentTheme == Theme.Dark
      ? CommunityToolkit.Maui.Core.StatusBarStyle.LightContent
      : CommunityToolkit.Maui.Core.StatusBarStyle.DarkContent;
  }

  /// Resets the status bar to the theme's standard background colour. Call this from
  /// <c>OnAppearing()</c> on any page that does not manage its own custom status bar colour,
  /// to guard against Android resetting the colour to the native theme default on navigation.
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Interoperability",
    "CA1416:Validate platform compatibility"
  )]
  public static void ResetStatusBarToThemeColour()
  {
#if __ANDROID__ || __IOS__
    var application = Application.Current;
    if (application == null)
      return;

    if (!application.Resources.TryGetValue("StatusBarColor", out var colorValue))
    {
      Logger.Warn("StatusBarColor not found in resources");
      return;
    }

    var statusBarColor = (Color)colorValue;
    CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(statusBarColor);
    CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(GetStatusBarStyleForCurrentTheme());
#endif
  }
}
