using Listem.Mobile.Models;
using Listem.Mobile.ViewModel;
using Microsoft.Extensions.Logging;

namespace Listem.Mobile.Views.Controls;

public partial class ListControl
{
  private readonly ILogger<ListControl> _logger;

  public ListControl()
  {
    InitializeComponent();
    _logger = IPlatformApplication.Current!.Services.GetService<ILogger<ListControl>>()!;
  }

  private void CheckBox_OnCheckChanged(object? sender, CheckedChangedEventArgs e)
  {
    if (sender is not CheckBox checkBox)
      return;

    if (checkBox.BindingContext is not ObservableItem item)
      return;

    var viewModel = BindingContext as ListViewModel;
    _logger.Debug("CheckBox_OnCheckChanged: {0} - {1}", item.Title, checkBox.IsChecked);
    if (checkBox.IsChecked)
    {
      viewModel!.ItemsToDelete.Add(item);
      return;
    }

    viewModel!.ItemsToDelete.Remove(item);
  }
}
