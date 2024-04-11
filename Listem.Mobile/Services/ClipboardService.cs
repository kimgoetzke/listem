using System.Text;
using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Microsoft.Extensions.Logging;

namespace Listem.Mobile.Services;

public class ClipboardService(IServiceProvider sp) : IClipboardService
{
  private readonly ICategoryService _categoryService = sp.GetService<ICategoryService>()!;
  private readonly IItemService _itemService = sp.GetService<IItemService>()!;
  private readonly ILogger<ClipboardService> _logger = sp.GetService<ILogger<ClipboardService>>()!;

  public async void InsertFromClipboardAsync(
    IList<Item> items,
    IList<Category> categories,
    List list
  )
  {
    var import = await Clipboard.GetTextAsync();

    if (IsClipboardEmpty(import))
      return;

    if (
      !WasAbleToExtractCandidates(
        list,
        import!,
        categories.ToList(),
        out var itemCount,
        out var categoryCount,
        out var itemCandidates,
        out var categoryCandidates
      )
    )
      return;

    if (!await IsImportConfirmedByUser(itemCount, categoryCount))
      return;

    await _categoryService.CreateAllAsync(categoryCandidates, list);
    await CreateCandidateItems(itemCandidates);
    Notifier.ShowToast($"Imported {itemCount} items from clipboard");
  }

  private static bool IsClipboardEmpty(string? import)
  {
    if (import != null)
      return false;

    Notifier.ShowToast("Nothing to import - your clipboard is empty");
    return true;
  }

  private bool WasAbleToExtractCandidates(
    List list,
    string import,
    List<Category> existingCategories,
    out int itemCount,
    out int categoryCount,
    out List<Item> itemList,
    out List<Category> categoryCandidates
  )
  {
    _logger.Info("Extracted from clipboard: {Elements}", import.Replace(Environment.NewLine, ","));
    var category = existingCategories.Find(c => c.Name == Constants.DefaultCategoryName)!;
    itemCount = 0;
    categoryCount = 0;
    itemList = [];
    categoryCandidates = [];
    foreach (var substring in import.Replace(Environment.NewLine, ",").Split(","))
    {
      if (string.IsNullOrWhiteSpace(substring))
        continue;

      if (StringProcessor.IsCategoryName(substring))
      {
        if (
          AddValidCategoryCandidate(
            existingCategories,
            ref categoryCount,
            categoryCandidates,
            substring
          ) is
          { } newCategory
        )
          category = newCategory;
        continue;
      }

      AddItemCandidate(ref itemCount, itemList, substring, category, list);
    }

    if (itemCount != 0)
      return true;

    Notifier.ShowToast("Nothing to import - your clipboard may contain invalid data");
    return false;
  }

  private static Category? AddValidCategoryCandidate(
    List<Category> existingCategories,
    ref int categoryCount,
    List<Category> categoryCandidates,
    string str
  )
  {
    var name = StringProcessor.ExtractCategoryName(str);

    if (existingCategories.Find(c => c.Name == name) != null)
      return null;

    var processedName = new Category { Name = name };
    categoryCount++;
    categoryCandidates.Add(processedName);
    return processedName;
  }

  private static void AddItemCandidate(
    ref int itemCount,
    List<Item> itemCandidates,
    string substring,
    Category category,
    List list
  )
  {
    var (title, quantity, isImportant) = StringProcessor.ExtractItem(substring);
    var processedTitle = StringProcessor.TrimAndCapitalise(title);
    var item = new Item
    {
      Name = processedTitle,
      List = list,
      Category = new Category { Name = category.Name },
      Quantity = quantity,
      IsImportant = isImportant,
      OwnedBy = list.OwnedBy
    };
    foreach (var id in list.SharedWith)
    {
      item.SharedWith.Add(id);
    }
    itemCandidates.Add(item);
    itemCount++;
  }

  private static async Task<bool> IsImportConfirmedByUser(int itemCount, int categoryCount)
  {
    var message = CreateToastMessage(itemCount, categoryCount);
    var isConfirmed = await Shell.Current.DisplayAlert(
      "Import from clipboard",
      message,
      "Yes",
      "No"
    );

    if (!isConfirmed)
      Notifier.ShowToast("Import cancelled");

    return isConfirmed;
  }

  private static string CreateToastMessage(int itemCount, int categoryCount)
  {
    return categoryCount > 0
      ? $"Extracted {itemCount} item(s) and {categoryCount} category(/ies) from your clipboard. Would you like to create the missing category(/ies) and add the item(s) to your list?"
      : $"Extracted {itemCount} item(s) from your clipboard. Would you like to add the item(s) to your list?";
  }

  private async Task CreateCandidateItems(List<Item> toCreate)
  {
    foreach (var item in toCreate)
    {
      await _itemService.CreateAsync(item);
    }
  }

  public void CopyToClipboard(IList<Item> items, IList<Category> categories)
  {
    var text = BuildStringFromList(items.ToList(), categories.ToList());
    Clipboard.SetTextAsync(text);
    _logger.Info("Copied to clipboard: {Elements}", text.Replace(Environment.NewLine, ", "));
    Notifier.ShowToast("Copied list to clipboard");
  }

  private static string BuildStringFromList(List<Item> items, List<Category> categories)
  {
    var builder = new StringBuilder();
    foreach (var category in categories)
    {
      var itemsFromStore = items.Where(item => item.Category!.Name == category.Name).ToList();
      if (itemsFromStore.Count == 0)
        continue;
      builder.AppendLine($"[{category.Name}]:");
      foreach (var item in itemsFromStore)
      {
        builder.Append(item);
        if (item.Quantity > 1)
          builder.Append($" ({item.Quantity})");
        if (item.IsImportant)
          builder.Append('!');
        builder.AppendLine();
      }

      builder.AppendLine();
    }

    // Remove last two line breaks as they are only needed to separate stores
    if (builder.Length >= 2)
      builder.Length -= 2;

    return builder.ToString();
  }
}
