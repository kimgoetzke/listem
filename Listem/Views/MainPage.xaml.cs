using AsyncAwaitBestPractices;
using Listem.Utilities;
using Listem.ViewModel;

// ReSharper disable UnusedMember.Local

namespace Listem.Views;

public partial class MainPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.LoadItemLists().SafeFireAndForget();

        StickyEntry.Submitted += (_, text) =>
        {
            _viewModel.AddListCommand.Execute(text);
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StickyEntry.SetVisibility(false);

        if (!Settings.FirstRun)
            return;

        Logger.Log("First time running this application");
        Settings.FirstRun = false;
    }

    private void AddListButton_OnClicked(object? sender, EventArgs e)
    {
        if (!_viewModel.AddListCommand.CanExecute(null))
            return;

        StickyEntry.SetVisibility(true);
    }
}
