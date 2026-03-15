using System.Text;
using System.Text.RegularExpressions;
using GrafcetStudio.Core.CodeGeneration.Interfaces;
using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.Core.Models.Variables;

namespace GrafcetStudio.Core.CodeGeneration.StructuredText;

/// <summary>
/// Generates IEC 61131-3 Structured Text (.st) code from a GRAFCET document.
/// Output structure: TYPE block → VAR blocks → CASE statement.
/// Logical variable names are used directly in code (declared in VAR with optional AT clause).
/// </summary>
public class StructuredTextGenerator : ICodeGenerator
{
    private const string NL  = "\r\n";
    private const string I1  = "    ";        // 1-level indent (4 spaces)
    private const string I2  = "        ";    // 2-level indent
    private const string I3  = "            "; // 3-level indent

    private readonly StructuredTextOptions _opts;

    public string TargetName    => "StructuredText";
    public string FileExtension => ".st";

    public StructuredTextGenerator(StructuredTextOptions? options = null)
    {
        _opts = options ?? new StructuredTextOptions();
    }

    public CodeGenerationResult Generate(GrafcetDocument document, CodeGenOptions options)
    {
        var warnings = new List<string>();

        if (document.Steps.Count == 0)
            return CodeGenerationResult.Fail("Document has no steps.");

        // Apply step filter
        var stepsToGen = (options.TargetStepIds is null
                ? document.Steps
                : document.Steps.Where(s => options.TargetStepIds.Contains(s.Id)))
            .OrderBy(s => s.Id)
            .ToList();

        if (stepsToGen.Count == 0)
            return CodeGenerationResult.Fail("No steps match the specified TargetStepIds.");

        var transitionsToGen = document.Transitions
            .Where(t => stepsToGen.Any(s => s.Id == t.FromStepId))
            .ToList();

        // Enum name map: stepId → enum label (e.g. "STEP_1", "RUNNING")
        var stepNameMap = stepsToGen.ToDictionary(s => s.Id, s => ToEnumName(s));

        // Initial step (used for the eStep default value)
        var initialStep = stepsToGen.FirstOrDefault(s => s.IsInitial) ?? stepsToGen[0];

        // All variables with N qualifier across generated steps (reset before CASE)
        var nOutputs = stepsToGen
            .SelectMany(s => s.Actions)
            .Where(a => a.Qualifier == ActionQualifier.N)
            .Select(a => a.Variable)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // L/D timed actions: each gets a dedicated TON variable
        var timedActions = new List<(GrafcetStep Step, GrafcetAction Action, string TimerName)>();
        foreach (var step in stepsToGen)
            foreach (var action in step.Actions)
                if (action.Qualifier is ActionQualifier.L or ActionQualifier.D)
                {
                    var timerName = $"TON_{stepNameMap[step.Id]}_{SanitizeId(action.Variable)}";
                    timedActions.Add((step, action, timerName));
                }

        string enumTypeName = $"E_{_opts.PouName}_STEP";
        var    sb           = new StringBuilder();

        // ── Part 1: TYPE block ────────────────────────────────────────────────
        if (_opts.UseStepEnum)
        {
            sb.Append("TYPE").Append(NL);
            sb.Append(I1).Append(enumTypeName).Append(" : (").Append(NL);
            for (int i = 0; i < stepsToGen.Count; i++)
            {
                var s = stepsToGen[i];
                sb.Append(I2).Append(stepNameMap[s.Id]).Append(" := ").Append(s.Id);
                if (i < stepsToGen.Count - 1) sb.Append(',');
                sb.Append(NL);
            }
            sb.Append(I1).Append(");").Append(NL);
            sb.Append("END_TYPE").Append(NL).Append(NL);
        }

        // ── POU header ────────────────────────────────────────────────────────
        string pouKeyword = _opts.PouType == PouType.FunctionBlock
            ? "FUNCTION_BLOCK" : "PROGRAM";
        sb.Append(pouKeyword).Append(' ').Append(_opts.PouName).Append(NL);

        // ── Part 2: VAR blocks ────────────────────────────────────────────────
        if (_opts.GenerateVarDecl)
        {
            var inputs  = document.VariableTable.GetByKind(VariableKind.Input).ToList();
            var outputs = document.VariableTable.GetByKind(VariableKind.Output).ToList();
            var others  = document.VariableTable.Variables
                .Where(v => v.Kind != VariableKind.Input && v.Kind != VariableKind.Output)
                .ToList();

            EmitVarBlock(sb, "VAR_INPUT",  inputs);
            EmitVarBlock(sb, "VAR_OUTPUT", outputs);

            // VAR: step tracker + TON timers + other variables
            sb.Append("VAR").Append(NL);

            if (_opts.UseStepEnum)
                sb.Append(I1).Append(_opts.StepVarName).Append(" : ").Append(enumTypeName)
                  .Append(" := ").Append(stepNameMap[initialStep.Id]).Append(';').Append(NL);
            else
                sb.Append(I1).Append(_opts.StepVarName).Append(" : INT := ")
                  .Append(initialStep.Id).Append(';').Append(NL);

            foreach (var (_, _, timerName) in timedActions)
                sb.Append(I1).Append(timerName).Append(" : TON;").Append(NL);

            foreach (var v in others)
                EmitVarLine(sb, v);

            sb.Append("END_VAR").Append(NL);
        }

        sb.Append(NL);

        // ── Part 3a: Reset all N-qualified outputs before CASE ────────────────
        foreach (var varName in nOutputs)
            sb.Append(I1).Append(varName).Append(" := FALSE;").Append(NL);

        if (nOutputs.Count > 0) sb.Append(NL);

        // ── Part 3b: CASE statement ───────────────────────────────────────────
        sb.Append("CASE ").Append(_opts.StepVarName).Append(" OF").Append(NL).Append(NL);

        foreach (var step in stepsToGen)
        {
            string caseLabel = _opts.UseStepEnum
                ? stepNameMap[step.Id]
                : step.Id.ToString();

            sb.Append(I1).Append(caseLabel).Append(':').Append(NL);

            // TON calls for timed actions (before assignments that reference .Q)
            var stepTimers = timedActions.Where(t => t.Step.Id == step.Id).ToList();
            foreach (var (_, action, timerName) in stepTimers)
            {
                string inExpr = _opts.UseStepEnum
                    ? $"{_opts.StepVarName} = {stepNameMap[step.Id]}"
                    : $"{_opts.StepVarName} = {step.Id}";
                string pt = string.IsNullOrWhiteSpace(action.Parameter)
                    ? "T#0s" : action.Parameter;

                sb.Append(I2).Append(timerName).Append("(IN := (")
                  .Append(inExpr).Append("), PT := ").Append(pt).Append(");").Append(NL);
            }

            // Action assignments
            foreach (var action in step.Actions)
            {
                switch (action.Qualifier)
                {
                    case ActionQualifier.N:
                        // N outputs were reset before CASE; set TRUE in active branch
                        sb.Append(I2).Append(action.Variable).Append(" := TRUE;").Append(NL);
                        break;

                    case ActionQualifier.S:
                        sb.Append(I2).Append(action.Variable).Append(" := TRUE;").Append(NL);
                        break;

                    case ActionQualifier.R:
                        sb.Append(I2).Append(action.Variable).Append(" := FALSE;").Append(NL);
                        break;

                    case ActionQualifier.P:
                        // P: pulse – fires on step activation; simplified as toggle
                        sb.Append(I2).Append(action.Variable)
                          .Append(" := NOT ").Append(action.Variable).Append(';').Append(NL);
                        warnings.Add(
                            $"Step {step.Id}: qualifier P on '{action.Variable}' generated as toggle; " +
                            "add rising-edge detection for a proper single-scan pulse.");
                        break;

                    case ActionQualifier.L:
                    {
                        var entry = stepTimers.FirstOrDefault(t => t.Action == action);
                        if (entry == default) break;
                        // L: active while step active and timer has NOT expired
                        sb.Append(I2).Append(action.Variable)
                          .Append(" := NOT ").Append(entry.TimerName).Append(".Q;").Append(NL);
                        break;
                    }

                    case ActionQualifier.D:
                    {
                        var entry = stepTimers.FirstOrDefault(t => t.Action == action);
                        if (entry == default) break;
                        // D: active only after delay (timer.Q)
                        sb.Append(I2).Append(action.Variable)
                          .Append(" := ").Append(entry.TimerName).Append(".Q;").Append(NL);
                        break;
                    }
                }
            }

            // Outgoing transitions → IF condition THEN eStep := next; END_IF;
            var outgoing = transitionsToGen.Where(t => t.FromStepId == step.Id).ToList();
            foreach (var transition in outgoing)
            {
                string toLabel = _opts.UseStepEnum
                    ? stepNameMap.GetValueOrDefault(transition.ToStepId, $"STEP_{transition.ToStepId}")
                    : transition.ToStepId.ToString();

                sb.Append(I2).Append("IF ").Append(transition.Condition).Append(" THEN").Append(NL);
                sb.Append(I3).Append(_opts.StepVarName).Append(" := ").Append(toLabel).Append(';').Append(NL);
                sb.Append(I2).Append("END_IF;").Append(NL);
            }

            sb.Append(NL);
        }

        sb.Append("END_CASE;").Append(NL).Append(NL);

        // ── POU footer ────────────────────────────────────────────────────────
        string pouEnd = _opts.PouType == PouType.FunctionBlock
            ? "END_FUNCTION_BLOCK" : "END_PROGRAM";
        sb.Append(pouEnd).Append(NL);

        return CodeGenerationResult.Ok(sb.ToString(), warnings);
    }

    // ── VAR block helpers ─────────────────────────────────────────────────────

    private static void EmitVarBlock(StringBuilder sb, string keyword, List<VariableDeclaration> vars)
    {
        if (vars.Count == 0) return;
        sb.Append(keyword).Append(NL);
        foreach (var v in vars) EmitVarLine(sb, v);
        sb.Append("END_VAR").Append(NL);
    }

    private static void EmitVarLine(StringBuilder sb, VariableDeclaration v)
    {
        sb.Append(I1).Append(v.Name);

        // AT clause for IEC-standard addresses (%IX0.0, %QX0.0, %MW0, …)
        if (!string.IsNullOrEmpty(v.Address) && v.Address.StartsWith('%'))
            sb.Append(" AT ").Append(v.Address);

        sb.Append(" : ").Append(v.DataType);

        if (!string.IsNullOrEmpty(v.InitValue))
            sb.Append(" := ").Append(v.InitValue);

        sb.Append(';');

        // Collect non-IEC address and user comment into a single (* … *) block
        var commentParts = new List<string>();
        if (!string.IsNullOrEmpty(v.Address) && !v.Address.StartsWith('%'))
            commentParts.Add(v.Address);
        if (!string.IsNullOrEmpty(v.Comment))
            commentParts.Add(v.Comment);
        if (commentParts.Count > 0)
            sb.Append(" (* ").Append(string.Join(", ", commentParts)).Append(" *)");

        sb.Append(NL);
    }

    // ── Name helpers ──────────────────────────────────────────────────────────

    /// <summary>Returns a sanitised UPPER_CASE enum identifier for a step.</summary>
    private static string ToEnumName(GrafcetStep step)
    {
        if (string.IsNullOrWhiteSpace(step.Name))
            return $"STEP_{step.Id}";

        var s = SanitizeId(step.Name).ToUpperInvariant();
        return string.IsNullOrEmpty(s) ? $"STEP_{step.Id}" : s;
    }

    /// <summary>Replaces non-alphanumeric characters with underscores and ensures the result
    /// starts with a letter or underscore.</summary>
    private static string SanitizeId(string raw)
    {
        var s = Regex.Replace(raw.Trim(), @"[^A-Za-z0-9_]", "_");
        s = Regex.Replace(s, "_+", "_").Trim('_');
        if (!string.IsNullOrEmpty(s) && char.IsDigit(s[0])) s = "ID_" + s;
        return s;
    }
}
