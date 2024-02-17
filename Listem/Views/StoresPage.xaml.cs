using Listem.ViewModel;

namespace Listem.Views;

public partial class StoresPage
{
    public StoresPage(StoresViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        AddStoreButton.Focus();
    }

    private void ImageButton_OnPressed(object? sender, EventArgs e)
    {
        if (sender is not ImageButton button)
            return;
        
        button.Source = "bin_pink.png";
    }

    private void ImageButton_OnReleased(object? sender, EventArgs e)
    {
        if (sender is not ImageButton button)
            return;
        
        button.Source = "bin_neutral.png";
    }
}
