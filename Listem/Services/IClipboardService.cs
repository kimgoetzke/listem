using System.Collections.ObjectModel;
using Listem.Models;

namespace Listem.Services;

public interface IClipboardService : IService
{
    ServiceType IService.Type => ServiceType.Clipboard;

    void InsertFromClipboardAsync(
        ObservableCollection<ObservableItem> items,
        ObservableCollection<ObservableCategory> categories,
        string listId
    );

    void CopyToClipboard(
        ObservableCollection<ObservableItem> items,
        ObservableCollection<ObservableCategory> categories
    );
}
