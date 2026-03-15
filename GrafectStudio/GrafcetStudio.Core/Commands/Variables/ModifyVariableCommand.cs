using System.Text.RegularExpressions;
using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.Core.Models.Variables;

namespace GrafcetStudio.Core.Commands.Variables;

/// <summary>
/// Partially updates one or more fields of an existing variable.
/// When the name changes, all references in transition conditions and step actions
/// are updated automatically using word-boundary replacement.
/// </summary>
public class ModifyVariableCommand : IGrafcetCommand
{
    private readonly string _variableName;
    private readonly string? _newName;
    private readonly PlcDataType? _dataType;
    private readonly VariableKind? _kind;
    private readonly string? _address;
    private readonly string? _initValue;
    private readonly string? _comment;
    private readonly string? _group;

    private record Snapshot(string Name, PlcDataType DataType, VariableKind Kind,
        string Address, string? InitValue, string? Comment, string? Group);
    private Snapshot? _snapshot;

    // Patches applied during Execute; reversed during Undo
    private readonly List<(GrafcetTransition Transition, string OldCondition)> _conditionPatches = [];
    private readonly List<(GrafcetAction Action, string OldVariable)> _actionPatches = [];

    public string Description => $"Modify variable '{_variableName}'";

    public ModifyVariableCommand(string variableName,
        string?      newName   = null,
        PlcDataType? dataType  = null,
        VariableKind? kind     = null,
        string?      address   = null,
        string?      initValue = null,
        string?      comment   = null,
        string?      group     = null)
    {
        _variableName = variableName;
        _newName      = newName;
        _dataType     = dataType;
        _kind         = kind;
        _address      = address;
        _initValue    = initValue;
        _comment      = comment;
        _group        = group;
    }

    public void Execute(GrafcetDocument document)
    {
        // Clear patches from any previous Execute (Redo scenario)
        _conditionPatches.Clear();
        _actionPatches.Clear();

        var decl = document.VariableTable.Find(_variableName)
            ?? throw new InvalidOperationException($"Variable '{_variableName}' does not exist.");

        // Validate new name uniqueness before mutating anything
        if (_newName is not null &&
            !string.Equals(_newName, _variableName, StringComparison.OrdinalIgnoreCase) &&
            document.VariableTable.Exists(_newName))
        {
            throw new InvalidOperationException(
                $"Variable '{_newName}' already exists (names are case-insensitive).");
        }

        _snapshot = new Snapshot(decl.Name, decl.DataType, decl.Kind,
            decl.Address, decl.InitValue, decl.Comment, decl.Group);

        if (_dataType  is not null) decl.DataType  = _dataType.Value;
        if (_kind      is not null) decl.Kind      = _kind.Value;
        if (_address   is not null) decl.Address   = _address;
        if (_initValue is not null) decl.InitValue = _initValue;
        if (_comment   is not null) decl.Comment   = _comment;
        if (_group     is not null) decl.Group     = _group;

        // Apply rename last so lookups above still work with the old name
        if (_newName is not null &&
            !string.Equals(_newName, _variableName, StringComparison.OrdinalIgnoreCase))
        {
            decl.Name = _newName;
            UpdateReferences(document, _variableName, _newName);
        }
    }

    public void Undo(GrafcetDocument document)
    {
        if (_snapshot is null) return;

        // Find by current (possibly renamed) name
        var decl = document.VariableTable.Find(_newName ?? _variableName);
        if (decl is null) return;

        // Reverse reference patches before restoring name
        foreach (var (transition, oldCondition) in _conditionPatches)
            transition.Condition = oldCondition;

        foreach (var (action, oldVariable) in _actionPatches)
            action.Variable = oldVariable;

        _conditionPatches.Clear();
        _actionPatches.Clear();

        decl.Name      = _snapshot.Name;
        decl.DataType  = _snapshot.DataType;
        decl.Kind      = _snapshot.Kind;
        decl.Address   = _snapshot.Address;
        decl.InitValue = _snapshot.InitValue;
        decl.Comment   = _snapshot.Comment;
        decl.Group     = _snapshot.Group;
    }

    // Replaces all occurrences of oldName with newName using word-boundary regex
    private void UpdateReferences(GrafcetDocument document, string oldName, string newName)
    {
        string pattern = $@"\b{Regex.Escape(oldName)}\b";

        foreach (var t in document.Transitions)
        {
            string updated = Regex.Replace(t.Condition, pattern, newName, RegexOptions.IgnoreCase);
            if (!string.Equals(updated, t.Condition, StringComparison.Ordinal))
            {
                _conditionPatches.Add((t, t.Condition));
                t.Condition = updated;
            }
        }

        foreach (var step in document.Steps)
        {
            foreach (var action in step.Actions)
            {
                if (string.Equals(action.Variable, oldName, StringComparison.OrdinalIgnoreCase))
                {
                    _actionPatches.Add((action, action.Variable));
                    action.Variable = newName;
                }
            }
        }
    }
}
