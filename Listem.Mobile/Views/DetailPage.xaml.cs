using System.Globalization;
using Listem.Mobile.Models;
using Listem.Mobile.ViewModel;

namespace Listem.Mobile.Views;

public partial class DetailPage
{
  public DetailPage(Item item)
  {
    InitializeComponent();

    if (IPlatformApplication.Current?.Services.GetService<IServiceProvider>() is not { } sp)
      throw new NullReferenceException("ServiceProvider is null");

    BindingContext = new DetailViewModel(item, sp);
  }

  private void QuantityStepper_OnValueChanged(object? sender, ValueChangedEventArgs e)
  {
    if (sender is not Stepper)
      return;

    QuantityProperty.Text = e.NewValue.ToString(CultureInfo.CurrentCulture);
  }

  private void IsImportantSwitch_OnToggled(object? sender, ToggledEventArgs _)
  {
    if (sender is not Switch toggle)
      return;

    IsImportantProperty.Text = toggle.IsToggled ? "Yes" : "No";
  }
}
