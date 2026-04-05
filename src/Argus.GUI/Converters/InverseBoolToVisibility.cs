using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Argus.GUI.Converters;

/// <summary>
/// Returns Collapsed when true, Visible when false.
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Visibility v && v == Visibility.Collapsed;
}

/// <summary>
/// Returns the string itself for nav binding (passthrough).
/// </summary>
public sealed class NavVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value ?? "";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value;
}
