using System;

namespace SJP.Schematic.SchemaSpy.Dot
{
    internal sealed class TableNodeOptions
    {
        public static TableNodeOptions Default { get; } = new TableNodeOptions();

        public bool ShowColumnDataType { get; set; }

        public bool IsReducedColumnSet { get; set; }

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

        private RgbColor _tableBackgroundColor = new RgbColor("#FFFFFF");
        private RgbColor _headerBackgroundColor = new RgbColor("#BFE3C6");
        private RgbColor _footerBackgroundColor = new RgbColor("#BFE3C6");
    }
}
