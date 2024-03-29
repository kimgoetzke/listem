using System.Globalization;

namespace Listem.Mobile.Converters;

public class QuantityGreaterThanOneConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is int quantity)
    {
      return quantity > 1;
    }

    return false;
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}
