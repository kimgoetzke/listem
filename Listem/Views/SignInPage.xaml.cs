// ReSharper disable UnusedMember.Local

using Listem.ViewModel;

namespace Listem.Views;

public partial class SignInPage
{
    public SignInPage()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel();
    }
}
