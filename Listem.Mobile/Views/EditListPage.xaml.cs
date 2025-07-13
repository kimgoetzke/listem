using Listem.Mobile.Models;
using Listem.Mobile.ViewModel;

namespace Listem.Mobile.Views;

public partial class EditListPage
{
  private readonly EditListViewModel _viewModel;

  public EditListPage(ObservableList list, IServiceProvider serviceProvider)
  {
    InitializeComponent();
    _viewModel = new EditListViewModel(list, serviceProvider);
    BindingContext = _viewModel;

    StickyEntryCategory.Submitted += (_, text) =>
    {
      _viewModel.AddCategoryCommand.Execute(text);
    };
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    StickyEntryCategory.SetVisibility(false);
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
}
