namespace Listem.Mobile.Utilities;

public static class StatusBarStyleResolver
{
  public static bool ShouldUseDarkStatusBarIcons(bool isDarkTheme) => !isDarkTheme;
}
