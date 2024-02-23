using Listem.Models;
using Listem.ViewModel;

namespace Listem.Views;

public partial class EditListPage
{
    private readonly EditListViewModel _viewModel;

    public EditListPage(ObservableItemList observableItemList)
    {
        InitializeComponent();
        _viewModel = new EditListViewModel(observableItemList);
        BindingContext = _viewModel;

        StickyEntry.Submitted += (_, text) =>
        {
            _viewModel.AddCategoryCommand.Execute(text);
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StickyEntry.SetVisibility(false);
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

    private void AddListButton_OnClicked(object? sender, EventArgs e)
    {
        if (!_viewModel.AddCategoryCommand.CanExecute(null))
            return;

        StickyEntry.SetVisibility(true);
    }
}
