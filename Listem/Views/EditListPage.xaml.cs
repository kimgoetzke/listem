using Listem.Models;
using Listem.ViewModel;

namespace Listem.Views;

public partial class EditListPage
{
    public EditListPage(ObservableItemList observableItemList)
    {
        InitializeComponent();
        BindingContext = new EditListViewModel(observableItemList);
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        AddCategoryButton.Focus();
    }

    private void ImageButton_OnPressed(object? sender, EventArgs e)
    {
        if (sender is not ImageButton button)
            return;

        button.Source = "bin_secondary.png";
    }

    private void ImageButton_OnReleased(object? sender, EventArgs e)
    {
        if (sender is not ImageButton button)
            return;

        button.Source = "bin_neutral.png";
    }
}
