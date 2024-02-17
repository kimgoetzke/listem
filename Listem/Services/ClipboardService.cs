using System.Collections.ObjectModel;
using System.Text;
using Listem.Models;
using Listem.Utilities;

namespace Listem.Services;

public class ClipboardService(IStoreService storeService, IItemService itemService)
    : IClipboardService
{
    public async void InsertFromClipboardAsync(
        ObservableCollection<ConfigurableStore> observableStores,
        ObservableCollection<Item> observableItems
    )
    {
        var import = await Clipboard.GetTextAsync();
        if (IsClipboardEmpty(import))
            return;
        var stores = await storeService.GetAllAsync();
        if (
            !WasAbleToConvertToItemList(
                import!,
                stores.ToList(),
                out var itemCount,
                out var storeCount,
                out var itemList,
                out var storeList
            )
        )
            return;
        if (!await IsImportConfirmedByUser(itemCount, storeCount))
            return;
        await CreateMissingStores(observableStores, storeList);
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
        string import,
        List<ConfigurableStore> stores,
        out int itemCount,
        out int storeCount,
        out List<Item> itemList,
        out List<ConfigurableStore> storeList
    )
    {
        Logger.Log("Extracted from clipboard: " + import.Replace(Environment.NewLine, ","));
        var storeName = IStoreService.DefaultStoreName;
        itemCount = 0;
        storeCount = 0;
        itemList = [];
        storeList = [];
        foreach (var substring in import.Replace(Environment.NewLine, ",").Split(","))
        {
            if (string.IsNullOrWhiteSpace(substring))
                continue;

            if (StringProcessor.IsStoreName(substring))
            {
                storeName = AddStore(stores, ref storeCount, storeList, substring);
                continue;
            }

            AddItem(ref itemCount, itemList, substring, storeName);
        }

        if (itemCount != 0)
            return true;
        Notifier.ShowToast("Nothing to import - your clipboard may contain invalid data");
        return false;
    }

    private static void AddItem(
        ref int itemCount,
        List<Item> itemList,
        string substring,
        string storeName
    )
    {
        var (title, quantity, isImportant) = StringProcessor.ExtractItem(substring);
        var processedTitle = StringProcessor.TrimAndCapitaliseFirstChar(title);
        var item = new Item
        {
            Title = processedTitle,
            StoreName = storeName,
            Quantity = quantity,
            IsImportant = isImportant
        };
        itemList.Add(item);
        itemCount++;
    }

    private static string AddStore(
        List<ConfigurableStore> stores,
        ref int storeCount,
        List<ConfigurableStore> storeList,
        string s
    )
    {
        var extractedName = StringProcessor.ExtractStoreName(s);
        var matchingStore = stores.Find(store => store.Name == extractedName);
        if (matchingStore != null)
            return extractedName;
        storeList.Add(new ConfigurableStore { Name = extractedName });
        storeCount++;
        return extractedName;
    }

    private static async Task<bool> IsImportConfirmedByUser(int itemCount, int storeCount)
    {
        var message = CreateToastMessage(itemCount, storeCount);
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

    private static string CreateToastMessage(int itemCount, int storeCount)
    {
        return storeCount > 0
            ? $"Extracted {itemCount} item(s) and {storeCount} store(s) from your clipboard. Would you like to create the missing store(s) and add the item(s) to your list?"
            : $"Extracted {itemCount} item(s) from your clipboard. Would you like to add the item(s) to your list?";
    }

    private async Task CreateMissingStores(
        ObservableCollection<ConfigurableStore> stores,
        List<ConfigurableStore> toCreate
    )
    {
        foreach (var store in toCreate)
        {
            await storeService.CreateOrUpdateAsync(store);
            stores.Add(store);
        }
    }

    private async Task ImportItemList(ObservableCollection<Item> items, List<Item> toImport)
    {
        foreach (var item in toImport)
        {
            await itemService.CreateOrUpdateAsync(item);
            items.Add(item);
        }
    }

    public void CopyToClipboard(
        ObservableCollection<Item> items,
        ObservableCollection<ConfigurableStore> stores
    )
    {
        var text = BuildStringFromList(items, stores);
        Clipboard.SetTextAsync(text);
        Logger.Log("Copied to clipboard: " + text.Replace(Environment.NewLine, ", "));
        Notifier.ShowToast("Copied list to clipboard");
    }

    private static string BuildStringFromList(
        ObservableCollection<Item> items,
        ObservableCollection<ConfigurableStore> stores
    )
    {
        var builder = new StringBuilder();
        foreach (var store in stores)
        {
            var itemsFromStore = items.Where(item => item.StoreName == store.Name).ToList();
            if (itemsFromStore.Count == 0)
                continue;
            builder.AppendLine($"[{store.Name}]:");
            foreach (var item in itemsFromStore)
            {
                builder.Append(item.Title);
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
