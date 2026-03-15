using GrafcetStudio.Core.CodeGeneration.Interfaces;

namespace GrafcetStudio.Core.CodeGeneration;

/// <summary>
/// Open/closed registry of code generators. Add new targets via <see cref="Register"/>
/// without modifying existing code.
/// </summary>
public class CodeGenerationService
{
    private readonly Dictionary<string, ICodeGenerator> _generators =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Names of all currently registered generation targets.</summary>
    public IEnumerable<string> AvailableTargets => _generators.Keys;

    /// <summary>Registers or replaces a generator for its declared <c>TargetName</c>.</summary>
    public void Register(ICodeGenerator generator)
    {
        _generators[generator.TargetName] = generator;
    }

    /// <summary>Returns the generator for the given target name.</summary>
    /// <exception cref="KeyNotFoundException">Thrown when no generator is registered for the name.</exception>
    public ICodeGenerator Get(string targetName)
    {
        if (_generators.TryGetValue(targetName, out var gen))
            return gen;

        throw new KeyNotFoundException(
            $"No generator registered for target '{targetName}'. " +
            $"Available: {string.Join(", ", AvailableTargets)}");
    }

    /// <summary>Returns true when a generator is registered for <paramref name="targetName"/>.</summary>
    public bool IsRegistered(string targetName) =>
        _generators.ContainsKey(targetName);
}
