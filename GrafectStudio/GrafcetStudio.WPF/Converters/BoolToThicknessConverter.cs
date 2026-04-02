using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GrafcetStudio.WPF.Converters;

/// <summary>
/// Converts a <see langword="bool"/> to a stroke thickness <see langword="double"/>.
/// Used to render parallel branches (IEC 60848 double bar) thicker than selective ones.
/// <list type="bullet">
///   <item><see langword="true"/>  (Parallel)  → <see cref="TrueValue"/>  (default 3.0)</item>
///   <item><see langword="false"/> (Selective) → <see cref="FalseValue"/> (default 1.5)</item>
/// </list>
/// </summary>
[ValueConversion(typeof(bool), typeof(double))]
public sealed class BoolToThicknessConverter : IValueConverter
{
    public double TrueValue  { get; set; } = 3.0;
    public double FalseValue { get; set; } = 1.5;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? TrueValue : FalseValue;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}
