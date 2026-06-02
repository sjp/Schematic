namespace SJP.Schematic.Dot.Themes;

/// <summary>
/// The dark counterpart to <see cref="DefaultTheme"/>. Tables sit on a deep slate canvas, the blue header
/// chrome and amber/violet/grey key accents are darkened to readable mid-tones, and text flips to a light
/// tone with soft slate borders/edges. The accent <em>positions</em> mirror <see cref="DefaultTheme"/> so the
/// two themes pair up property-for-property — e.g. when generating a light → dark recolouring for an
/// embedded SVG.
/// </summary>
internal sealed class DarkTheme : IGraphTheme
{
    public RgbColor BackgroundColor { get; } = new RgbColor("#0F172A");

    public RgbColor EdgeColor { get; } = new RgbColor("#64748B");

    public RgbColor TableForegroundColor { get; } = new RgbColor("#E6EDF3");

    public RgbColor HeaderForegroundColor { get; } = new RgbColor("#E6EDF3");

    public RgbColor FooterForegroundColor { get; } = new RgbColor("#E6EDF3");

    public RgbColor PrimaryKeyHeaderForegroundColor { get; } = new RgbColor("#E6EDF3");

    public RgbColor UniqueKeyHeaderForegroundColor { get; } = new RgbColor("#E6EDF3");

    public RgbColor ForeignKeyHeaderForegroundColor { get; } = new RgbColor("#E6EDF3");

    public RgbColor HighlightedTableForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedHeaderForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedFooterForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedPrimaryKeyHeaderForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedUniqueKeyHeaderForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedForeignKeyHeaderForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor TableBackgroundColor { get; } = new RgbColor("#1E293B");

    public RgbColor HeaderBackgroundColor { get; } = new RgbColor("#2C5282");

    public RgbColor FooterBackgroundColor { get; } = new RgbColor("#21425E");

    public RgbColor PrimaryKeyHeaderBackgroundColor { get; } = new RgbColor("#4D4A1A");

    public RgbColor UniqueKeyHeaderBackgroundColor { get; } = new RgbColor("#3C3470");

    public RgbColor ForeignKeyHeaderBackgroundColor { get; } = new RgbColor("#374151");

    public RgbColor HighlightedTableBackgroundColor { get; } = new RgbColor("#1E293B");

    public RgbColor HighlightedHeaderBackgroundColor { get; } = new RgbColor("#3B82F6");

    public RgbColor HighlightedFooterBackgroundColor { get; } = new RgbColor("#2F6FBF");

    public RgbColor HighlightedPrimaryKeyHeaderBackgroundColor { get; } = new RgbColor("#6B6410");

    public RgbColor HighlightedUniqueKeyHeaderBackgroundColor { get; } = new RgbColor("#5B4FBF");

    public RgbColor HighlightedForeignKeyHeaderBackgroundColor { get; } = new RgbColor("#4B5563");

    public RgbColor TableBorderColor { get; } = new RgbColor("#475569");

    public RgbColor HeaderBorderColor { get; } = new RgbColor("#475569");

    public RgbColor FooterBorderColor { get; } = new RgbColor("#475569");

    public RgbColor PrimaryKeyHeaderBorderColor { get; } = new RgbColor("#475569");

    public RgbColor UniqueKeyHeaderBorderColor { get; } = new RgbColor("#475569");

    public RgbColor ForeignKeyHeaderBorderColor { get; } = new RgbColor("#475569");

    public RgbColor HighlightedTableBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedHeaderBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedFooterBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedPrimaryKeyHeaderBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedUniqueKeyHeaderBorderColor { get; } = new RgbColor("#64748B");

    public RgbColor HighlightedForeignKeyHeaderBorderColor { get; } = new RgbColor("#64748B");
}
