using System.Globalization;
using Listem.Models;
using Listem.Services;
using Listem.ViewModel;

namespace Listem.Views;

public partial class DetailPage
{
    public DetailPage(Item item)
    {
        InitializeComponent();
        var storeService = IPlatformApplication.Current?.Services.GetService<IStoreService>();
        var itemService = IPlatformApplication.Current?.Services.GetService<IItemService>();
        if (itemService is null || storeService is null)
            throw new NullReferenceException("ItemService or StoreService is null");
        BindingContext = new DetailViewModel(item, storeService, itemService);
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
