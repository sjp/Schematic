using System;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Dot.Themes;

namespace SJP.Schematic.Reporting.Dot
{
    internal sealed class TableNodeOptions
    {
        public static TableNodeOptions Default { get; } = new TableNodeOptions();

        public bool ShowColumnDataType { get; set; }

        public bool IsReducedColumnSet { get; set; }

        public bool IsHighlighted { get; set; }

        public IGraphTheme Theme
        {
            get => _theme;
            set => _theme = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IGraphTheme _theme = new DefaultTheme();

        public RgbColor GetKeyHeaderBackgroundColor(DatabaseKeyType keyType)
        {
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            switch (keyType)
            {
                case DatabaseKeyType.Primary:
                    return Theme.PrimaryKeyHeaderBackgroundColor;
                case DatabaseKeyType.Unique:
                    return Theme.UniqueKeyHeaderBackgroundColor;
                case DatabaseKeyType.Foreign:
                    return Theme.ForeignKeyHeaderBackgroundColor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyType), "Unknown or unsupported key type: " + keyType.GetName());
            }
        }

        public RgbColor GetHighlightedKeyHeaderBackgroundColor(DatabaseKeyType keyType)
        {
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            switch (keyType)
            {
                case DatabaseKeyType.Primary:
                    return Theme.HighlightedPrimaryKeyHeaderBackgroundColor;
                case DatabaseKeyType.Unique:
                    return Theme.HighlightedUniqueKeyHeaderBackgroundColor;
                case DatabaseKeyType.Foreign:
                    return Theme.HighlightedForeignKeyHeaderBackgroundColor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyType), "Unknown or unsupported key type: " + keyType.GetName());
            }
        }
    }
}
