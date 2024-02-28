using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using Listem.Events;
using Listem.Models;
using Listem.Utilities;

namespace Listem.Services;

public class ClipboardService(ICategoryService categoryService, IItemService itemService)
    : IClipboardService
{
    public async void InsertFromClipboardAsync(
        ObservableCollection<ObservableItem> observableItems,
        ObservableCollection<ObservableCategory> categories,
        string listId
    )
    {
        var import = await Clipboard.GetTextAsync();

        if (IsClipboardEmpty(import))
            return;

        var stores = await categoryService.GetAllAsync();
        if (
            !WasAbleToConvertToItemList(
                listId,
                import!,
                stores.ToList(),
                out var itemCount,
                out var categoryCount,
                out var itemList,
                out var categoryList
            )
        )
            return;

        if (!await IsImportConfirmedByUser(itemCount, categoryCount))
            return;

        await CreateMissingStores(categories, categoryList);
        await ImportItemList(observableItems, itemList);
        Notifier.ShowToast($"Imported {itemCount} items from clipboard");
    }

    private static bool IsClipboardEmpty(string? import)
    {
        if (import != null)
            return false;

        Notifier.ShowToast("Nothing to import - your clipboard is empty");
        return true;
    }

    private static bool WasAbleToConvertToItemList(
        string listId,
        string import,
        List<ObservableCategory> categories,
        out int itemCount,
        out int categoryCount,
        out List<ObservableItem> itemList,
        out List<ObservableCategory> categoryList
    )
    {
        Logger.Log("Extracted from clipboard: " + import.Replace(Environment.NewLine, ","));
        var categoryName = ICategoryService.DefaultCategoryName;
        itemCount = 0;
        categoryCount = 0;
        itemList = [];
        categoryList = [];
        foreach (var substring in import.Replace(Environment.NewLine, ",").Split(","))
        {
            if (string.IsNullOrWhiteSpace(substring))
                continue;

            if (StringProcessor.IsCategoryName(substring))
            {
                categoryName = AddCategory(
                    listId,
                    categories,
                    ref categoryCount,
                    categoryList,
                    substring
                );
                continue;
            }

            AddItem(ref itemCount, itemList, substring, categoryName, listId);
        }

        if (itemCount != 0)
            return true;

        Notifier.ShowToast("Nothing to import - your clipboard may contain invalid data");
        return false;
    }

    private static void AddItem(
        ref int itemCount,
        List<ObservableItem> itemList,
        string substring,
        string categoryName,
        string listId
    )
    {
        var (title, quantity, isImportant) = StringProcessor.ExtractItem(substring);
        var processedTitle = StringProcessor.TrimAndCapitalise(title);
        var item = new ObservableItem(listId)
        {
            Title = processedTitle,
            CategoryName = categoryName,
            Quantity = quantity,
            IsImportant = isImportant
        };
        itemList.Add(item);
        itemCount++;
    }

    private static string AddCategory(
        string listId,
        List<ObservableCategory> categories,
        ref int categoryCount,
        List<ObservableCategory> categoryList,
        string s
    )
    {
        var extractedName = StringProcessor.ExtractCategoryName(s);
        var matchingStore = categories.Find(c => c.Name == extractedName);

        if (matchingStore != null)
            return extractedName;

        categoryList.Add(new ObservableCategory(listId) { Name = extractedName });
        categoryCount++;
        return extractedName;
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

    private async Task CreateMissingStores(
        ObservableCollection<ObservableCategory> stores,
        List<ObservableCategory> toCreate
    )
    {
        foreach (var store in toCreate)
        {
            await categoryService.CreateOrUpdateAsync(store);
            stores.Add(store);
        }
    }

    private async Task ImportItemList(
        ObservableCollection<ObservableItem> items,
        List<ObservableItem> toImport
    )
    {
        foreach (var item in toImport)
        {
            await itemService.CreateOrUpdateAsync(item);
            var value = new ItemChangedDto(item.ListId, item);
            WeakReferenceMessenger.Default.Send(new ItemAddedToListMessage(value));
            items.Add(item);
        }
    }

    public void CopyToClipboard(
        ObservableCollection<ObservableItem> items,
        ObservableCollection<ObservableCategory> categories
    )
    {
        var text = BuildStringFromList(items, categories);
        Clipboard.SetTextAsync(text);
        Logger.Log("Copied to clipboard: " + text.Replace(Environment.NewLine, ", "));
        Notifier.ShowToast("Copied list to clipboard");
    }

    private static string BuildStringFromList(
        ObservableCollection<ObservableItem> items,
        ObservableCollection<ObservableCategory> categories
    )
    {
        var builder = new StringBuilder();
        foreach (var category in categories)
        {
            var itemsFromStore = items.Where(item => item.CategoryName == category.Name).ToList();
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
