using System.Text.RegularExpressions;

namespace GrafcetStudio.Core.Models.Variables;

/// <summary>Holds all variable declarations for a GRAFCET document and provides query helpers.</summary>
public class VariableTable
{
    /// <summary>Ordered list of all declared variables.</summary>
    public List<VariableDeclaration> Variables { get; set; } = new();

    /// <summary>Returns the first variable whose name matches <paramref name="name"/> (case-insensitive), or null.</summary>
    public VariableDeclaration? Find(string name)
        => Variables.FirstOrDefault(v =>
               string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Returns true if a variable with the given name already exists in the table.</summary>
    public bool Exists(string name) => Find(name) is not null;

    /// <summary>Returns all variables of the specified <paramref name="kind"/>.</summary>
    public IEnumerable<VariableDeclaration> GetByKind(VariableKind kind)
        => Variables.Where(v => v.Kind == kind);

    /// <summary>Returns all variables belonging to the specified <paramref name="group"/>.</summary>
    public IEnumerable<VariableDeclaration> GetByGroup(string group)
        => Variables.Where(v => v.Group == group);

    /// <summary>
    /// Returns true if <paramref name="name"/> appears as a whole word in any of the
    /// <paramref name="searchTargets"/> strings (e.g. condition/action expressions).
    /// </summary>
    public bool IsUsed(string name, IEnumerable<string> searchTargets)
        => searchTargets.Any(t => Regex.IsMatch(t,
               $@"\b{Regex.Escape(name)}\b",
               RegexOptions.IgnoreCase));
}
