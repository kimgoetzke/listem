using Listem.Mobile.ViewModel;

namespace Listem.Mobile.Views;

public partial class SignUpPage
{
  public SignUpPage(IServiceProvider serviceProvider)
  {
    InitializeComponent();
    BindingContext = serviceProvider.GetService<LoginViewModel>();
  }
}
