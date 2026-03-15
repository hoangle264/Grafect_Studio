namespace GrafcetStudio.WPF.ViewModels.Canvas;

/// <summary>Data carrier for a toolbox element dropped onto the GRAFCET canvas.</summary>
/// <param name="ElementType">One of: Step | InitialStep | Transition | Link | ParallelBranch | SelectiveBranch</param>
/// <param name="X">Canvas X coordinate of the drop point (unscaled).</param>
/// <param name="Y">Canvas Y coordinate of the drop point (unscaled).</param>
public record DropPayload(string ElementType, double X, double Y);
