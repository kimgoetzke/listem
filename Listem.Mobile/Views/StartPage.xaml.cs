using Listem.Mobile.ViewModel;

namespace Listem.Mobile.Views;

public partial class StartPage
{
    private readonly LoginViewModel _viewModel;

    public StartPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = serviceProvider.GetService<LoginViewModel>()!;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RedirectIfUserIsSignedIn();
    }
}
