using Listem.ViewModel;

namespace Listem.Views;

public partial class SignUpPage
{
    public SignUpPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = serviceProvider.GetService<LoginViewModel>();
    }
}
