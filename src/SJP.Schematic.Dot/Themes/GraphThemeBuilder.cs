namespace SJP.Schematic.Dot.Themes
{
    internal sealed class GraphThemeBuilder : IGraphTheme
    {
        public RgbColor BackgroundColor
        {
            get => _backgroundColor ??= Default.BackgroundColor;
            set => _backgroundColor = value ?? Default.BackgroundColor;
        }

        public RgbColor EdgeColor
        {
            get => _edgeColor ??= Default.EdgeColor;
            set => _edgeColor = value ?? Default.EdgeColor;
        }

        public RgbColor TableForegroundColor
        {
            get => _tableForegroundColor ??= Default.TableForegroundColor;
            set => _tableForegroundColor = value ?? Default.TableForegroundColor;
        }

        public RgbColor HeaderForegroundColor
        {
            get => _headerForegroundColor ??= Default.HeaderForegroundColor;
            set => _headerForegroundColor = value ?? Default.HeaderForegroundColor;
        }

        public RgbColor FooterForegroundColor
        {
            get => _footerForegroundColor ??= Default.FooterForegroundColor;
            set => _footerForegroundColor = value ?? Default.FooterForegroundColor;
        }

        public RgbColor PrimaryKeyHeaderForegroundColor
        {
            get => _primaryKeyHeaderForegroundColor ??= Default.PrimaryKeyHeaderForegroundColor;
            set => _primaryKeyHeaderForegroundColor = value ?? Default.PrimaryKeyHeaderForegroundColor;
        }

        public RgbColor UniqueKeyHeaderForegroundColor
        {
            get => _uniqueKeyHeaderForegroundColor ??= Default.UniqueKeyHeaderForegroundColor;
            set => _uniqueKeyHeaderForegroundColor = value ?? Default.UniqueKeyHeaderForegroundColor;
        }

        public RgbColor ForeignKeyHeaderForegroundColor
        {
            get => _foreignKeyHeaderForegroundColor ??= Default.ForeignKeyHeaderForegroundColor;
            set => _foreignKeyHeaderForegroundColor = value ?? Default.ForeignKeyHeaderForegroundColor;
        }

        public RgbColor HighlightedTableForegroundColor
        {
            get => _highlightedTableForegroundColor ??= Default.HighlightedTableForegroundColor;
            set => _highlightedTableForegroundColor = value ?? Default.HighlightedTableForegroundColor;
        }

        public RgbColor HighlightedHeaderForegroundColor
        {
            get => _highlightedHeaderForegroundColor ??= Default.HighlightedHeaderForegroundColor;
            set => _highlightedHeaderForegroundColor = value ?? Default.HighlightedHeaderForegroundColor;
        }

        public RgbColor HighlightedFooterForegroundColor
        {
            get => _highlightedFooterForegroundColor ??= Default.HighlightedFooterForegroundColor;
            set => _highlightedFooterForegroundColor = value ?? Default.HighlightedFooterForegroundColor;
        }

        public RgbColor HighlightedPrimaryKeyHeaderForegroundColor
        {
            get => _highlightedPrimaryKeyHeaderForegroundColor ??= Default.HighlightedPrimaryKeyHeaderForegroundColor;
            set => _highlightedPrimaryKeyHeaderForegroundColor = value ?? Default.HighlightedPrimaryKeyHeaderForegroundColor;
        }

        public RgbColor HighlightedUniqueKeyHeaderForegroundColor
        {
            get => _highlightedUniqueKeyHeaderForegroundColor ??= Default.HighlightedUniqueKeyHeaderForegroundColor;
            set => _highlightedUniqueKeyHeaderForegroundColor = value ?? Default.HighlightedUniqueKeyHeaderForegroundColor;
        }

        public RgbColor HighlightedForeignKeyHeaderForegroundColor
        {
            get => _highlightedForeignKeyHeaderForegroundColor ??= Default.HighlightedForeignKeyHeaderForegroundColor;
            set => _highlightedForeignKeyHeaderForegroundColor = value ?? Default.HighlightedForeignKeyHeaderForegroundColor;
        }

        public RgbColor TableBackgroundColor
        {
            get => _tableBackgroundColor ??= Default.TableBackgroundColor;
            set => _tableBackgroundColor = value ?? Default.TableBackgroundColor;
        }

        public RgbColor HeaderBackgroundColor
        {
            get => _headerBackgroundColor ??= Default.HeaderBackgroundColor;
            set => _headerBackgroundColor = value ?? Default.HeaderBackgroundColor;
        }

        public RgbColor FooterBackgroundColor
        {
            get => _footerBackgroundColor ??= Default.FooterBackgroundColor;
            set => _footerBackgroundColor = value ?? Default.FooterBackgroundColor;
        }

        public RgbColor PrimaryKeyHeaderBackgroundColor
        {
            get => _primaryKeyHeaderBackgroundColor ??= Default.PrimaryKeyHeaderBackgroundColor;
            set => _primaryKeyHeaderBackgroundColor = value ?? Default.PrimaryKeyHeaderBackgroundColor;
        }

        public RgbColor UniqueKeyHeaderBackgroundColor
        {
            get => _uniqueKeyHeaderBackgroundColor ??= Default.UniqueKeyHeaderBackgroundColor;
            set => _uniqueKeyHeaderBackgroundColor = value ?? Default.UniqueKeyHeaderBackgroundColor;
        }

        public RgbColor ForeignKeyHeaderBackgroundColor
        {
            get => _foreignKeyHeaderBackgroundColor ??= Default.ForeignKeyHeaderBackgroundColor;
            set => _foreignKeyHeaderBackgroundColor = value ?? Default.ForeignKeyHeaderBackgroundColor;
        }

        public RgbColor HighlightedTableBackgroundColor
        {
            get => _highlightedTableBackgroundColor ??= Default.HighlightedTableBackgroundColor;
            set => _highlightedTableBackgroundColor = value ?? Default.HighlightedTableBackgroundColor;
        }

        public RgbColor HighlightedHeaderBackgroundColor
        {
            get => _highlightedHeaderBackgroundColor ??= Default.HighlightedFooterBackgroundColor;
            set => _highlightedHeaderBackgroundColor = value ?? Default.HighlightedHeaderBackgroundColor;
        }

        public RgbColor HighlightedFooterBackgroundColor
        {
            get => _highlightedFooterBackgroundColor ??= Default.HighlightedFooterBackgroundColor;
            set => _highlightedFooterBackgroundColor = value ?? Default.HighlightedFooterBackgroundColor;
        }

        public RgbColor HighlightedPrimaryKeyHeaderBackgroundColor
        {
            get => _highlightedPrimaryKeyHeaderBackgroundColor ??= Default.HighlightedPrimaryKeyHeaderBackgroundColor;
            set => _highlightedPrimaryKeyHeaderBackgroundColor = value ?? Default.HighlightedPrimaryKeyHeaderBackgroundColor;
        }

        public RgbColor HighlightedUniqueKeyHeaderBackgroundColor
        {
            get => _highlightedUniqueKeyHeaderBackgroundColor ??= Default.HighlightedUniqueKeyHeaderBackgroundColor;
            set => _highlightedUniqueKeyHeaderBackgroundColor = value ?? Default.HighlightedUniqueKeyHeaderBackgroundColor;
        }

        public RgbColor HighlightedForeignKeyHeaderBackgroundColor
        {
            get => _highlightedForeignKeyHeaderBackgroundColor ??= Default.HighlightedForeignKeyHeaderBackgroundColor;
            set => _highlightedForeignKeyHeaderBackgroundColor = value ?? Default.HighlightedForeignKeyHeaderBackgroundColor;
        }

        public RgbColor TableBorderColor
        {
            get => _tableBorderColor ??= Default.TableBorderColor;
            set => _tableBorderColor = value ?? Default.TableBorderColor;
        }

        public RgbColor HeaderBorderColor
        {
            get => _headerBorderColor ??= Default.HeaderBorderColor;
            set => _headerBorderColor = value ?? Default.HeaderBorderColor;
        }

        public RgbColor FooterBorderColor
        {
            get => _footerBorderColor ??= Default.FooterBorderColor;
            set => _footerBorderColor = value ?? Default.FooterBorderColor;
        }

        public RgbColor PrimaryKeyHeaderBorderColor
        {
            get => _primaryKeyHeaderBorderColor ??= Default.PrimaryKeyHeaderBorderColor;
            set => _primaryKeyHeaderBorderColor = value ?? Default.PrimaryKeyHeaderBorderColor;
        }

        public RgbColor UniqueKeyHeaderBorderColor
        {
            get => _uniqueKeyHeaderBorderColor ??= Default.UniqueKeyHeaderBorderColor;
            set => _uniqueKeyHeaderBorderColor = value ?? Default.UniqueKeyHeaderBorderColor;
        }

        public RgbColor ForeignKeyHeaderBorderColor
        {
            get => _foreignKeyHeaderBorderColor ??= Default.ForeignKeyHeaderBorderColor;
            set => _foreignKeyHeaderBorderColor = value ?? Default.ForeignKeyHeaderBorderColor;
        }

        public RgbColor HighlightedTableBorderColor
        {
            get => _highlightedTableBorderColor ??= Default.HighlightedTableBorderColor;
            set => _highlightedTableBorderColor = value ?? Default.HighlightedTableBorderColor;
        }

        public RgbColor HighlightedHeaderBorderColor
        {
            get => _highlightedHeaderBorderColor ??= Default.HighlightedHeaderBorderColor;
            set => _highlightedHeaderBorderColor = value ?? Default.HighlightedHeaderBorderColor;
        }

        public RgbColor HighlightedFooterBorderColor
        {
            get => _highlightedFooterBorderColor ??= Default.HighlightedFooterBorderColor;
            set => _highlightedFooterBorderColor = value ?? Default.HighlightedFooterBorderColor;
        }

        public RgbColor HighlightedPrimaryKeyHeaderBorderColor
        {
            get => _highlightedPrimaryKeyHeaderBorderColor ??= Default.HighlightedPrimaryKeyHeaderBorderColor;
            set => _highlightedPrimaryKeyHeaderBorderColor = value ?? Default.HighlightedPrimaryKeyHeaderBorderColor;
        }

        public RgbColor HighlightedUniqueKeyHeaderBorderColor
        {
            get => _highlightedUniqueKeyHeaderBorderColor ??= Default.HighlightedUniqueKeyHeaderBorderColor;
            set => _highlightedUniqueKeyHeaderBorderColor = value ?? Default.HighlightedUniqueKeyHeaderBorderColor;
        }

        public RgbColor HighlightedForeignKeyHeaderBorderColor
        {
            get => _highlightedForeignKeyHeaderBorderColor ??= Default.HighlightedForeignKeyHeaderBorderColor;
            set => _highlightedForeignKeyHeaderBorderColor = value ?? Default.HighlightedForeignKeyHeaderBorderColor;
        }

        private RgbColor _backgroundColor = Default.BackgroundColor;
        private RgbColor _edgeColor = Default.EdgeColor;

        private RgbColor _tableForegroundColor = Default.TableForegroundColor;
        private RgbColor _headerForegroundColor = Default.HeaderForegroundColor;
        private RgbColor _footerForegroundColor = Default.FooterForegroundColor;
        private RgbColor _primaryKeyHeaderForegroundColor = Default.PrimaryKeyHeaderBackgroundColor;
        private RgbColor _uniqueKeyHeaderForegroundColor = Default.UniqueKeyHeaderForegroundColor;
        private RgbColor _foreignKeyHeaderForegroundColor = Default.ForeignKeyHeaderForegroundColor;
        private RgbColor _highlightedTableForegroundColor = Default.HighlightedTableForegroundColor;
        private RgbColor _highlightedHeaderForegroundColor = Default.HighlightedHeaderForegroundColor;
        private RgbColor _highlightedFooterForegroundColor = Default.HighlightedFooterForegroundColor;
        private RgbColor _highlightedPrimaryKeyHeaderForegroundColor = Default.HighlightedPrimaryKeyHeaderForegroundColor;
        private RgbColor _highlightedUniqueKeyHeaderForegroundColor = Default.HighlightedUniqueKeyHeaderForegroundColor;
        private RgbColor _highlightedForeignKeyHeaderForegroundColor = Default.HighlightedForeignKeyHeaderForegroundColor;

        private RgbColor _tableBackgroundColor = Default.TableBackgroundColor;
        private RgbColor _headerBackgroundColor = Default.HeaderBackgroundColor;
        private RgbColor _footerBackgroundColor = Default.FooterBackgroundColor;
        private RgbColor _primaryKeyHeaderBackgroundColor = Default.PrimaryKeyHeaderBackgroundColor;
        private RgbColor _uniqueKeyHeaderBackgroundColor = Default.UniqueKeyHeaderBackgroundColor;
        private RgbColor _foreignKeyHeaderBackgroundColor = Default.ForeignKeyHeaderBackgroundColor;
        private RgbColor _highlightedTableBackgroundColor = Default.HighlightedTableBackgroundColor;
        private RgbColor _highlightedHeaderBackgroundColor = Default.HighlightedHeaderBackgroundColor;
        private RgbColor _highlightedFooterBackgroundColor = Default.HighlightedFooterBackgroundColor;
        private RgbColor _highlightedPrimaryKeyHeaderBackgroundColor = Default.HighlightedPrimaryKeyHeaderBackgroundColor;
        private RgbColor _highlightedUniqueKeyHeaderBackgroundColor = Default.HighlightedUniqueKeyHeaderBackgroundColor;
        private RgbColor _highlightedForeignKeyHeaderBackgroundColor = Default.HighlightedForeignKeyHeaderBackgroundColor;

        private RgbColor _tableBorderColor = Default.TableBorderColor;
        private RgbColor _headerBorderColor = Default.HeaderBorderColor;
        private RgbColor _footerBorderColor = Default.FooterBorderColor;
        private RgbColor _primaryKeyHeaderBorderColor = Default.PrimaryKeyHeaderBorderColor;
        private RgbColor _uniqueKeyHeaderBorderColor = Default.UniqueKeyHeaderBorderColor;
        private RgbColor _foreignKeyHeaderBorderColor = Default.ForeignKeyHeaderBorderColor;
        private RgbColor _highlightedTableBorderColor = Default.HighlightedTableBorderColor;
        private RgbColor _highlightedHeaderBorderColor = Default.HighlightedHeaderBorderColor;
        private RgbColor _highlightedFooterBorderColor = Default.HighlightedFooterBorderColor;
        private RgbColor _highlightedPrimaryKeyHeaderBorderColor = Default.HighlightedPrimaryKeyHeaderBorderColor;
        private RgbColor _highlightedUniqueKeyHeaderBorderColor = Default.HighlightedUniqueKeyHeaderBorderColor;
        private RgbColor _highlightedForeignKeyHeaderBorderColor = Default.HighlightedForeignKeyHeaderBorderColor;

        private static readonly IGraphTheme Default = new DefaultTheme();
    }
}
