using Listem.Mobile.Views;

namespace Listem.Mobile;

public partial class AppShell
{
  public AppShell()
  {
    InitializeComponent();
    Routing.RegisterRoute(nameof(ListPage), typeof(ListPage));
    Routing.RegisterRoute(nameof(EditListPage), typeof(EditListPage));
    Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
  }
}
