using GrafcetStudio.Core.Models.Document;
using Prism.Mvvm;

namespace GrafcetStudio.WPF.ViewModels.Canvas;

/// <summary>ViewModel representing a GRAFCET step node on the diagram canvas.</summary>
public class StepViewModel : BindableBase
{
    private int _id;
    private string _name = "";
    private bool _isInitial;
    private double _x;
    private double _y;
    private bool  _isSelected;
    private int?  _branchId;

    public StepViewModel(GrafcetStep step)
    {
        _id       = step.Id;
        _name     = step.Name;
        _isInitial = step.IsInitial;
        _x        = step.X;
        _y        = step.Y;
        _branchId = step.BranchId;
    }

    /// <summary>Unique step identifier matching the model.</summary>
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>Display label shown inside the step rectangle.</summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// True for the initial step — rendered with a double border per IEC 60848.
    /// </summary>
    public bool IsInitial
    {
        get => _isInitial;
        set => SetProperty(ref _isInitial, value);
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

    /// <summary>True when this step is the currently selected canvas element.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>Display text showing step number (e.g., "Step 1", "Step 2").</summary>
    public string StepDisplayText => $"Step {_id}";

    /// <summary>Id of the branch this step belongs to; null when not part of any branch.</summary>
    public int? BranchId
    {
        get => _branchId;
        set => SetProperty(ref _branchId, value);
    }
}
