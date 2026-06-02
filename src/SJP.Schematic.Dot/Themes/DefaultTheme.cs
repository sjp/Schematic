namespace SJP.Schematic.Dot.Themes;

/// <summary>
/// The default light theme. Tables sit on a faint slate canvas with white bodies; the header/footer form a
/// blue "chrome", and the key sections use distinct accent hues (amber for primary, violet for unique, grey
/// for foreign) so none of them collide with the header blue. Borders and edges are a soft slate rather than
/// black. The focal (highlighted) table uses punchier versions of the same hues.
/// </summary>
internal sealed class DefaultTheme : IGraphTheme
{
    public RgbColor BackgroundColor { get; } = new RgbColor("#F8FAFC");

    public RgbColor EdgeColor { get; } = new RgbColor("#64748B");

    public RgbColor TableForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor HeaderForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor FooterForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor PrimaryKeyHeaderForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor UniqueKeyHeaderForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor ForeignKeyHeaderForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor HighlightedTableForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor HighlightedHeaderForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor HighlightedFooterForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor HighlightedPrimaryKeyHeaderForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor HighlightedUniqueKeyHeaderForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor HighlightedForeignKeyHeaderForegroundColor { get; } = new RgbColor("#000000");

    public RgbColor TableBackgroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HeaderBackgroundColor { get; } = new RgbColor("#BBDEFB");

    public RgbColor FooterBackgroundColor { get; } = new RgbColor("#E3F0FB");

    public RgbColor PrimaryKeyHeaderBackgroundColor { get; } = new RgbColor("#FDE68A");

    public RgbColor UniqueKeyHeaderBackgroundColor { get; } = new RgbColor("#DDD6FE");

    public RgbColor ForeignKeyHeaderBackgroundColor { get; } = new RgbColor("#E5E7EB");

    public RgbColor HighlightedTableBackgroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedHeaderBackgroundColor { get; } = new RgbColor("#64B5F6");

    public RgbColor HighlightedFooterBackgroundColor { get; } = new RgbColor("#90CAF9");

    public RgbColor HighlightedPrimaryKeyHeaderBackgroundColor { get; } = new RgbColor("#FCD34D");

    public RgbColor HighlightedUniqueKeyHeaderBackgroundColor { get; } = new RgbColor("#C4B5FD");

    public RgbColor HighlightedForeignKeyHeaderBackgroundColor { get; } = new RgbColor("#D1D5DB");

    public RgbColor TableBorderColor { get; } = new RgbColor("#94A3B8");

    public RgbColor HeaderBorderColor { get; } = new RgbColor("#94A3B8");

    public RgbColor FooterBorderColor { get; } = new RgbColor("#94A3B8");

    public RgbColor PrimaryKeyHeaderBorderColor { get; } = new RgbColor("#94A3B8");

    public RgbColor UniqueKeyHeaderBorderColor { get; } = new RgbColor("#94A3B8");

    public RgbColor ForeignKeyHeaderBorderColor { get; } = new RgbColor("#94A3B8");

    public RgbColor HighlightedTableBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedHeaderBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedFooterBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedPrimaryKeyHeaderBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedUniqueKeyHeaderBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedForeignKeyHeaderBorderColor { get; } = new RgbColor("#64748B");
}
