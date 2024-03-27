using Listem.Mobile.Models;
using Listem.Mobile.ViewModel;

namespace Listem.Mobile.Views;

public partial class EditListPage
{
  private readonly EditListViewModel _viewModel;

  public EditListPage(List list, IServiceProvider serviceProvider)
  {
    InitializeComponent();
    _viewModel = new EditListViewModel(list, serviceProvider);
    BindingContext = _viewModel;

    StickyEntryCategory.Submitted += (_, text) =>
    {
      _viewModel.AddCategoryCommand.Execute(text);
    };
    StickyEntryShareWith.Submitted += (_, text) =>
    {
      _viewModel.ShareCommand.Execute(text);
    };
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    StickyEntryCategory.SetVisibility(false);
    StickyEntryShareWith.SetVisibility(false);
  }

  private void OnEntryUnfocused(object sender, FocusEventArgs e)
  {
    AddCategoryButton.Focus();
  }

  private void AddCategoryButton_OnClicked(object? sender, EventArgs e)
  {
    if (!_viewModel.AddCategoryCommand.CanExecute(null))
      return;

    StickyEntryCategory.SetVisibility(true);
  }

  private void ShareWithButton_OnClicked(object? sender, EventArgs e)
  {
    if (!_viewModel.ShareCommand.CanExecute(null))
      return;

    StickyEntryShareWith.SetVisibility(true);
  }
}
