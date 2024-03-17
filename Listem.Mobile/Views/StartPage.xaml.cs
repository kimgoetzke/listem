using Listem.Mobile.ViewModel;

namespace Listem.Mobile.Views;

public partial class StartPage
{
    public StartPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = serviceProvider.GetService<LoginViewModel>();
    }
}
