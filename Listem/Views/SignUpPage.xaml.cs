// ReSharper disable UnusedMember.Local

using Listem.ViewModel;

namespace Listem.Views;

public partial class SignUpPage
{
    public SignUpPage()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel();
    }
}
