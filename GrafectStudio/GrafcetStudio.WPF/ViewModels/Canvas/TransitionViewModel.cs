using GrafcetStudio.Core.Models.Document;
using Prism.Mvvm;

namespace GrafcetStudio.WPF.ViewModels.Canvas;

/// <summary>ViewModel representing a GRAFCET transition bar on the diagram canvas.</summary>
public class TransitionViewModel : BindableBase
{
    private int _id;
    private string _condition = "";
    private double _x;
    private double _y;
    private bool _isSelected;

    /// <param name="transition">Source model object.</param>
    /// <param name="x">Computed canvas X position (left edge).</param>
    /// <param name="y">Computed canvas Y position (top edge).</param>
    public TransitionViewModel(GrafcetTransition transition, double x, double y)
    {
        _id        = transition.Id;
        _condition = transition.Condition;
        _x         = x;
        _y         = y;
    }

    /// <summary>Unique transition identifier matching the model.</summary>
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>Boolean condition expression displayed below the transition bar.</summary>
    public string Condition
    {
        get => _condition;
        set => SetProperty(ref _condition, value);
    }

    /// <summary>Canvas X position (left edge) in device-independent units.</summary>
    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    /// <summary>Canvas Y position (top edge) in device-independent units.</summary>
    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    /// <summary>True when this transition is the currently selected canvas element.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
