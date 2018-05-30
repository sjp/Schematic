using System;

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

        private RgbColor _tableBackgroundColor = new RgbColor("#FFFFFF");
        private RgbColor _headerBackgroundColor = new RgbColor("#BFE3C6");
        private RgbColor _footerBackgroundColor = new RgbColor("#BFE3C6");
        private RgbColor _highlightedHeaderBackgroundColor = new RgbColor("#5CD674");
        private RgbColor _highlightedFooterBackgroundColor = new RgbColor("#5CD674");
    }
}
