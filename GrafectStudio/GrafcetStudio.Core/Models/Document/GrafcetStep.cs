namespace GrafcetStudio.Core.Models.Document;

/// <summary>Represents a GRAFCET step node in the diagram.</summary>
public class GrafcetStep
{
    /// <summary>Unique numeric identifier for this step within the document.</summary>
    public int Id { get; set; }

    /// <summary>Human-readable label displayed inside the step rectangle.</summary>
    public string Name { get; set; } = "";

    /// <summary>True if this is the initial step (double-bordered rectangle per IEC 60848).</summary>
    public bool IsInitial { get; set; }

    /// <summary>List of actions executed while this step is active.</summary>
    public List<GrafcetAction> Actions { get; set; } = new();

    /// <summary>Horizontal position on the canvas in device-independent units.</summary>
    public double X { get; set; }

    /// <summary>Vertical position on the canvas in device-independent units.</summary>
    public double Y { get; set; }

    /// <summary>Id of the GrafcetBranch this step belongs to. Null when not part of any branch.</summary>
    public int? BranchId { get; set; }

    /// <summary>Role of this step within its branch structure (Split, Member, Merge, or None).</summary>
    public BranchRole BranchRole { get; set; }
}
