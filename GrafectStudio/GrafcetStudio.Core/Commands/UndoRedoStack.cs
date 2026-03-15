using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands;

/// <summary>
/// Manages undo and redo history for a <see cref="GrafcetDocument"/>.
/// Pushing a new command clears the redo stack.
/// </summary>
public class UndoRedoStack
{
    private readonly Stack<IGrafcetCommand> _undoStack = new();
    private readonly Stack<IGrafcetCommand> _redoStack = new();

    /// <summary>Raised whenever <see cref="CanUndo"/> or <see cref="CanRedo"/> may have changed.</summary>
    public event EventHandler? StateChanged;

    /// <summary>True when at least one command can be undone.</summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>True when at least one command can be redone.</summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>Description of the next command to be undone, or null.</summary>
    public string? UndoDescription => CanUndo ? _undoStack.Peek().Description : null;

    /// <summary>Description of the next command to be redone, or null.</summary>
    public string? RedoDescription => CanRedo ? _redoStack.Peek().Description : null;

    // ── Stack operations ──────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="command"/>, pushes it onto the undo stack,
    /// and clears the redo stack.
    /// </summary>
    public void Push(IGrafcetCommand command, GrafcetDocument document)
    {
        command.Execute(document);
        _undoStack.Push(command);
        _redoStack.Clear();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Undoes the most recent command and moves it to the redo stack.</summary>
    public void Undo(GrafcetDocument document)
    {
        if (!CanUndo) return;

        var command = _undoStack.Pop();
        command.Undo(document);
        _redoStack.Push(command);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Re-executes the most recently undone command and moves it back to the undo stack.</summary>
    public void Redo(GrafcetDocument document)
    {
        if (!CanRedo) return;

        var command = _redoStack.Pop();
        command.Execute(document);
        _undoStack.Push(command);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Clears both undo and redo history (e.g. after saving or loading a document).</summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
