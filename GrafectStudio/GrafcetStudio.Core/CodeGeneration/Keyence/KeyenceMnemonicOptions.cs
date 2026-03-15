namespace GrafcetStudio.Core.CodeGeneration.Keyence;

/// <summary>Hardware-specific configuration for the Keyence KV mnemonic generator.</summary>
public class KeyenceMnemonicOptions
{
    /// <summary>Base internal relay address for step activation bits (e.g. "R100").</summary>
    public string StepActiveBitBase { get; set; } = "R100";

    /// <summary>Base timer address for L/D action qualifiers (e.g. "T000").</summary>
    public string TimerBase { get; set; } = "T000";

    /// <summary>Base counter address reserved for future use (e.g. "C000").</summary>
    public string CounterBase { get; set; } = "C000";

    /// <summary>
    /// When true, wraps generated rungs inside a CALL/SBR/RET subroutine block.
    /// </summary>
    public bool UseSubroutine { get; set; } = false;

    /// <summary>
    /// First-scan special relay used to initialise the initial step bit on power-up.
    /// KV-5000/7000: CR2002. KV-3000: R9013.
    /// </summary>
    public string FirstScanContact { get; set; } = "CR2002";
}
