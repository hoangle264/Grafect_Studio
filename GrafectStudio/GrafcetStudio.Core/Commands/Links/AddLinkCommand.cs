using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands.Links;

/// <summary>Adds a directed link between a step and a transition (or vice versa).</summary>
public class AddLinkCommand : IGrafcetCommand
{
    private readonly GrafcetLink _link;

    public AddLinkCommand(GrafcetLink link) => _link = link;

    public string Description => $"Add link {_link.SourceId} → {_link.TargetId}";

    public void Execute(GrafcetDocument doc) => doc.Links.Add(_link);

    public void Undo(GrafcetDocument doc)    => doc.Links.Remove(_link);
}
