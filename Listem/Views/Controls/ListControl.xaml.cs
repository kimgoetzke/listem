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

    private void SwipeItemView_OnInvoked(object? sender, EventArgs e)
    {
        // TODO: Give user feedback through particles or animation
        Logger.Log("OnInvokedSwipeItem");
    }

    private async void CheckBox_OnTaskCompleted(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox { IsChecked: true } checkBox)
            return;

        if (checkBox.BindingContext is not ObservableItem item)
            return;

        var viewModel = BindingContext as ListViewModel;
        viewModel!.ItemsToDelete.Add(item);
        await Task.Delay(200);
    }
}
