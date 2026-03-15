using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands.Steps;

/// <summary>Removes a step and optionally its orphaned transitions from the document.</summary>
public class RemoveStepCommand : IGrafcetCommand
{
    private readonly int _stepId;
    private readonly bool _removeOrphanTransitions;

    // Snapshots restored on Undo
    private GrafcetStep? _removedStep;
    private List<GrafcetTransition> _removedTransitions = [];
    private List<GrafcetLink> _removedLinks = [];

    public string Description => $"Remove step {_stepId}";

    public RemoveStepCommand(int stepId, bool removeOrphanTransitions)
    {
        _stepId = stepId;
        _removeOrphanTransitions = removeOrphanTransitions;
    }

    public void Execute(GrafcetDocument document)
    {
        var step = document.GetStep(_stepId)
            ?? throw new InvalidOperationException($"Step {_stepId} does not exist.");

        if (step.IsInitial)
            throw new InvalidOperationException("The initial step cannot be removed.");

        _removedStep = step;

        // Collect orphan transitions before removing
        _removedTransitions = document.Transitions
            .Where(t => t.FromStepId == _stepId || t.ToStepId == _stepId)
            .ToList();

        if (_removeOrphanTransitions)
            document.Transitions.RemoveAll(t => t.FromStepId == _stepId || t.ToStepId == _stepId);

        // Collect and remove links that reference this step
        _removedLinks = document.Links
            .Where(l => (l.IsStepToTransition  && l.SourceId == _stepId) ||
                        (!l.IsStepToTransition && l.TargetId == _stepId))
            .ToList();

        document.Links.RemoveAll(l =>
            (l.IsStepToTransition  && l.SourceId == _stepId) ||
            (!l.IsStepToTransition && l.TargetId == _stepId));

        document.Steps.RemoveAll(s => s.Id == _stepId);
    }

    public void Undo(GrafcetDocument document)
    {
        if (_removedStep is null) return;

        document.Steps.Add(_removedStep);

        if (_removeOrphanTransitions)
            document.Transitions.AddRange(_removedTransitions);

        document.Links.AddRange(_removedLinks);
    }
}
