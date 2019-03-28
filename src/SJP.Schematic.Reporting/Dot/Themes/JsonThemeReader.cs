using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Dot.Themes
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

            var dto = JsonConvert.DeserializeObject<GraphThemeDto>(json, Settings);
            return new GraphThemeBuilder
            {
                BackgroundColor = dto.BackgroundColor,
                EdgeColor = dto.EdgeColor,

                TableForegroundColor = dto.TableForegroundColor,
                HeaderForegroundColor = dto.HeaderForegroundColor,
                FooterForegroundColor = dto.FooterForegroundColor,
                PrimaryKeyHeaderForegroundColor = dto.PrimaryKeyHeaderForegroundColor,
                UniqueKeyHeaderForegroundColor = dto.UniqueKeyHeaderForegroundColor,
                ForeignKeyHeaderForegroundColor = dto.ForeignKeyHeaderForegroundColor,
                HighlightedTableForegroundColor = dto.HighlightedTableForegroundColor,
                HighlightedHeaderForegroundColor = dto.HighlightedHeaderForegroundColor,
                HighlightedFooterForegroundColor = dto.HighlightedFooterForegroundColor,
                HighlightedPrimaryKeyHeaderForegroundColor = dto.HighlightedPrimaryKeyHeaderForegroundColor,
                HighlightedUniqueKeyHeaderForegroundColor = dto.HighlightedUniqueKeyHeaderForegroundColor,
                HighlightedForeignKeyHeaderForegroundColor = dto.HighlightedForeignKeyHeaderForegroundColor,

                TableBackgroundColor = dto.TableBackgroundColor,
                HeaderBackgroundColor = dto.HeaderBackgroundColor,
                FooterBackgroundColor = dto.FooterBackgroundColor,
                PrimaryKeyHeaderBackgroundColor = dto.PrimaryKeyHeaderBackgroundColor,
                UniqueKeyHeaderBackgroundColor = dto.UniqueKeyHeaderBackgroundColor,
                ForeignKeyHeaderBackgroundColor = dto.ForeignKeyHeaderBackgroundColor,
                HighlightedTableBackgroundColor = dto.HighlightedTableBackgroundColor,
                HighlightedHeaderBackgroundColor = dto.HighlightedHeaderBackgroundColor,
                HighlightedFooterBackgroundColor = dto.HighlightedFooterBackgroundColor,
                HighlightedPrimaryKeyHeaderBackgroundColor = dto.HighlightedPrimaryKeyHeaderBackgroundColor,
                HighlightedUniqueKeyHeaderBackgroundColor = dto.HighlightedUniqueKeyHeaderBackgroundColor,
                HighlightedForeignKeyHeaderBackgroundColor = dto.HighlightedForeignKeyHeaderBackgroundColor,

                TableBorderColor = dto.TableBorderColor,
                HeaderBorderColor = dto.HeaderBorderColor,
                FooterBorderColor = dto.FooterBorderColor,
                PrimaryKeyHeaderBorderColor = dto.PrimaryKeyHeaderBorderColor,
                UniqueKeyHeaderBorderColor = dto.UniqueKeyHeaderBorderColor,
                ForeignKeyHeaderBorderColor = dto.ForeignKeyHeaderBorderColor,
                HighlightedTableBorderColor = dto.HighlightedTableBorderColor,
                HighlightedHeaderBorderColor = dto.HighlightedHeaderBorderColor,
                HighlightedFooterBorderColor = dto.HighlightedFooterBorderColor,
                HighlightedPrimaryKeyHeaderBorderColor = dto.HighlightedPrimaryKeyHeaderBorderColor,
                HighlightedUniqueKeyHeaderBorderColor = dto.HighlightedUniqueKeyHeaderBorderColor,
                HighlightedForeignKeyHeaderBorderColor = dto.HighlightedForeignKeyHeaderBorderColor
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
