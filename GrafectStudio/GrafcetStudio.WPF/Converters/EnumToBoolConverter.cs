using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GrafcetStudio.WPF.Converters;

/// <summary>
/// Converts an enum value to <see langword="bool"/> by comparing it to a named
/// <see cref="ConverterParameter"/> string.  Used to bind <see cref="CanvasMode"/>
/// to ToggleButton.IsChecked.
/// <list type="bullet">
///   <item>Convert:     (int)value == (int)Enum.Parse(value.GetType(), parameter) → true</item>
///   <item>ConvertBack: true → Enum.Parse(targetType, parameter); false → UnsetValue</item>
/// </list>
/// </summary>
[ValueConversion(typeof(Enum), typeof(bool))]
public sealed class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return false;
        return (int)value == (int)Enum.Parse(value.GetType(), parameter.ToString()!);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not true || parameter is null) return DependencyProperty.UnsetValue;
        return Enum.Parse(targetType, parameter.ToString()!);
    }
}
