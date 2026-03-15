namespace GrafcetStudio.Core.Models;

/// <summary>Qualifiers for GRAFCET actions as defined in IEC 60848.</summary>
public enum ActionQualifier { N, S, R, P, L, D }

/// <summary>Divergence type of a GRAFCET branch structure.</summary>
public enum BranchType { Parallel, Selective }

/// <summary>IEC 61131-3 data types supported by the variable table.</summary>
public enum PlcDataType
{
    BOOL, INT, UINT, DINT, UDINT,
    REAL, LREAL, TIME, WORD, DWORD,
    STRING, ARRAY, STRUCT
}

/// <summary>Classification of a variable by its role in the PLC I/O model.</summary>
public enum VariableKind
{
    Input, Output, Memory, Timer, Counter, DataWord, Constant
}

/// <summary>IEC 61131-3 Program Organisation Unit type for code generation output.</summary>
public enum PouType { Program, FunctionBlock }
