using System.Globalization;

namespace Listem.Mobile.Converters;

public class BoolToYesNoConverter : IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is bool boolean)
    {
      return boolean ? "Yes" : "No";
    }

    throw new ArgumentException("Converter must not be used with a non-boolean value");
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is string stringValue)
    {
      return stringValue.Equals("Yes", StringComparison.OrdinalIgnoreCase);
    }

    return false;
  }
}
