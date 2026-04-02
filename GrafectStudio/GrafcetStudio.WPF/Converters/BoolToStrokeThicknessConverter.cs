using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GrafcetStudio.WPF.Converters;

/// <summary>
/// Converts a <see langword="bool"/> to a stroke-thickness <see langword="double"/>.
/// Used to render parallel branch bars (IEC 60848 double-bar) visually thicker than selective ones.
/// <list type="bullet">
///   <item><see langword="true"/>  (Parallel)  → 4.0</item>
///   <item><see langword="false"/> (Selective) → 2.0</item>
/// </list>
/// </summary>
[ValueConversion(typeof(bool), typeof(double))]
public sealed class BoolToStrokeThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? 4.0 : 2.0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}
