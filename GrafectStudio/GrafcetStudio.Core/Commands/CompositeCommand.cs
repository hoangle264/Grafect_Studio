using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands;

/// <summary>
/// Composes multiple <see cref="IGrafcetCommand"/> instances into a single undo entry.
/// Execute runs commands in forward order; Undo runs them in reverse.
/// </summary>
public class CompositeCommand : IGrafcetCommand
{
    private readonly List<IGrafcetCommand> _commands;

    /// <inheritdoc/>
    public string Description { get; }

    /// <param name="commands">Ordered list of commands to execute as a unit.</param>
    /// <param name="description">Description shown in Undo/Redo menu.</param>
    public CompositeCommand(IEnumerable<IGrafcetCommand> commands, string description)
    {
        _commands = commands.ToList();
        Description = description;
    }

    /// <inheritdoc/>
    public void Execute(GrafcetDocument document)
    {
        foreach (var command in _commands)
            command.Execute(document);
    }

    /// <inheritdoc/>
    /// <remarks>Commands are undone in reverse order to correctly restore state.</remarks>
    public void Undo(GrafcetDocument document)
    {
        foreach (var command in Enumerable.Reverse(_commands))
            command.Undo(document);
    }
}
