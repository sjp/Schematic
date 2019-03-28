namespace SJP.Schematic.Reporting.Dot.Themes
{
    public interface IGraphTheme
    {
        RgbColor BackgroundColor { get; }
        RgbColor EdgeColor { get; }
        RgbColor TableForegroundColor { get; }
        RgbColor HeaderForegroundColor { get; }
        RgbColor FooterForegroundColor { get; }
        RgbColor PrimaryKeyHeaderForegroundColor { get; }
        RgbColor UniqueKeyHeaderForegroundColor { get; }
        RgbColor ForeignKeyHeaderForegroundColor { get; }
        RgbColor HighlightedTableForegroundColor { get; }
        RgbColor HighlightedHeaderForegroundColor { get; }
        RgbColor HighlightedFooterForegroundColor { get; }
        RgbColor HighlightedPrimaryKeyHeaderForegroundColor { get; }
        RgbColor HighlightedUniqueKeyHeaderForegroundColor { get; }
        RgbColor HighlightedForeignKeyHeaderForegroundColor { get; }
        RgbColor TableBackgroundColor { get; }
        RgbColor HeaderBackgroundColor { get; }
        RgbColor FooterBackgroundColor { get; }
        RgbColor PrimaryKeyHeaderBackgroundColor { get; }
        RgbColor UniqueKeyHeaderBackgroundColor { get; }
        RgbColor ForeignKeyHeaderBackgroundColor { get; }
        RgbColor HighlightedTableBackgroundColor { get; }
        RgbColor HighlightedHeaderBackgroundColor { get; }
        RgbColor HighlightedFooterBackgroundColor { get; }
        RgbColor HighlightedPrimaryKeyHeaderBackgroundColor { get; }
        RgbColor HighlightedUniqueKeyHeaderBackgroundColor { get; }
        RgbColor HighlightedForeignKeyHeaderBackgroundColor { get; }
        RgbColor TableBorderColor { get; }
        RgbColor HeaderBorderColor { get; }
        RgbColor FooterBorderColor { get; }
        RgbColor PrimaryKeyHeaderBorderColor { get; }
        RgbColor UniqueKeyHeaderBorderColor { get; }
        RgbColor ForeignKeyHeaderBorderColor { get; }
        RgbColor HighlightedTableBorderColor { get; }
        RgbColor HighlightedHeaderBorderColor { get; }
        RgbColor HighlightedFooterBorderColor { get; }
        RgbColor HighlightedPrimaryKeyHeaderBorderColor { get; }
        RgbColor HighlightedUniqueKeyHeaderBorderColor { get; }
        RgbColor HighlightedForeignKeyHeaderBorderColor { get; }
    }
}