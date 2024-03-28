using AsyncAwaitBestPractices;
using Listem.Mobile.Services;
using Listem.Mobile.Utilities;

namespace Listem.Mobile;

public partial class App
{
  public App()
  {
    InitializeComponent();
    LoadEncryptionKeyAsync().SafeFireAndForget();
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

  private static async Task LoadEncryptionKeyAsync()
  {
    if (await SecureStorage.Default.GetAsync(Constants.LocalEncryptionKey) is { } key)
      RealmService.ExistingEncryptionKey = Convert.FromBase64String(key);
  }
}
