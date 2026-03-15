using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands.Transitions;

/// <summary>Partially updates one or more fields of an existing transition (null fields are left unchanged).</summary>
public class ModifyTransitionCommand : IGrafcetCommand
{
    private readonly int _transitionId;
    private readonly string? _condition;
    private readonly int? _fromStepId;
    private readonly int? _toStepId;

    private record Snapshot(string Condition, int FromStepId, int ToStepId);
    private Snapshot? _snapshot;

    public string Description => $"Modify transition {_transitionId}";

    public ModifyTransitionCommand(int transitionId,
        string? condition  = null,
        int?    fromStepId = null,
        int?    toStepId   = null)
    {
        _transitionId = transitionId;
        _condition    = condition;
        _fromStepId   = fromStepId;
        _toStepId     = toStepId;
    }

    public void Execute(GrafcetDocument document)
    {
        var transition = document.GetTransition(_transitionId)
            ?? throw new InvalidOperationException($"Transition {_transitionId} does not exist.");

        _snapshot = new Snapshot(transition.Condition, transition.FromStepId, transition.ToStepId);

        if (_condition  is not null) transition.Condition  = _condition;
        if (_fromStepId is not null) transition.FromStepId = _fromStepId.Value;
        if (_toStepId   is not null) transition.ToStepId   = _toStepId.Value;
    }

    public void Undo(GrafcetDocument document)
    {
        if (_snapshot is null) return;

        var transition = document.GetTransition(_transitionId);
        if (transition is null) return;

        transition.Condition  = _snapshot.Condition;
        transition.FromStepId = _snapshot.FromStepId;
        transition.ToStepId   = _snapshot.ToStepId;
    }
}
