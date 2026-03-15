using Prism.Mvvm;

namespace GrafcetStudio.WPF.ViewModels.Canvas;

/// <summary>
/// ViewModel representing a directed arrow connecting a step to a transition
/// or a transition to a step on the GRAFCET canvas.
/// </summary>
public class LinkViewModel : BindableBase
{
    private double _startX;
    private double _startY;
    private double _endX;
    private double _endY;

    public LinkViewModel(double startX, double startY, double endX, double endY)
    {
        _startX = startX;
        _startY = startY;
        _endX   = endX;
        _endY   = endY;
    }

    /// <summary>X coordinate of the arrow origin point.</summary>
    public double StartX
    {
        get => _startX;
        set
        {
            if (SetProperty(ref _startX, value))
                RaisePropertyChanged(nameof(ArrowGeometry));
        }
    }

    /// <summary>Y coordinate of the arrow origin point.</summary>
    public double StartY
    {
        get => _startY;
        set
        {
            if (SetProperty(ref _startY, value))
                RaisePropertyChanged(nameof(ArrowGeometry));
        }
    }

    /// <summary>X coordinate of the arrowhead tip.</summary>
    public double EndX
    {
        get => _endX;
        set
        {
            if (SetProperty(ref _endX, value))
                RaisePropertyChanged(nameof(ArrowGeometry));
        }
    }

    /// <summary>Y coordinate of the arrowhead tip.</summary>
    public double EndY
    {
        get => _endY;
        set
        {
            if (SetProperty(ref _endY, value))
                RaisePropertyChanged(nameof(ArrowGeometry));
        }
    }

    /// <summary>WPF path geometry string for the arrowhead (readonly, computed from end coordinates).</summary>
    public string ArrowGeometry
    {
        get
        {
            const double arrowLength = 10.0;
            const double arrowWidth = 5.0;

            double angle = Math.Atan2(_endY - _startY, _endX - _startX);

            double lx = _endX - arrowLength * Math.Cos(angle) + arrowWidth * Math.Sin(angle);
            double ly = _endY - arrowLength * Math.Sin(angle) - arrowWidth * Math.Cos(angle);

            double rx = _endX - arrowLength * Math.Cos(angle) - arrowWidth * Math.Sin(angle);
            double ry = _endY - arrowLength * Math.Sin(angle) + arrowWidth * Math.Cos(angle);

            return $"M {_endX},{_endY} L {lx},{ly} L {rx},{ry} Z";
        }
    }
}
