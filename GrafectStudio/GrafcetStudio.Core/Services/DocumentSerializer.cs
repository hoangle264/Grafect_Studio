using System.Text.Json;
using System.Text.Json.Serialization;
using GrafcetStudio.Core.Models.Document;

namespace GrafcetStudio.Core.Services;

/// <summary>
/// Saves and loads <see cref="GrafcetDocument"/> to/from <c>.gfx</c> files
/// using <c>System.Text.Json</c>.
/// </summary>
public class DocumentSerializer
{
    /// <summary>Canonical file extension for GRAFCET Studio documents.</summary>
    public const string FILE_EXTENSION = ".gfx";

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented          = true,
        PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters             = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes <paramref name="document"/> and writes it to <paramref name="filePath"/>.
    /// The file is created or overwritten.
    /// </summary>
    public async Task SaveAsync(GrafcetDocument document, string filePath)
    {
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, document, _options);
    }

    /// <summary>
    /// Reads and deserializes a <c>.gfx</c> file from <paramref name="filePath"/>.
    /// </summary>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the content cannot be deserialized.</exception>
    public async Task<GrafcetDocument> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Document file not found: '{filePath}'.");

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<GrafcetDocument>(stream, _options)
               ?? throw new InvalidDataException(
                      $"File '{filePath}' does not contain a valid GRAFCET document.");
    }
}
