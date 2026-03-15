namespace GrafcetStudio.WPF.ViewModels.Canvas;

/// <summary>Carries state for an in-progress drag-to-create-link gesture.</summary>
public sealed class DragLinkState
{
    public object SourceElement { get; init; } = null!;
    public double StartX        { get; init; }
    public double StartY        { get; init; }
    public double CurrentX      { get; set;  }
    public double CurrentY      { get; set;  }
    public bool   IsValidTarget { get; set;  }
}
