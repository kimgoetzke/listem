using System.ComponentModel;
using Listem.Mobile.Utilities;
using Listem.Mobile.ViewModel;

namespace Listem.Mobile.Views;

public partial class StartPage
{
    private readonly LoginViewModel _viewModel;

    public StartPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = serviceProvider.GetService<LoginViewModel>()!;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        BindingContext = _viewModel;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(_viewModel.IsUserSignedIn))
            return;

        Logger.Log("User just signed in, redirecting to MainPage...");
        _viewModel.RedirectIfUserIsSignedIn();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }
}
