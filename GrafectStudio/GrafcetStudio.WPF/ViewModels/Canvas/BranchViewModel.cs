using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Document;
using Prism.Mvvm;

namespace GrafcetStudio.WPF.ViewModels.Canvas;

/// <summary>ViewModel representing a GRAFCET branch (parallel or selective) on the diagram canvas.</summary>
public class BranchViewModel : BindableBase
{
    private int        _id;
    private BranchType _type;
    private double     _x;
    private double     _y;
    private double     _width;
    private double     _mergeX;
    private double     _mergeY;

    public BranchViewModel(GrafcetBranch branch, double x, double y, double width, double mergeY)
    {
        _id     = branch.Id;
        _type   = branch.Type;
        _x      = x;
        _y      = y;
        _width  = width;
        _mergeX = x;
        _mergeY = mergeY;
    }

    /// <summary>Unique branch identifier matching the model.</summary>
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>Parallel (AND) or Selective (OR) divergence type.</summary>
    public BranchType Type
    {
        get => _type;
        set
        {
            if (SetProperty(ref _type, value))
                RaisePropertyChanged(nameof(IsParallel));
        }
    }

    /// <summary>Canvas X position of the split bar in device-independent units.</summary>
    public double X
    {
        get => _x;
        set
        {
            if (SetProperty(ref _x, value))
                RaisePropertyChanged(nameof(X2));
        }
    }

    /// <summary>Canvas Y position of the split bar in device-independent units.</summary>
    public double Y
    {
        get => _y;
        set
        {
            if (SetProperty(ref _y, value))
                RaisePropertyChanged(nameof(Y2Offset));
        }
    }

    /// <summary>Y position of the secondary split bar for parallel branches (Y + 4).</summary>
    public double Y2Offset => Y + 4;

    /// <summary>Horizontal width of the split/merge bars in device-independent units.</summary>
    public double Width
    {
        get => _width;
        set
        {
            if (SetProperty(ref _width, value))
                RaisePropertyChanged(nameof(X2));
        }
    }

    /// <summary>Right edge of the split/merge bars (X + Width).</summary>
    public double X2 => X + Width;

    /// <summary>Canvas X position of the merge bar in device-independent units.</summary>
    public double MergeX
    {
        get => _mergeX;
        set => SetProperty(ref _mergeX, value);
    }

    /// <summary>Canvas Y position of the merge bar in device-independent units.</summary>
    public double MergeY
    {
        get => _mergeY;
        set
        {
            if (SetProperty(ref _mergeY, value))
                RaisePropertyChanged(nameof(MergeY2Offset));
        }
    }

    /// <summary>Y position of the secondary merge bar for parallel branches (MergeY + 4).</summary>
    public double MergeY2Offset => MergeY + 4;

    /// <summary>True when this branch is a parallel (AND) divergence — rendered with a double bar per IEC 60848.</summary>
    public bool IsParallel => Type == BranchType.Parallel;
}
