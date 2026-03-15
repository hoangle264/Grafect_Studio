using GrafcetStudio.Core.Models;

namespace GrafcetStudio.Core.Models.Variables;

/// <summary>Represents a single variable entry in the GRAFCET variable table.</summary>
public class VariableDeclaration
{
    /// <summary>Logical name used in conditions and actions.</summary>
    public string Name { get; set; } = "";

    /// <summary>IEC 61131-3 data type of the variable.</summary>
    public PlcDataType DataType { get; set; }

    /// <summary>Role of the variable in the PLC I/O model.</summary>
    public VariableKind Kind { get; set; }

    /// <summary>Hardware address (e.g. %IX0.0, DM100). Empty when not mapped.</summary>
    public string Address { get; set; } = "";

    /// <summary>Initial value expressed as a string literal; null means use type default.</summary>
    public string? InitValue { get; set; }

    /// <summary>Optional human-readable comment for documentation purposes.</summary>
    public string? Comment { get; set; }

    /// <summary>Optional group tag for organising variables in the UI.</summary>
    public string? Group { get; set; }
}
