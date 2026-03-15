using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands.Steps;

/// <summary>Adds a new step to the document.</summary>
public class AddStepCommand : IGrafcetCommand
{
    private readonly GrafcetStep _step;

    public string Description => $"Add step '{_step.Name}'";

    public AddStepCommand(GrafcetStep step)
    {
        _step = step;
    }

    public void Execute(GrafcetDocument document)
    {
        document.Steps.Add(_step);
    }

    public void Undo(GrafcetDocument document)
    {
        document.Steps.RemoveAll(s => s.Id == _step.Id);

        // Safety: remove any orphan links that reference this step
        document.Links.RemoveAll(l =>
            (l.IsStepToTransition  && l.SourceId == _step.Id) ||
            (!l.IsStepToTransition && l.TargetId == _step.Id));
    }
}
