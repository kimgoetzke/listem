using System.Globalization;
using Listem.Mobile.Models;
using Listem.Mobile.ViewModel;

namespace Listem.Mobile.Views;

public partial class DetailPage
{
    public DetailPage(Item item, List list)
    {
        InitializeComponent();

        if (
            IPlatformApplication.Current?.Services.GetService<IServiceProvider>() is
            { } serviceProvider
        )
        {
            BindingContext = new DetailViewModel(item, list, serviceProvider);
        }
        throw new NullReferenceException("ServiceProvider is null");
    }

    private void QuantityStepper_OnValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (sender is not Stepper)
            return;

        QuantityProperty.Text = e.NewValue.ToString(CultureInfo.CurrentCulture);
    }

    private void IsImportantSwitch_OnToggled(object? sender, ToggledEventArgs toggledEventArgs)
    {
        if (sender is not Switch toggle)
            return;

        IsImportantProperty.Text = toggle.IsToggled ? "Yes" : "No";
    }
}
