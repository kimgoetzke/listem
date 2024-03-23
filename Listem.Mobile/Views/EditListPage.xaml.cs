using Listem.Mobile.Models;
using Listem.Mobile.ViewModel;
using Listem.Shared.Enums;

namespace Listem.Mobile.Views;

public partial class EditListPage
{
    private readonly EditListViewModel _viewModel;

    public EditListPage(ObservableList observableList)
    {
        InitializeComponent();
        _viewModel = new EditListViewModel(observableList);
        BindingContext = _viewModel;

        StickyEntry.Submitted += (_, text) =>
        {
            _viewModel.AddCategoryCommand.Execute(text);
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ListTypePicker.SelectedIndex = (int)_viewModel.ObservableList.ListType;
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

    private void ListTypePicker_OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender is not Picker picker)
            return;

        if (picker.SelectedItem is not ListType listType)
            return;

        _viewModel.ObservableList.ListType = listType;
    }
}
