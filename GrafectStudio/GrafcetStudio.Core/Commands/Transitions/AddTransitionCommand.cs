using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands.Transitions;

/// <summary>Adds a new transition to the document.</summary>
public class AddTransitionCommand : IGrafcetCommand
{
    private readonly GrafcetTransition _transition;

    public string Description => $"Add transition {_transition.Id}";

    public AddTransitionCommand(GrafcetTransition transition)
    {
        _transition = transition;
    }

    public void Execute(GrafcetDocument document)
    {
        document.Transitions.Add(_transition);
    }

    public void Undo(GrafcetDocument document)
    {
        document.Transitions.RemoveAll(t => t.Id == _transition.Id);

        // Safety: remove orphan links that reference this transition
        document.Links.RemoveAll(l =>
            (!l.IsStepToTransition && l.SourceId == _transition.Id) ||
            (l.IsStepToTransition  && l.TargetId == _transition.Id));
    }
}
