using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.CodeGeneration.Interfaces;

/// <summary>Controls which steps are included in a generation pass.</summary>
public class CodeGenOptions
{
    /// <summary>When null, all steps are processed. When set, only the specified step IDs are included.</summary>
    public List<int>? TargetStepIds { get; set; }
}

/// <summary>Result returned by an <see cref="ICodeGenerator"/> after a generation pass.</summary>
public class CodeGenerationResult
{
    /// <summary>True when code was produced without fatal errors.</summary>
    public bool Success { get; private init; }

    /// <summary>The generated source code string.</summary>
    public string Code { get; private init; } = "";

    /// <summary>Non-fatal issues encountered during generation (e.g. unresolved variables).</summary>
    public List<string> Warnings { get; private init; } = [];

    /// <summary>Fatal issues that prevented generation.</summary>
    public List<string> Errors { get; private init; } = [];

    private CodeGenerationResult() { }

    /// <summary>Returns a successful result containing the generated code and optional warnings.</summary>
    public static CodeGenerationResult Ok(string code, List<string>? warnings = null) =>
        new() { Success = true, Code = code, Warnings = warnings ?? [] };

    /// <summary>Returns a failed result with one or more error messages.</summary>
    public static CodeGenerationResult Fail(params string[] errors) =>
        new() { Success = false, Errors = [.. errors] };
}

/// <summary>Contract for all code generation strategies (Strategy Pattern).</summary>
public interface ICodeGenerator
{
    /// <summary>Unique name used to look up this generator in <c>CodeGenerationService</c>.</summary>
    string TargetName { get; }

    /// <summary>File extension for the generated output (including the dot, e.g. ".st").</summary>
    string FileExtension { get; }

    /// <summary>Generates source code from <paramref name="document"/> using the supplied options.</summary>
    CodeGenerationResult Generate(GrafcetDocument document, CodeGenOptions options);
}
