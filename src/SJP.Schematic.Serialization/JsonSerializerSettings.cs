using System.Text.Json;
using System.Text.Json.Serialization;

namespace SJP.Schematic.Serialization;

/// <summary>
/// The JSON serializer settings shared by all of the JSON serializers in this project.
/// </summary>
internal static class JsonSerializerSettings
{
    /// <summary>
    /// The settings used when reading or writing a serialized database definition.
    /// </summary>
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        RespectNullableAnnotations = true,
        Converters = { new JsonStringEnumConverter() },
    };
}
