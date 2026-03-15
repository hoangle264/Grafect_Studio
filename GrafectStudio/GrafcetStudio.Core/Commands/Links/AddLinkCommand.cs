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
    }

    public void Undo(GrafcetDocument doc) =>
        doc.Links.RemoveAll(l =>
            l.SourceId == _sourceId &&
            l.TargetId == _targetId &&
            l.IsStepToTransition == _isStepToTransition);
}
