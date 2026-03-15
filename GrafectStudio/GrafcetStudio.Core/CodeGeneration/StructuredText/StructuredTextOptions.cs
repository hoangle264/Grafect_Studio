using GrafcetStudio.Core.Models;

namespace GrafcetStudio.Core.CodeGeneration.StructuredText;

/// <summary>Configuration for the IEC 61131-3 Structured Text code generator.</summary>
public class StructuredTextOptions
{
    /// <summary>Name of the generated Program Organisation Unit.</summary>
    public string PouName { get; set; } = "GRAFCET_Main";

    /// <summary>Whether to emit a FUNCTION_BLOCK or PROGRAM POU.</summary>
    public PouType PouType { get; set; } = PouType.FunctionBlock;

    /// <summary>
    /// When true, emits a TYPE block with a named enum for step values and uses
    /// enum literals in the CASE statement and transitions.
    /// </summary>
    public bool UseStepEnum { get; set; } = true;

    /// <summary>Name of the step-tracking variable declared in the VAR block.</summary>
    public string StepVarName { get; set; } = "eStep";

    /// <summary>When true, emits VAR_INPUT, VAR_OUTPUT, and VAR declaration blocks.</summary>
    public bool GenerateVarDecl { get; set; } = true;
}
