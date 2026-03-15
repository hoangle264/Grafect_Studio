using System.Text.Json;
using System.Text.Json.Serialization;
using GrafcetStudio.Core.Commands;
using GrafcetStudio.Core.Commands.Steps;
using GrafcetStudio.Core.Commands.Transitions;
using GrafcetStudio.Core.Commands.Variables;
using GrafcetStudio.Core.Models;
using GrafcetStudio.Core.Models.Document;
using GrafcetStudio.Core.Models.Variables;

namespace GrafcetStudio.Core.Services;

/// <summary>
/// Creates <see cref="IGrafcetCommand"/> instances from <see cref="ToolCall"/> descriptors
/// by deserializing the JSON <c>Params</c> payload and constructing the appropriate command.
/// </summary>
public class ToolCallFactory
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Creates the command matching <paramref name="call"/>.Tool.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the tool name is not recognised.</exception>
    /// <exception cref="InvalidOperationException">Thrown when params cannot be deserialized.</exception>
    public IGrafcetCommand Create(ToolCall call) => call.Tool switch
    {
        "AddStep"          => CreateAddStep(call.Params),
        "RemoveStep"       => CreateRemoveStep(call.Params),
        "ModifyStep"       => CreateModifyStep(call.Params),
        "AddTransition"    => CreateAddTransition(call.Params),
        "RemoveTransition" => CreateRemoveTransition(call.Params),
        "ModifyTransition" => CreateModifyTransition(call.Params),
        "AddVariable"      => CreateAddVariable(call.Params),
        "RemoveVariable"   => CreateRemoveVariable(call.Params),
        "ModifyVariable"   => CreateModifyVariable(call.Params),
        _                  => throw new ArgumentException($"Unknown tool: '{call.Tool}'.")
    };

    // ── Step factories ────────────────────────────────────────────────────────

    private static AddStepCommand CreateAddStep(JsonElement p)
    {
        var dto = Deserialize<AddStepParams>(p);
        var step = new GrafcetStep
        {
            Id        = dto.Id,
            Name      = dto.Name ?? $"Step {dto.Id}",
            IsInitial = dto.IsInitial,
            X         = dto.X,
            Y         = dto.Y,
            Actions   = dto.Actions ?? []
        };
        return new AddStepCommand(step);
    }

    private static RemoveStepCommand CreateRemoveStep(JsonElement p)
    {
        var dto = Deserialize<RemoveStepParams>(p);
        return new RemoveStepCommand(dto.StepId, dto.RemoveOrphanTransitions);
    }

    private static ModifyStepCommand CreateModifyStep(JsonElement p)
    {
        var dto = Deserialize<ModifyStepParams>(p);
        return new ModifyStepCommand(dto.StepId, dto.Name, dto.IsInitial, dto.X, dto.Y, dto.Actions);
    }

    // ── Transition factories ──────────────────────────────────────────────────

    private static AddTransitionCommand CreateAddTransition(JsonElement p)
    {
        var dto = Deserialize<AddTransitionParams>(p);
        var transition = new GrafcetTransition
        {
            Id         = dto.Id,
            Condition  = dto.Condition ?? "TRUE",
            FromStepId = dto.FromStepId,
            ToStepId   = dto.ToStepId
        };
        return new AddTransitionCommand(transition);
    }

    private static RemoveTransitionCommand CreateRemoveTransition(JsonElement p)
    {
        var dto = Deserialize<RemoveTransitionParams>(p);
        return new RemoveTransitionCommand(dto.TransitionId);
    }

    private static ModifyTransitionCommand CreateModifyTransition(JsonElement p)
    {
        var dto = Deserialize<ModifyTransitionParams>(p);
        return new ModifyTransitionCommand(dto.TransitionId, dto.Condition, dto.FromStepId, dto.ToStepId);
    }

    // ── Variable factories ────────────────────────────────────────────────────

    private static AddVariableCommand CreateAddVariable(JsonElement p)
    {
        var dto = Deserialize<AddVariableParams>(p);
        var decl = new VariableDeclaration
        {
            Name      = dto.Name,
            DataType  = dto.DataType,
            Kind      = dto.Kind,
            Address   = dto.Address   ?? "",
            InitValue = dto.InitValue,
            Comment   = dto.Comment,
            Group     = dto.Group
        };
        return new AddVariableCommand(decl);
    }

    private static RemoveVariableCommand CreateRemoveVariable(JsonElement p)
    {
        var dto = Deserialize<RemoveVariableParams>(p);
        return new RemoveVariableCommand(dto.VariableName, dto.CheckUsage);
    }

    private static ModifyVariableCommand CreateModifyVariable(JsonElement p)
    {
        var dto = Deserialize<ModifyVariableParams>(p);
        return new ModifyVariableCommand(
            dto.VariableName, dto.NewName, dto.DataType,
            dto.Kind, dto.Address, dto.InitValue, dto.Comment, dto.Group);
    }

    // ── Deserialize helper ────────────────────────────────────────────────────

    private static T Deserialize<T>(JsonElement element) =>
        element.Deserialize<T>(_jsonOptions)
        ?? throw new InvalidOperationException(
               $"Failed to deserialize params as {typeof(T).Name}.");

    // ── Parameter DTOs (private — used only within this factory) ─────────────

    private sealed record AddStepParams(
        int Id, string? Name, bool IsInitial, double X, double Y,
        List<GrafcetAction>? Actions);

    private sealed record RemoveStepParams(int StepId, bool RemoveOrphanTransitions);

    private sealed record ModifyStepParams(
        int StepId, string? Name, bool? IsInitial, double? X, double? Y,
        List<GrafcetAction>? Actions);

    private sealed record AddTransitionParams(
        int Id, string? Condition, int FromStepId, int ToStepId);

    private sealed record RemoveTransitionParams(int TransitionId);

    private sealed record ModifyTransitionParams(
        int TransitionId, string? Condition, int? FromStepId, int? ToStepId);

    private sealed record AddVariableParams(
        string Name, PlcDataType DataType, VariableKind Kind,
        string? Address, string? InitValue, string? Comment, string? Group);

    private sealed record RemoveVariableParams(string VariableName, bool CheckUsage);

    private sealed record ModifyVariableParams(
        string VariableName, string? NewName, PlcDataType? DataType, VariableKind? Kind,
        string? Address, string? InitValue, string? Comment, string? Group);
}
