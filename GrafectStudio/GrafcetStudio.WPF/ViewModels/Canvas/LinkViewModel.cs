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
    private bool   _isSelected;

    public LinkViewModel(
        double startX, double startY, double endX, double endY,
        int sourceId, int targetId, bool isStepToTransition)
    {
        _startX = startX;
        _startY = startY;
        _endX   = endX;
        _endY   = endY;

        SourceId           = sourceId;
        TargetId           = targetId;
        IsStepToTransition = isStepToTransition;
    }

    // ── Identity (immutable) ──────────────────────────────────────────────────

    /// <summary>Id of the source element this link originates from.</summary>
    public int SourceId { get; }

    /// <summary>Id of the target element this link points to.</summary>
    public int TargetId { get; }

    /// <summary>True when the link goes Step → Transition; false for Transition → Step.</summary>
    public bool IsStepToTransition { get; }

    // ── Mutable geometry ──────────────────────────────────────────────────────

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

    /// <summary>True when this link is the currently selected canvas element.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    // ── Computed geometry ─────────────────────────────────────────────────────

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
