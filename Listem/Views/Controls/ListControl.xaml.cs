using Listem.Models;
using Listem.Utilities;
using Listem.ViewModel;

namespace Listem.Views.Controls;

public partial class ListControl
{
    public ListControl()
    {
        InitializeComponent();
    }

    private void CheckBox_OnCheckChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        if (checkBox.BindingContext is not ObservableItem item)
            return;

        var viewModel = BindingContext as ListViewModel;

        if (checkBox.IsChecked)
        {
            viewModel!.ItemsToDelete.Add(item);
            return;
        }

        viewModel!.ItemsToDelete.Remove(item);
    }
}
