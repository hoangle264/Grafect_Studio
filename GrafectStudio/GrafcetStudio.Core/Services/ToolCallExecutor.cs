using GrafcetStudio.Core.Commands;
using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Services;

/// <summary>
/// Validates a <see cref="ToolCallBatch"/>, builds a single <see cref="CompositeCommand"/>
/// from all calls, and pushes it onto the <see cref="UndoRedoStack"/> as one undo entry.
/// </summary>
public class ToolCallExecutor
{
    private static readonly HashSet<string> _validTools =
    [
        "AddStep", "RemoveStep", "ModifyStep",
        "AddTransition", "RemoveTransition", "ModifyTransition",
        "AddVariable", "RemoveVariable", "ModifyVariable",
        "RemoveLink"
    ];

    private readonly GrafcetDocument _document;
    private readonly UndoRedoStack   _undoRedoStack;
    private readonly ToolCallFactory _factory;

    public ToolCallExecutor(
        GrafcetDocument document,
        UndoRedoStack   undoRedoStack,
        ToolCallFactory factory)
    {
        _document      = document;
        _undoRedoStack = undoRedoStack;
        _factory       = factory;
    }

    /// <summary>
    /// Executes a batch of tool calls:
    /// <list type="number">
    ///   <item>Validates all tool names.</item>
    ///   <item>Creates commands via <see cref="ToolCallFactory"/>.</item>
    ///   <item>Wraps them in a <see cref="CompositeCommand"/>.</item>
    ///   <item>Pushes the composite onto the undo stack (which also executes it).</item>
    /// </list>
    /// Returns <see cref="ExecutionResult.Fail"/> at the first error; the document is unchanged.
    /// </summary>
    public ExecutionResult Execute(ToolCallBatch batch)
    {
        if (batch.Calls.Count == 0)
            return ExecutionResult.Fail("Batch contains no tool calls.");

        // Step 1: validate all tool names before touching the document
        var unknownTools = batch.Calls
            .Select(c => c.Tool)
            .Where(t => !_validTools.Contains(t))
            .Distinct()
            .ToList();

        if (unknownTools.Count > 0)
            return ExecutionResult.Fail(
                unknownTools.Select(t => $"Unknown tool: '{t}'.").ToArray());

        // Step 2: build commands (parameter errors surface here)
        List<IGrafcetCommand> commands;
        try
        {
            commands = batch.Calls.Select(_factory.Create).ToList();
        }
        catch (Exception ex)
        {
            return ExecutionResult.Fail($"Parameter error: {ex.Message}");
        }

        // Step 3 & 4: wrap + push (CompositeCommand.Execute runs inside Push)
        string description = batch.Explanation
            ?? string.Join(", ", batch.Calls
                .Select(c => c.Description)
                .Where(d => !string.IsNullOrWhiteSpace(d)));

        var composite = new CompositeCommand(commands, description);
        try
        {
            _undoRedoStack.Push(composite, _document);
        }
        catch (Exception ex)
        {
            return ExecutionResult.Fail($"Execution error: {ex.Message}");
        }

        return ExecutionResult.Ok();
    }
}
