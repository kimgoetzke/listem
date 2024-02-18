using System.Collections.ObjectModel;
using Listem.Models;

namespace Listem.Services;

public interface IClipboardService : IService
{
    ServiceType IService.Type => ServiceType.Clipboard;

    void InsertFromClipboardAsync(
        ObservableCollection<Category> stores,
        ObservableCollection<Item> items
    );

    void CopyToClipboard(
        ObservableCollection<Item> items,
        ObservableCollection<Category> stores
    );
}
