using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands.Links;

/// <summary>Adds a directed link between a step and a transition (or vice versa).</summary>
public class AddLinkCommand : IGrafcetCommand
{
    private readonly int  _sourceId;
    private readonly int  _targetId;
    private readonly bool _isStepToTransition;

    public AddLinkCommand(int sourceId, int targetId, bool isStepToTransition)
    {
        _sourceId           = sourceId;
        _targetId           = targetId;
        _isStepToTransition = isStepToTransition;
    }

    public string Description => $"Add link {_sourceId} → {_targetId}";

    public void Execute(GrafcetDocument doc)
    {
        bool duplicate = doc.Links.Any(l =>
            l.SourceId == _sourceId &&
            l.TargetId == _targetId &&
            l.IsStepToTransition == _isStepToTransition);

        if (duplicate)
            throw new InvalidOperationException(
                $"Link {_sourceId} → {_targetId} already exists.");

        doc.Links.Add(new GrafcetLink
        {
            SourceId           = _sourceId,
            TargetId           = _targetId,
            IsStepToTransition = _isStepToTransition
        });

        SyncTransition(doc, set: true);
    }

    public void Undo(GrafcetDocument doc)
    {
        doc.Links.RemoveAll(l =>
            l.SourceId == _sourceId &&
            l.TargetId == _targetId &&
            l.IsStepToTransition == _isStepToTransition);

        SyncTransition(doc, set: false);
    }

    // Updates FromStepId / ToStepId on the transition to reflect the link state.
    private void SyncTransition(GrafcetDocument doc, bool set)
    {
        if (_isStepToTransition)
        {
            // _sourceId = stepId, _targetId = transitionId
            var transition = doc.Transitions.FirstOrDefault(t => t.Id == _targetId);
            if (transition is not null)
                transition.FromStepId = set ? _sourceId : 0;
        }
        else
        {
            // _sourceId = transitionId, _targetId = stepId
            var transition = doc.Transitions.FirstOrDefault(t => t.Id == _sourceId);
            if (transition is not null)
                transition.ToStepId = set ? _targetId : 0;
        }
    }
}
