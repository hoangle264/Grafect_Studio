using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.Core.Models.Variables;

namespace GrafcetStudio.Core.Commands.Variables;

/// <summary>Removes a variable from the document's variable table.</summary>
public class RemoveVariableCommand : IGrafcetCommand
{
    private readonly string _variableName;
    private readonly bool _checkUsage;

    private VariableDeclaration? _removed;

    public string Description => $"Remove variable '{_variableName}'";

    public RemoveVariableCommand(string variableName, bool checkUsage)
    {
        _variableName = variableName;
        _checkUsage   = checkUsage;
    }

    public void Execute(GrafcetDocument document)
    {
        _removed = document.VariableTable.Find(_variableName)
            ?? throw new InvalidOperationException($"Variable '{_variableName}' does not exist.");

        if (_checkUsage)
        {
            var searchTargets = CollectSearchTargets(document);
            if (document.VariableTable.IsUsed(_variableName, searchTargets))
                throw new InvalidOperationException(
                    $"Variable '{_variableName}' is still referenced in conditions or actions and cannot be removed.");
        }

        document.VariableTable.Variables.Remove(_removed);
    }

    public void Undo(GrafcetDocument document)
    {
        if (_removed is null) return;

        document.VariableTable.Variables.Add(_removed);
    }

    // Collects all expression strings that may reference variable names
    private static IEnumerable<string> CollectSearchTargets(GrafcetDocument document)
    {
        foreach (var t in document.Transitions)
            yield return t.Condition;

        foreach (var step in document.Steps)
            foreach (var action in step.Actions)
                yield return action.Variable;
    }
}
