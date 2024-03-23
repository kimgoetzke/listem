using Listem.Mobile.ViewModel;

namespace Listem.Mobile.Views;

public partial class SignInPage
{
    public SignInPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = serviceProvider.GetService<LoginViewModel>();
    }
}
