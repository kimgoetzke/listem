using Listem.Mobile.Models;
using Listem.Mobile.ViewModel;
using Listem.Shared.Enums;

namespace Listem.Mobile.Views;

public partial class EditListPage
{
    private readonly EditListViewModel _viewModel;

    public EditListPage(List observableList, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = new EditListViewModel(observableList, serviceProvider);
        BindingContext = _viewModel;

        StickyEntry.Submitted += (_, text) =>
        {
            _viewModel.AddCategoryCommand.Execute(text);
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ListTypePicker.SelectedItem = _viewModel.List.ListType;
        StickyEntry.SetVisibility(false);
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        AddCategoryButton.Focus();
    }

    private void AddCategoryButton_OnClicked(object? sender, EventArgs e)
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

        _viewModel.List.ListType = listType.ToString();
    }
}
