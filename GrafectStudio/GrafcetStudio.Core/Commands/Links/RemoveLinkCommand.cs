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
    }

    public void Undo(GrafcetDocument doc)
    {
        if (_snapshot is not null)
            doc.Links.Add(_snapshot);
    }
}
