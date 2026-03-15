namespace GrafcetStudio.WPF.ViewModels.Canvas;

/// <summary>Data carrier for moving a canvas element by a delta offset.</summary>
/// <param name="Element">The StepViewModel or TransitionViewModel being moved.</param>
/// <param name="DeltaX">Horizontal offset in canvas device-independent units.</param>
/// <param name="DeltaY">Vertical offset in canvas device-independent units.</param>
public record MovePayload(object Element, double DeltaX, double DeltaY);
