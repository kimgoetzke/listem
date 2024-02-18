using System.Collections.ObjectModel;
using Listem.Models;

namespace Listem.Services;

public interface IClipboardService : IService
{
    ServiceType IService.Type => ServiceType.Clipboard;

    void InsertFromClipboardAsync(
        ObservableCollection<Item> items,
        ObservableCollection<Category> categories
    );

    void CopyToClipboard(
        ObservableCollection<Item> items,
        ObservableCollection<Category> categories
    );
}
