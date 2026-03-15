using GrafcetStudio.Core.Models.Variables;

namespace GrafcetStudio.Core.Models.Document;

/// <summary>Root aggregate containing the complete GRAFCET diagram and variable table.</summary>
public class GrafcetDocument
{
    /// <summary>Display name of this document (used as file title).</summary>
    public string Name { get; set; } = "Untitled";

    /// <summary>All declared PLC variables for this document.</summary>
    public VariableTable VariableTable { get; set; } = new();

    /// <summary>All steps in the GRAFCET diagram.</summary>
    public List<GrafcetStep> Steps { get; set; } = new();

    /// <summary>All transitions in the GRAFCET diagram.</summary>
    public List<GrafcetTransition> Transitions { get; set; } = new();

    /// <summary>All parallel and selective branch structures.</summary>
    public List<GrafcetBranch> Branches { get; set; } = new();

    /// <summary>All directed connections between steps and transitions.</summary>
    public List<GrafcetLink> Links { get; set; } = new();

    // ── Lookup helpers ────────────────────────────────────────────────────────

    /// <summary>Returns the step with the given <paramref name="id"/>, or null if not found.</summary>
    public GrafcetStep? GetStep(int id)
        => Steps.FirstOrDefault(s => s.Id == id);

    /// <summary>Returns the transition with the given <paramref name="id"/>, or null if not found.</summary>
    public GrafcetTransition? GetTransition(int id)
        => Transitions.FirstOrDefault(t => t.Id == id);

    // ── ID generation ─────────────────────────────────────────────────────────

    /// <summary>Returns the next available step ID (max existing + 1, or 1 when empty).</summary>
    public int NextStepId()
        => Steps.Count == 0 ? 1 : Steps.Max(s => s.Id) + 1;

    /// <summary>Returns the next available transition ID (max existing + 1, or 1 when empty).</summary>
    public int NextTransitionId()
        => Transitions.Count == 0 ? 1 : Transitions.Max(t => t.Id) + 1;

    // ── Validation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates the document against GRAFCET structural rules.
    /// Returns an empty list when the document is valid.
    /// </summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        // IEC 60848: exactly one initial step required
        int initialCount = Steps.Count(s => s.IsInitial);
        if (initialCount == 0)
            errors.Add("Document must have exactly one initial step.");
        else if (initialCount > 1)
            errors.Add($"Document has {initialCount} initial steps; exactly one is required.");

        var stepIds = Steps.Select(s => s.Id).ToHashSet();

        // Each transition must reference existing steps
        foreach (var t in Transitions)
        {
            if (!stepIds.Contains(t.FromStepId))
                errors.Add($"Transition {t.Id}: FromStepId {t.FromStepId} references a non-existent step.");
            if (!stepIds.Contains(t.ToStepId))
                errors.Add($"Transition {t.Id}: ToStepId {t.ToStepId} references a non-existent step.");
        }

        // Each branch must reference existing steps
        foreach (var b in Branches)
        {
            foreach (int sid in b.StepIds)
                if (!stepIds.Contains(sid))
                    errors.Add($"Branch {b.Id}: StepId {sid} references a non-existent step.");

            if (!stepIds.Contains(b.MergeStepId))
                errors.Add($"Branch {b.Id}: MergeStepId {b.MergeStepId} references a non-existent step.");
        }

        return errors;
    }
}
