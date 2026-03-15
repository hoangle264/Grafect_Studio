using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Commands;

/// <summary>Contract for every reversible operation on a <see cref="GrafcetDocument"/>.</summary>
public interface IGrafcetCommand
{
    /// <summary>Short human-readable description shown in the Undo/Redo menu.</summary>
    string Description { get; }

    /// <summary>Applies the operation to <paramref name="document"/>.</summary>
    void Execute(GrafcetDocument document);

    /// <summary>Reverses the operation on <paramref name="document"/>.</summary>
    void Undo(GrafcetDocument document);
}
