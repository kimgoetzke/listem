using System.ComponentModel;
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

    // Evaluates redirect when user comes back from sign in page - is evaluated before token is refreshed on
    // initial app startup, so we need ViewModel_PropertyChanged too; there's probably a cleaner way to do this
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _viewModel.RedirectIfUserIsSignedIn();
    }

    // Evaluates redirect on app startup - doesn't kick when user comes back from sign in page
    // which is why we need to evaluate on OnNavigatedTo too; there's probably a cleaner way to do this
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(_viewModel.IsUserSignedIn))
            return;

        _viewModel.RedirectIfUserIsSignedIn();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }
}
