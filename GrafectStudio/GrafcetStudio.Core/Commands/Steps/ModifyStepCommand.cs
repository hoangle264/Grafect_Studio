using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands.Steps;

/// <summary>Partially updates one or more fields of an existing step (null fields are left unchanged).</summary>
public class ModifyStepCommand : IGrafcetCommand
{
    private readonly int _stepId;
    private readonly string? _name;
    private readonly bool? _isInitial;
    private readonly double? _x;
    private readonly double? _y;
    private readonly List<GrafcetAction>? _actions;

    private record Snapshot(string Name, bool IsInitial, double X, double Y, List<GrafcetAction> Actions);
    private Snapshot? _snapshot;

    public string Description => $"Modify step {_stepId}";

    public ModifyStepCommand(int stepId,
        string? name     = null,
        bool?   isInitial = null,
        double? x        = null,
        double? y        = null,
        List<GrafcetAction>? actions = null)
    {
        _stepId    = stepId;
        _name      = name;
        _isInitial = isInitial;
        _x         = x;
        _y         = y;
        _actions   = actions;
    }

    public void Execute(GrafcetDocument document)
    {
        var step = document.GetStep(_stepId)
            ?? throw new InvalidOperationException($"Step {_stepId} does not exist.");

        // Deep-copy actions list for snapshot
        _snapshot = new Snapshot(
            step.Name,
            step.IsInitial,
            step.X,
            step.Y,
            step.Actions.Select(a => new GrafcetAction
            {
                Qualifier = a.Qualifier,
                Variable  = a.Variable,
                Parameter = a.Parameter
            }).ToList());

        if (_name      is not null) step.Name      = _name;
        if (_isInitial is not null) step.IsInitial = _isInitial.Value;
        if (_x         is not null) step.X         = _x.Value;
        if (_y         is not null) step.Y         = _y.Value;
        if (_actions   is not null) step.Actions   = _actions;
    }

    public void Undo(GrafcetDocument document)
    {
        if (_snapshot is null) return;

        var step = document.GetStep(_stepId);
        if (step is null) return;

        step.Name      = _snapshot.Name;
        step.IsInitial = _snapshot.IsInitial;
        step.X         = _snapshot.X;
        step.Y         = _snapshot.Y;
        step.Actions   = _snapshot.Actions;
    }
}
