// ReSharper disable MemberCanBePrivate.Global
namespace Listem.Mobile.Services;

public partial class ClipboardService
{
  internal static bool ShouldIncludeInExport(bool isRecurring, bool isActive)
  {
    return !isRecurring || isActive;
  }

  internal static bool ResolveIsImportantOnImport(bool isRecurring, bool parsedIsImportant)
  {
    return !isRecurring && parsedIsImportant;
  }

  internal static bool ResolveIsActiveOnImport()
  {
    return true;
  }
}
