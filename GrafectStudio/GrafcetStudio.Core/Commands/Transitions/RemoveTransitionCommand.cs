using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands.Transitions;

/// <summary>Removes a transition and its associated links from the document.</summary>
public class RemoveTransitionCommand : IGrafcetCommand
{
    private readonly int _transitionId;

    private GrafcetTransition? _removed;
    private List<GrafcetLink> _removedLinks = [];

    public string Description => $"Remove transition {_transitionId}";

    public RemoveTransitionCommand(int transitionId)
    {
        _transitionId = transitionId;
    }

    public void Execute(GrafcetDocument document)
    {
        _removed = document.GetTransition(_transitionId)
            ?? throw new InvalidOperationException($"Transition {_transitionId} does not exist.");

        // Collect links that reference this transition
        _removedLinks = document.Links
            .Where(l => (!l.IsStepToTransition && l.SourceId == _transitionId) ||
                        (l.IsStepToTransition  && l.TargetId == _transitionId))
            .ToList();

        document.Links.RemoveAll(l =>
            (!l.IsStepToTransition && l.SourceId == _transitionId) ||
            (l.IsStepToTransition  && l.TargetId == _transitionId));

        document.Transitions.RemoveAll(t => t.Id == _transitionId);
    }

    public void Undo(GrafcetDocument document)
    {
        if (_removed is null) return;

        document.Transitions.Add(_removed);
        document.Links.AddRange(_removedLinks);
    }
}
