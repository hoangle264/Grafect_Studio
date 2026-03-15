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
            {
                RaisePropertyChanged(nameof(LinePath));
                RaisePropertyChanged(nameof(ArrowGeometry));
            }
        }
    }

    /// <summary>Y coordinate of the arrow origin point.</summary>
    public double StartY
    {
        get => _startY;
        set
        {
            if (SetProperty(ref _startY, value))
            {
                RaisePropertyChanged(nameof(LinePath));
                RaisePropertyChanged(nameof(ArrowGeometry));
            }
        }
    }

    /// <summary>X coordinate of the arrowhead tip.</summary>
    public double EndX
    {
        get => _endX;
        set
        {
            if (SetProperty(ref _endX, value))
            {
                RaisePropertyChanged(nameof(LinePath));
                RaisePropertyChanged(nameof(ArrowGeometry));
            }
        }
    }

    /// <summary>Y coordinate of the arrowhead tip.</summary>
    public double EndY
    {
        get => _endY;
        set
        {
            if (SetProperty(ref _endY, value))
            {
                RaisePropertyChanged(nameof(LinePath));
                RaisePropertyChanged(nameof(ArrowGeometry));
            }
        }
    }

    /// <summary>WPF path geometry string for the orthogonal (right-angle) link body.</summary>
    public string LinePath
    {
        get
        {
            double midY = (_startY + _endY) / 2.0;
            return $"M {_startX},{_startY} L {_startX},{midY} L {_endX},{midY} L {_endX},{_endY}";
        }
    }

    /// <summary>WPF path geometry string for the arrowhead at (EndX, EndY), pointing up or down.</summary>
    public string ArrowGeometry
    {
        get
        {
            double lx, ly, rx, ry;

            if (_endY >= _startY)
            {
                // Arrow pointing down (↓)
                lx = _endX - 5; ly = _endY - 10;
                rx = _endX + 5; ry = _endY - 10;
            }
            else
            {
                // Arrow pointing up (↑)
                lx = _endX - 5; ly = _endY + 10;
                rx = _endX + 5; ry = _endY + 10;
            }

            return $"M {_endX},{_endY} L {lx},{ly} L {rx},{ry} Z";
        }
    }
}
