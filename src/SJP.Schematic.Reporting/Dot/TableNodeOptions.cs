using System;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Dot
{
    internal sealed class TableNodeOptions
    {
        public static TableNodeOptions Default { get; } = new TableNodeOptions();

        public bool ShowColumnDataType { get; set; }

        public bool IsReducedColumnSet { get; set; }

        public bool IsHighlighted { get; set; }

        public RgbColor TableBackgroundColor
        {
            get => _tableBackgroundColor;
            set => _tableBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set => _headerBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor FooterBackgroundColor
        {
            get => _footerBackgroundColor;
            set => _footerBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor GetKeyHeaderBackgroundColor(DatabaseKeyType keyType)
        {
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

            switch (keyType)
            {
                case DatabaseKeyType.Primary:
                    return PrimaryKeyHeaderBackgroundColor;
                case DatabaseKeyType.Unique:
                    return UniqueKeyHeaderBackgroundColor;
                case DatabaseKeyType.Foreign:
                    return ForeignKeyHeaderBackgroundColor;
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
                    return HighlightedPrimaryKeyHeaderBackgroundColor;
                case DatabaseKeyType.Unique:
                    return HighlightedUniqueKeyHeaderBackgroundColor;
                case DatabaseKeyType.Foreign:
                    return HighlightedForeignKeyHeaderBackgroundColor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyType), "Unknown or unsupported key type: " + keyType.GetName());
            }
        }

        public RgbColor PrimaryKeyHeaderBackgroundColor
        {
            get => _primaryKeyHeaderBackgroundColor;
            set => _primaryKeyHeaderBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor UniqueKeyHeaderBackgroundColor
        {
            get => _uniqueKeyHeaderBackgroundColor;
            set => _uniqueKeyHeaderBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor ForeignKeyHeaderBackgroundColor
        {
            get => _foreignKeyHeaderBackgroundColor;
            set => _foreignKeyHeaderBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor HighlightedHeaderBackgroundColor
        {
            get => _highlightedHeaderBackgroundColor;
            set => _highlightedHeaderBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor HighlightedFooterBackgroundColor
        {
            get => _highlightedFooterBackgroundColor;
            set => _highlightedFooterBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor HighlightedPrimaryKeyHeaderBackgroundColor
        {
            get => _highlightedPrimaryKeyHeaderBackgroundColor;
            set => _highlightedPrimaryKeyHeaderBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor HighlightedUniqueKeyHeaderBackgroundColor
        {
            get => _highlightedUniqueKeyHeaderBackgroundColor;
            set => _highlightedUniqueKeyHeaderBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RgbColor HighlightedForeignKeyHeaderBackgroundColor
        {
            get => _highlightedForeignKeyHeaderBackgroundColor;
            set => _highlightedForeignKeyHeaderBackgroundColor = value ?? throw new ArgumentNullException(nameof(value));
        }

        private RgbColor _tableBackgroundColor = new RgbColor("#FFFFFF");
        private RgbColor _headerBackgroundColor = new RgbColor("#BFE3C6");
        private RgbColor _footerBackgroundColor = new RgbColor("#BFE3C6");

        private RgbColor _primaryKeyHeaderBackgroundColor = new RgbColor("#EFEBA8");
        private RgbColor _uniqueKeyHeaderBackgroundColor = new RgbColor("#B8D0DD");
        private RgbColor _foreignKeyHeaderBackgroundColor = new RgbColor("#E5E5E5");

        private RgbColor _highlightedHeaderBackgroundColor = new RgbColor("#7DDE90");
        private RgbColor _highlightedFooterBackgroundColor = new RgbColor("#7DDE90");

        private RgbColor _highlightedPrimaryKeyHeaderBackgroundColor = new RgbColor("#D7CD28");
        private RgbColor _highlightedUniqueKeyHeaderBackgroundColor = new RgbColor("#8FB3C7");
        private RgbColor _highlightedForeignKeyHeaderBackgroundColor = new RgbColor("#B0B0B0");
    }
}
