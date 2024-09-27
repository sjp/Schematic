using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SJP.Schematic.Dot.Themes;

internal sealed class JsonThemeReader
{
    /// <summary>
    /// Attempts to read a JSON description of a theme. Returns the default theme if this was unsuccessful.
    /// </summary>
    /// <param name="json">A JSON theme definition.</param>
    /// <param name="theme">The resulting theme when parsed, the default theme otherwise.</param>
    /// <returns><c>true</c> if a theme was parsed successfully, <c>false</c> otherwise.</returns>
    public bool TryReadFromJson(string json, out IGraphTheme theme)
    {
        try
        {
            theme = ReadFromJson(json);
            return true;
        }
        catch
        {
            theme = new DefaultTheme();
            return false;
        }
    }

    /// <summary>
    /// Reads a theme definition from JSON.
    /// </summary>
    /// <param name="json">The JSON to parse.</param>
    /// <returns>A theme to render a graph with.</returns>
    public IGraphTheme ReadFromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var defaultTheme = new DefaultTheme();
        var dto = JsonSerializer.Deserialize<GraphThemeDto>(json, GetSerializerSettings());
        if (dto == null)
            throw new InvalidOperationException("Failed to parse JSON theme file.");

        return new GraphThemeBuilder
        {
            BackgroundColor = dto.BackgroundColor ?? defaultTheme.BackgroundColor,
            EdgeColor = dto.EdgeColor ?? defaultTheme.EdgeColor,

            TableForegroundColor = dto.TableForegroundColor ?? defaultTheme.TableForegroundColor,
            HeaderForegroundColor = dto.HeaderForegroundColor ?? defaultTheme.HeaderForegroundColor,
            FooterForegroundColor = dto.FooterForegroundColor ?? defaultTheme.FooterForegroundColor,
            PrimaryKeyHeaderForegroundColor = dto.PrimaryKeyHeaderForegroundColor ?? defaultTheme.PrimaryKeyHeaderForegroundColor,
            UniqueKeyHeaderForegroundColor = dto.UniqueKeyHeaderForegroundColor ?? defaultTheme.UniqueKeyHeaderForegroundColor,
            ForeignKeyHeaderForegroundColor = dto.ForeignKeyHeaderForegroundColor ?? defaultTheme.ForeignKeyHeaderForegroundColor,
            HighlightedTableForegroundColor = dto.HighlightedTableForegroundColor ?? defaultTheme.HighlightedTableForegroundColor,
            HighlightedHeaderForegroundColor = dto.HighlightedHeaderForegroundColor ?? defaultTheme.HighlightedHeaderForegroundColor,
            HighlightedFooterForegroundColor = dto.HighlightedFooterForegroundColor ?? defaultTheme.HighlightedFooterForegroundColor,
            HighlightedPrimaryKeyHeaderForegroundColor = dto.HighlightedPrimaryKeyHeaderForegroundColor ?? defaultTheme.HighlightedPrimaryKeyHeaderForegroundColor,
            HighlightedUniqueKeyHeaderForegroundColor = dto.HighlightedUniqueKeyHeaderForegroundColor ?? defaultTheme.HighlightedUniqueKeyHeaderForegroundColor,
            HighlightedForeignKeyHeaderForegroundColor = dto.HighlightedForeignKeyHeaderForegroundColor ?? defaultTheme.HighlightedForeignKeyHeaderForegroundColor,

            TableBackgroundColor = dto.TableBackgroundColor ?? defaultTheme.TableBackgroundColor,
            HeaderBackgroundColor = dto.HeaderBackgroundColor ?? defaultTheme.HeaderBackgroundColor,
            FooterBackgroundColor = dto.FooterBackgroundColor ?? defaultTheme.FooterBackgroundColor,
            PrimaryKeyHeaderBackgroundColor = dto.PrimaryKeyHeaderBackgroundColor ?? defaultTheme.PrimaryKeyHeaderBackgroundColor,
            UniqueKeyHeaderBackgroundColor = dto.UniqueKeyHeaderBackgroundColor ?? defaultTheme.UniqueKeyHeaderBackgroundColor,
            ForeignKeyHeaderBackgroundColor = dto.ForeignKeyHeaderBackgroundColor ?? defaultTheme.ForeignKeyHeaderBackgroundColor,
            HighlightedTableBackgroundColor = dto.HighlightedTableBackgroundColor ?? defaultTheme.HighlightedTableBackgroundColor,
            HighlightedHeaderBackgroundColor = dto.HighlightedHeaderBackgroundColor ?? defaultTheme.HighlightedHeaderBackgroundColor,
            HighlightedFooterBackgroundColor = dto.HighlightedFooterBackgroundColor ?? defaultTheme.HighlightedFooterBackgroundColor,
            HighlightedPrimaryKeyHeaderBackgroundColor = dto.HighlightedPrimaryKeyHeaderBackgroundColor ?? defaultTheme.HighlightedPrimaryKeyHeaderBackgroundColor,
            HighlightedUniqueKeyHeaderBackgroundColor = dto.HighlightedUniqueKeyHeaderBackgroundColor ?? defaultTheme.HighlightedUniqueKeyHeaderBackgroundColor,
            HighlightedForeignKeyHeaderBackgroundColor = dto.HighlightedForeignKeyHeaderBackgroundColor ?? defaultTheme.HighlightedForeignKeyHeaderBackgroundColor,

            TableBorderColor = dto.TableBorderColor ?? defaultTheme.TableBorderColor,
            HeaderBorderColor = dto.HeaderBorderColor ?? defaultTheme.HeaderBorderColor,
            FooterBorderColor = dto.FooterBorderColor ?? defaultTheme.FooterBorderColor,
            PrimaryKeyHeaderBorderColor = dto.PrimaryKeyHeaderBorderColor ?? defaultTheme.PrimaryKeyHeaderBorderColor,
            UniqueKeyHeaderBorderColor = dto.UniqueKeyHeaderBorderColor ?? defaultTheme.UniqueKeyHeaderBorderColor,
            ForeignKeyHeaderBorderColor = dto.ForeignKeyHeaderBorderColor ?? defaultTheme.ForeignKeyHeaderBorderColor,
            HighlightedTableBorderColor = dto.HighlightedTableBorderColor ?? defaultTheme.HighlightedTableBorderColor,
            HighlightedHeaderBorderColor = dto.HighlightedHeaderBorderColor ?? defaultTheme.HighlightedHeaderBorderColor,
            HighlightedFooterBorderColor = dto.HighlightedFooterBorderColor ?? defaultTheme.HighlightedFooterBorderColor,
            HighlightedPrimaryKeyHeaderBorderColor = dto.HighlightedPrimaryKeyHeaderBorderColor ?? defaultTheme.HighlightedPrimaryKeyHeaderBorderColor,
            HighlightedUniqueKeyHeaderBorderColor = dto.HighlightedUniqueKeyHeaderBorderColor ?? defaultTheme.HighlightedUniqueKeyHeaderBorderColor,
            HighlightedForeignKeyHeaderBorderColor = dto.HighlightedForeignKeyHeaderBorderColor ?? defaultTheme.HighlightedForeignKeyHeaderBorderColor
        };
    }

    private static JsonSerializerOptions GetSerializerSettings()
    {
        var settings = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        settings.Converters.Add(new JsonStringEnumConverter());
        settings.Converters.Add(new RgbColorConverter());

        return settings;
    }

    private sealed class RgbColorConverter : JsonConverter<RgbColor>
    {
        public override RgbColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var text = reader.GetString();
            if (text == null)
                throw new InvalidOperationException("Unable to read a colour from the JSON.");

            return new RgbColor(text);
        }

        public override void Write(Utf8JsonWriter writer, RgbColor value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}