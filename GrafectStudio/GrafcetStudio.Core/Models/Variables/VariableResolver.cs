using System.Text.RegularExpressions;

namespace GrafcetStudio.Core.Models.Variables;

/// <summary>
/// Resolves logical variable names to PLC hardware addresses using the document's
/// <see cref="VariableTable"/>. Unresolved names are passed through unchanged and
/// recorded in <see cref="Warnings"/>.
/// </summary>
public class VariableResolver
{
    // PLC boolean keywords that must never be treated as variable names
    private static readonly HashSet<string> _plcKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "TRUE", "FALSE", "AND", "OR", "NOT", "XOR", "AND_NOT", "OR_NOT"
    };

    private readonly VariableTable _table;
    private readonly List<string> _warnings = new();

    /// <summary>Warnings produced by the most recent resolve call.</summary>
    public IReadOnlyList<string> Warnings => _warnings;

    public VariableResolver(VariableTable table)
    {
        _table = table;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the hardware address for <paramref name="name"/>.
    /// If no mapping exists or the address is empty the original name is returned (pass-through).
    /// </summary>
    public string ResolveAddress(string name)
    {
        var decl = _table.Find(name);
        return decl is { Address.Length: > 0 } ? decl.Address : name;
    }

    /// <summary>
    /// Replaces every logical variable name found in <paramref name="condition"/> with its
    /// hardware address. Uses word-boundary matching to avoid partial replacements.
    /// Unresolved names are left unchanged and recorded in <see cref="Warnings"/>.
    /// </summary>
    public string ResolveCondition(string condition)
    {
        _warnings.Clear();

        // Record warnings for any identifiers that cannot be resolved
        foreach (string unresolved in FindUnresolved(condition))
            _warnings.Add($"Variable '{unresolved}' could not be resolved; using pass-through.");

        string result = condition;
        foreach (var decl in _table.Variables)
        {
            if (string.IsNullOrEmpty(decl.Address)) continue;

            result = Regex.Replace(
                result,
                $@"\b{Regex.Escape(decl.Name)}\b",
                decl.Address,
                RegexOptions.IgnoreCase);
        }

        return result;
    }

    /// <summary>
    /// Returns the list of identifier tokens in <paramref name="expression"/> that are
    /// neither PLC keywords nor present in the variable table.
    /// </summary>
    public IReadOnlyList<string> FindUnresolved(string expression)
    {
        var tokens = Regex.Matches(expression, @"\b[A-Za-z_][A-Za-z0-9_]*\b")
                          .Select(m => m.Value)
                          .Distinct(StringComparer.OrdinalIgnoreCase);

        return tokens
            .Where(t => !_plcKeywords.Contains(t) && _table.Find(t) is null)
            .ToList();
    }
}
