using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands.Links;

/// <summary>Removes a directed link from the document with full undo support.</summary>
public class RemoveLinkCommand : IGrafcetCommand
{
    private readonly int  _sourceId;
    private readonly int  _targetId;
    private readonly bool _isStepToTransition;

    private GrafcetLink? _snapshot;

    public RemoveLinkCommand(int sourceId, int targetId, bool isStepToTransition)
    {
        _sourceId           = sourceId;
        _targetId           = targetId;
        _isStepToTransition = isStepToTransition;
    }

    public string Description => $"Remove link {_sourceId} → {_targetId}";

    public void Execute(GrafcetDocument doc)
    {
        var link = doc.Links.FirstOrDefault(l =>
            l.SourceId           == _sourceId           &&
            l.TargetId           == _targetId           &&
            l.IsStepToTransition == _isStepToTransition);

        if (link is null)
            throw new InvalidOperationException(
                $"Link {_sourceId} → {_targetId} (IsStepToTransition={_isStepToTransition}) not found.");

        _snapshot = link;
        doc.Links.Remove(link);

        // Clear the corresponding step reference on the transition
        if (_isStepToTransition)
        {
            var transition = doc.Transitions.FirstOrDefault(t => t.Id == _targetId);
            if (transition is not null)
                transition.FromStepId = 0;
        }
        else
        {
            var transition = doc.Transitions.FirstOrDefault(t => t.Id == _sourceId);
            if (transition is not null)
                transition.ToStepId = 0;
        }
    }

    public void Undo(GrafcetDocument doc)
    {
        if (_snapshot is null) return;

        doc.Links.Add(_snapshot);

        // Restore the step reference on the transition
        if (_snapshot.IsStepToTransition)
        {
            var transition = doc.Transitions.FirstOrDefault(t => t.Id == _snapshot.TargetId);
            if (transition is not null)
                transition.FromStepId = _snapshot.SourceId;
        }
        else
        {
            var transition = doc.Transitions.FirstOrDefault(t => t.Id == _snapshot.SourceId);
            if (transition is not null)
                transition.ToStepId = _snapshot.TargetId;
        }
    }
}
