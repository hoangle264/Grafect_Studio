namespace GrafcetStudio.Core.Models.Document;

/// <summary>Represents a directed connection between a step and a transition in the GRAFCET diagram.</summary>
public class GrafcetLink
{
    /// <summary>Id of the source element (step or transition) this link originates from.</summary>
    public int SourceId { get; set; }

    /// <summary>Id of the target element (transition or step) this link points to.</summary>
    public int TargetId { get; set; }

    /// <summary>
    /// True when the link goes from a <see cref="GrafcetStep"/> to a <see cref="GrafcetTransition"/>;
    /// false when it goes from a <see cref="GrafcetTransition"/> to a <see cref="GrafcetStep"/>.
    /// </summary>
    public bool IsStepToTransition { get; set; }
}
