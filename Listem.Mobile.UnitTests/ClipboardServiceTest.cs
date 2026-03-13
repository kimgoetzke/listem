using Listem.Mobile.Services;

namespace Listem.Mobile.UnitTests;

[TestFixture]
public class ClipboardServiceTest
{
  [Test]
  public void ShouldIncludeInExport_NonRecurring_AlwaysTrue()
  {
    Assert.That(ClipboardService.ShouldIncludeInExport(false, true), Is.True);
    Assert.That(ClipboardService.ShouldIncludeInExport(false, false), Is.True);
  }

  [Test]
  public void ShouldIncludeInExport_Recurring_OnlyActiveItems()
  {
    Assert.That(ClipboardService.ShouldIncludeInExport(true, true), Is.True);
    Assert.That(ClipboardService.ShouldIncludeInExport(true, false), Is.False);
  }

  [Test]
  public void ResolveIsImportantOnImport_Recurring_IgnoresParsedImportantFlag()
  {
    Assert.That(ClipboardService.ResolveIsImportantOnImport(true, true), Is.False);
    Assert.That(ClipboardService.ResolveIsImportantOnImport(true, false), Is.False);
  }

  [Test]
  public void ResolveIsImportantOnImport_NonRecurring_UsesParsedImportantFlag()
  {
    Assert.That(ClipboardService.ResolveIsImportantOnImport(false, true), Is.True);
    Assert.That(ClipboardService.ResolveIsImportantOnImport(false, false), Is.False);
  }

  [Test]
  public void ResolveIsActiveOnImport_AlwaysTrue()
  {
    Assert.That(ClipboardService.ResolveIsActiveOnImport(), Is.True);
  }
}
