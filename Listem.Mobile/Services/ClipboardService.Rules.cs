using System.Collections.ObjectModel;
using System.Text;
using Listem.Mobile.Models;

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

  internal static string BuildStringFromList(
    ObservableCollection<ObservableItem> items,
    ObservableCollection<ObservableCategory> categories,
    bool isRecurring
  )
  {
    var builder = new StringBuilder();
    foreach (var category in categories)
    {
      var itemsFromCategory = items
        .Where(item => item.CategoryName == category.Name)
        .Where(item => ShouldIncludeInExport(isRecurring, item.IsActive))
        .ToList();
      if (itemsFromCategory.Count == 0)
        continue;

      builder.AppendLine($"[{category.Name}]:");
      foreach (var item in itemsFromCategory)
      {
        builder.Append(item);
        if (item.Quantity > 1)
          builder.Append($" ({item.Quantity})");
        if (!isRecurring && item.IsImportant)
          builder.Append('!');
        builder.AppendLine();
      }

      builder.AppendLine();
    }

    // Remove last two line breaks as they are only needed to separate categories
    var trailingSeparatorLength = Environment.NewLine.Length * 2;
    if (builder.Length >= trailingSeparatorLength)
      builder.Length -= trailingSeparatorLength;

    return builder.ToString();
  }
}
