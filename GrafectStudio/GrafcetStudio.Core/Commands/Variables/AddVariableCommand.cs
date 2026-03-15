using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.Core.Models.Variables;

namespace GrafcetStudio.Core.Commands.Variables;

/// <summary>Adds a new variable to the document's variable table.</summary>
public class AddVariableCommand : IGrafcetCommand
{
    private readonly VariableDeclaration _variable;

    public string Description => $"Add variable '{_variable.Name}'";

    public AddVariableCommand(VariableDeclaration variable)
    {
        _variable = variable;
    }

    public void Execute(GrafcetDocument document)
    {
        if (document.VariableTable.Exists(_variable.Name))
            throw new InvalidOperationException(
                $"Variable '{_variable.Name}' already exists (names are case-insensitive).");

        document.VariableTable.Variables.Add(_variable);
    }

    public void Undo(GrafcetDocument document)
    {
        document.VariableTable.Variables.Remove(_variable);
    }
}
