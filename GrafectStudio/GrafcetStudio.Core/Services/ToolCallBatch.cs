using System.Text.Json;

namespace GrafcetStudio.Core.Services;

/// <summary>A batch of AI-generated tool calls to be executed as a single undo entry.</summary>
public class ToolCallBatch
{
    /// <summary>Ordered list of tool calls to execute atomically.</summary>
    public List<ToolCall> Calls { get; set; } = [];

    /// <summary>Optional human-readable explanation shown as the undo entry label.</summary>
    public string? Explanation { get; set; }
}

/// <summary>A single tool invocation produced by the AI agent.</summary>
public class ToolCall
{
    /// <summary>Name of the tool to invoke (e.g. "AddStep", "RemoveTransition").</summary>
    public string Tool { get; set; } = "";

    /// <summary>JSON object containing the parameters for the tool.</summary>
    public JsonElement Params { get; set; }

    /// <summary>Human-readable description used as the individual undo label fallback.</summary>
    public string Description { get; set; } = "";
}

/// <summary>Result of executing a <see cref="ToolCallBatch"/>.</summary>
public class ExecutionResult
{
    /// <summary>True when all tool calls executed without errors.</summary>
    public bool Success { get; private init; }

    /// <summary>Error messages when <see cref="Success"/> is false.</summary>
    public List<string> Errors { get; private init; } = [];

    private ExecutionResult() { }

    /// <summary>Returns a successful result.</summary>
    public static ExecutionResult Ok() => new() { Success = true };

    /// <summary>Returns a failed result with one or more error messages.</summary>
    public static ExecutionResult Fail(params string[] errors) =>
        new() { Success = false, Errors = [.. errors] };
}
