using GrafcetStudio.Core.Models;

namespace GrafcetStudio.Core.Models.Document;

/// <summary>Represents a single action associated with a GRAFCET step.</summary>
public class GrafcetAction
{
    /// <summary>IEC 60848 action qualifier controlling activation behaviour (N, S, R, P, L, D).</summary>
    public ActionQualifier Qualifier { get; set; }

    /// <summary>Logical variable name or hardware address that this action drives.</summary>
    public string Variable { get; set; } = "";

    /// <summary>Optional parameter for time-limited qualifiers L and D (e.g. "T#2s").</summary>
    public string? Parameter { get; set; }
}
