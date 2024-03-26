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

  private void AddCategoryButton_OnClicked(object? sender, EventArgs e)
  {
    if (!_viewModel.AddCategoryCommand.CanExecute(null))
      return;

    StickyEntry.SetVisibility(true);
  }
}
