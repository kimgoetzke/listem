using Listem.ViewModel;

// ReSharper disable UnusedMember.Local

namespace Listem.Views;

public partial class MainPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
}
