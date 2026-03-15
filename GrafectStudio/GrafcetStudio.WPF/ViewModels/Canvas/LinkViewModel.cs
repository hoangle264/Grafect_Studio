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
        set => SetProperty(ref _startX, value);
    }

    /// <summary>Y coordinate of the arrow origin point.</summary>
    public double StartY
    {
        get => _startY;
        set => SetProperty(ref _startY, value);
    }

    /// <summary>X coordinate of the arrowhead tip.</summary>
    public double EndX
    {
        get => _endX;
        set => SetProperty(ref _endX, value);
    }

    /// <summary>Y coordinate of the arrowhead tip.</summary>
    public double EndY
    {
        get => _endY;
        set => SetProperty(ref _endY, value);
    }
}
