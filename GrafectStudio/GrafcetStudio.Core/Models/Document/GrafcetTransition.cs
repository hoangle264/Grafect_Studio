namespace GrafcetStudio.Core.Models.Document;

/// <summary>Represents a transition between two GRAFCET steps.</summary>
public class GrafcetTransition
{
    /// <summary>Unique numeric identifier for this transition within the document.</summary>
    public int Id { get; set; }

    /// <summary>
    /// Boolean expression evaluated each scan cycle. Uses logical variable names;
    /// resolved to hardware addresses by <c>VariableResolver</c> before code generation.
    /// Defaults to <c>TRUE</c> (unconditional).
    /// </summary>
    public string Condition { get; set; } = "TRUE";

    /// <summary>Id of the preceding step that must be active for this transition to fire.</summary>
    public int FromStepId { get; set; }

    /// <summary>Id of the successor step that becomes active when this transition fires.</summary>
    public int ToStepId { get; set; }
}
