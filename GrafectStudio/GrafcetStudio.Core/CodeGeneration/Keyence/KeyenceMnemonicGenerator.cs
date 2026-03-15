using System.Text;
using System.Text.RegularExpressions;
using GrafcetStudio.Core.CodeGeneration.Interfaces;
using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.Core.Models.Variables;

namespace GrafcetStudio.Core.CodeGeneration.Keyence;

/// <summary>
/// Generates Keyence KV mnemonic code (.mnm) from a GRAFCET document.
/// Output: UTF-8, CRLF, "OPCODE[TAB]OPERAND" format, no comments.
/// Logical variable names are resolved to hardware addresses via <see cref="VariableResolver"/>.
/// </summary>
public class KeyenceMnemonicGenerator : ICodeGenerator
{
    private const string CRLF = "\r\n";

    private readonly KeyenceMnemonicOptions _opts;

    public string TargetName    => "KeyenceMnemonic";
    public string FileExtension => ".mnm";

    public KeyenceMnemonicGenerator(KeyenceMnemonicOptions? options = null)
    {
        _opts = options ?? new KeyenceMnemonicOptions();
    }

    public CodeGenerationResult Generate(GrafcetDocument document, CodeGenOptions options)
    {
        var warnings = new List<string>();

        if (document.Steps.Count == 0)
            return CodeGenerationResult.Fail("Document has no steps.");

        var initialSteps = document.Steps.Where(s => s.IsInitial).ToList();
        if (initialSteps.Count == 0)
            return CodeGenerationResult.Fail("No initial step defined.");
        if (initialSteps.Count > 1)
            warnings.Add("Multiple initial steps found; all will be SET on first scan.");

        // Apply step filter
        var stepsToGen = (options.TargetStepIds is null
                ? document.Steps
                : document.Steps.Where(s => options.TargetStepIds.Contains(s.Id)))
            .OrderBy(s => s.Id)
            .ToList();

        if (stepsToGen.Count == 0)
            return CodeGenerationResult.Fail("No steps match the specified TargetStepIds.");

        // Build step → bit address map for ALL document steps (transitions may reference non-generated steps)
        var allSorted   = document.Steps.OrderBy(s => s.Id).ToList();
        var stepBitMap  = allSorted
            .Select((s, i) => (s.Id, Bit: OffsetAddress(_opts.StepActiveBitBase, i)))
            .ToDictionary(x => x.Id, x => x.Bit);

        // Build timer map for L/D qualifiers in generated steps
        var timerMap = new Dictionary<(int StepId, int ActionIdx), string>();
        int timerIdx = 0;
        foreach (var step in stepsToGen)
            for (int ai = 0; ai < step.Actions.Count; ai++)
                if (step.Actions[ai].Qualifier is ActionQualifier.L or ActionQualifier.D)
                    timerMap[(step.Id, ai)] = OffsetAddress(_opts.TimerBase, timerIdx++);

        var resolver = new VariableResolver(document.VariableTable);
        var sb       = new StringBuilder();

        // ── Subroutine wrapper header ─────────────────────────────────────────
        if (_opts.UseSubroutine)
        {
            EmitLine(sb, "CALL", "1");
            sb.Append("END").Append(CRLF);
            EmitLine(sb, "SBR", "1");
        }

        // ── Section 1: initialise initial step bit(s) on first scan ──────────
        foreach (var init in initialSteps)
        {
            if (!stepBitMap.TryGetValue(init.Id, out var initBit)) continue;
            EmitLine(sb, "LD",  _opts.FirstScanContact);
            EmitLine(sb, "SET", initBit);
        }

        // ── Section 2: transition rungs ───────────────────────────────────────
        var transitionsToGen = document.Transitions
            .Where(t => stepsToGen.Any(s => s.Id == t.FromStepId))
            .ToList();

        foreach (var transition in transitionsToGen)
        {
            if (!stepBitMap.TryGetValue(transition.FromStepId, out var fromBit)) continue;
            if (!stepBitMap.TryGetValue(transition.ToStepId,   out var toBit))
            {
                warnings.Add($"Transition {transition.Id}: ToStepId {transition.ToStepId} not found in document; skipped.");
                continue;
            }

            EmitLine(sb, "LD", fromBit);
            AppendConditionInstructions(sb, transition.Condition, resolver, warnings);
            EmitLine(sb, "RST", fromBit);
            EmitLine(sb, "SET", toBit);
        }

        // ── Section 3: action rungs ───────────────────────────────────────────
        foreach (var step in stepsToGen)
        {
            if (!stepBitMap.TryGetValue(step.Id, out var stepBit)) continue;

            for (int ai = 0; ai < step.Actions.Count; ai++)
            {
                var action      = step.Actions[ai];
                var resolvedVar = resolver.ResolveAddress(action.Variable);

                switch (action.Qualifier)
                {
                    case ActionQualifier.N:
                        EmitLine(sb, "LD",  stepBit);
                        EmitLine(sb, "OUT", resolvedVar);
                        break;

                    case ActionQualifier.S:
                        EmitLine(sb, "LD",  stepBit);
                        EmitLine(sb, "SET", resolvedVar);
                        break;

                    case ActionQualifier.R:
                        EmitLine(sb, "LD",  stepBit);
                        EmitLine(sb, "RST", resolvedVar);
                        break;

                    case ActionQualifier.P:
                        EmitLine(sb, "LD",   stepBit);
                        EmitLine(sb, "DIFU", resolvedVar);
                        break;

                    case ActionQualifier.L:
                    {
                        var timerAddr = timerMap.GetValueOrDefault((step.Id, ai), OffsetAddress(_opts.TimerBase, 0));
                        var setVal    = action.Parameter ?? "#100";
                        // Timer runs while step is active
                        EmitLine(sb, "LD",  stepBit);
                        sb.Append("TIM").Append('\t').Append(timerAddr).Append('\t').Append(setVal).Append(CRLF);
                        // Output while step active AND timer not expired
                        EmitLine(sb, "LD",  stepBit);
                        EmitLine(sb, "ANI", timerAddr);
                        EmitLine(sb, "OUT", resolvedVar);
                        break;
                    }

                    case ActionQualifier.D:
                    {
                        var timerAddr = timerMap.GetValueOrDefault((step.Id, ai), OffsetAddress(_opts.TimerBase, 0));
                        var setVal    = action.Parameter ?? "#100";
                        // Timer runs while step is active
                        EmitLine(sb, "LD",  stepBit);
                        sb.Append("TIM").Append('\t').Append(timerAddr).Append('\t').Append(setVal).Append(CRLF);
                        // Output only after timer expires
                        EmitLine(sb, "LD",  timerAddr);
                        EmitLine(sb, "OUT", resolvedVar);
                        break;
                    }
                }
            }
        }

        // ── Program / subroutine terminator ───────────────────────────────────
        sb.Append(_opts.UseSubroutine ? "RET" : "END").Append(CRLF);

        return CodeGenerationResult.Ok(sb.ToString(), warnings);
    }

    // ── Condition parser ──────────────────────────────────────────────────────

    private static void AppendConditionInstructions(
        StringBuilder sb, string condition, VariableResolver resolver, List<string> warnings)
    {
        condition = condition.Trim();

        if (string.Equals(condition, "TRUE", StringComparison.OrdinalIgnoreCase))
            return;

        if (string.Equals(condition, "FALSE", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add($"Condition 'FALSE' found; this transition can never fire.");
            return;
        }

        bool hasAnd = Regex.IsMatch(condition, @"\bAND\b", RegexOptions.IgnoreCase);
        bool hasOr  = Regex.IsMatch(condition, @"\bOR\b",  RegexOptions.IgnoreCase);

        if (hasAnd && hasOr)
            warnings.Add($"Mixed AND/OR condition '{condition}' detected; generated code may require manual review.");

        if (hasOr && !hasAnd)
        {
            // OR-only: LD first term, OR remaining terms
            var terms = Regex.Split(condition, @"\s+OR\s+", RegexOptions.IgnoreCase);
            bool first = true;
            foreach (var term in terms)
            {
                var (neg, name) = ParseFactor(term.Trim());
                var addr = resolver.ResolveAddress(name);
                EmitLine(sb, first ? (neg ? "LDI" : "LD") : (neg ? "ORI" : "OR"), addr);
                first = false;
            }
            return;
        }

        // AND-only or single factor (also handles complex: treat as AND chain)
        var andTerms = Regex.Split(condition, @"\s+AND\s+", RegexOptions.IgnoreCase);
        bool isFirst = true;
        foreach (var term in andTerms)
        {
            var (neg, name) = ParseFactor(term.Trim());
            var addr = resolver.ResolveAddress(name);
            EmitLine(sb, isFirst ? (neg ? "LDI" : "LD") : (neg ? "ANI" : "AND"), addr);
            isFirst = false;
        }
    }

    private static (bool negate, string name) ParseFactor(string token)
    {
        token = token.Trim();
        if (token.StartsWith("NOT", StringComparison.OrdinalIgnoreCase) &&
            token.Length > 3 && char.IsWhiteSpace(token[3]))
        {
            return (true, token[3..].TrimStart());
        }
        return (false, token);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void EmitLine(StringBuilder sb, string opcode, string operand) =>
        sb.Append(opcode).Append('\t').Append(operand).Append(CRLF);

    /// <summary>
    /// Parses "R100" → prefix "R", number 100, then returns "R" + (100+offset) zero-padded
    /// to the original digit count: OffsetAddress("R100", 3) → "R103".
    /// </summary>
    private static string OffsetAddress(string baseAddr, int offset)
    {
        var m = Regex.Match(baseAddr, @"^([A-Za-z]+)(\d+)$");
        if (!m.Success) return baseAddr + offset;

        string prefix   = m.Groups[1].Value;
        int    start    = int.Parse(m.Groups[2].Value);
        int    digits   = m.Groups[2].Length;
        return prefix + (start + offset).ToString().PadLeft(digits, '0');
    }
}
