using Listem.Mobile.Utilities;

namespace Listem.Mobile.UnitTests.Utilities;

[TestFixture]
public class StatusBarStyleResolverTests
{
  [Test]
  public void ShouldUseDarkStatusBarIcons_DarkTheme_ReturnsFalse()
  {
    var shouldUseDarkIcons = StatusBarStyleResolver.ShouldUseDarkStatusBarIcons(true);

    Assert.That(shouldUseDarkIcons, Is.False);
  }

  [Test]
  public void ShouldUseDarkStatusBarIcons_LightTheme_ReturnsTrue()
  {
    var shouldUseDarkIcons = StatusBarStyleResolver.ShouldUseDarkStatusBarIcons(false);

    Assert.That(shouldUseDarkIcons, Is.True);
  }
}
