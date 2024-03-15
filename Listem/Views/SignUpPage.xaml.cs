using Listem.Services;
using Listem.ViewModel;

namespace Listem.Views;

public partial class SignUpPage
{
    public SignUpPage()
    {
        InitializeComponent();
        var authService = IPlatformApplication.Current?.Services.GetService<AuthService>();

        if (authService is null)
            throw new NullReferenceException("AuthenticationService is null");

        var viewModel = new LoginViewModel(authService);
        BindingContext = viewModel;
        viewModel.Email = authService.CurrentUser.EmailAddress;
    }
}
