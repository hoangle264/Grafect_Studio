using GrafcetStudio.Core.Models;

namespace GrafcetStudio.Core.Models.Document;

/// <summary>Represents a parallel or selective divergence/convergence structure in the GRAFCET diagram.</summary>
public class GrafcetBranch
{
    /// <summary>Unique numeric identifier for this branch within the document.</summary>
    public int Id { get; set; }

    /// <summary>Determines whether this is a parallel (AND) or selective (OR) branch.</summary>
    public BranchType Type { get; set; }

    /// <summary>Ordered list of step IDs that form the diverging paths of this branch.</summary>
    public List<int> StepIds { get; set; } = new();

    /// <summary>Id of the step where all diverging paths reconverge.</summary>
    public int MergeStepId { get; set; }
}
