using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Dot.Themes
{
    internal class JsonThemeReader
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
            if (json.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(json));

            var defaultTheme = new DefaultTheme();
            var dto = JsonConvert.DeserializeObject<GraphThemeDto>(json, Settings);
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

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter> { new RgbColorConverter() }
        };

        private class RgbColorConverter : JsonConverter<RgbColor>
        {
            public override void WriteJson(JsonWriter writer, RgbColor value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override RgbColor ReadJson(JsonReader reader, Type objectType, RgbColor existingValue, bool hasExistingValue,
                JsonSerializer serializer)
            {
                var text = (string)reader.Value;
                return new RgbColor(text);
            }
        }
    }
}
