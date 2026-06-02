namespace SJP.Schematic.Dot.Themes;

/// <summary>
/// A dark counterpart to <see cref="DefaultTheme"/>. Backgrounds are darkened to sit on a near-black
/// surface, the pastel header/key accents become deeper saturated hues, and text/borders flip to light
/// tones. The accent <em>positions</em> mirror <see cref="DefaultTheme"/> so the two themes pair up
/// property-for-property — e.g. when generating a light → dark recolouring for an embedded SVG.
/// </summary>
internal sealed class DarkTheme : IGraphTheme
{
    public RgbColor BackgroundColor { get; } = new RgbColor("#1E1E1E");

    public RgbColor EdgeColor { get; } = new RgbColor("#6F6F6F");

    public RgbColor TableForegroundColor { get; } = new RgbColor("#E6E6E6");

    public RgbColor HeaderForegroundColor { get; } = new RgbColor("#E6E6E6");

    public RgbColor FooterForegroundColor { get; } = new RgbColor("#E6E6E6");

    public RgbColor PrimaryKeyHeaderForegroundColor { get; } = new RgbColor("#E6E6E6");

    public RgbColor UniqueKeyHeaderForegroundColor { get; } = new RgbColor("#E6E6E6");

    public RgbColor ForeignKeyHeaderForegroundColor { get; } = new RgbColor("#E6E6E6");

    public RgbColor HighlightedTableForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedHeaderForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedFooterForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedPrimaryKeyHeaderForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedUniqueKeyHeaderForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor HighlightedForeignKeyHeaderForegroundColor { get; } = new RgbColor("#FFFFFF");

    public RgbColor TableBackgroundColor { get; } = new RgbColor("#1E1E1E");

    public RgbColor HeaderBackgroundColor { get; } = new RgbColor("#2F4F37");

    public RgbColor FooterBackgroundColor { get; } = new RgbColor("#2F4F37");

    public RgbColor PrimaryKeyHeaderBackgroundColor { get; } = new RgbColor("#4F4A1F");

    public RgbColor UniqueKeyHeaderBackgroundColor { get; } = new RgbColor("#2E4651");

    public RgbColor ForeignKeyHeaderBackgroundColor { get; } = new RgbColor("#3A3A3A");

    public RgbColor HighlightedTableBackgroundColor { get; } = new RgbColor("#1E1E1E");

    public RgbColor HighlightedHeaderBackgroundColor { get; } = new RgbColor("#3A7D4E");

    public RgbColor HighlightedFooterBackgroundColor { get; } = new RgbColor("#3A7D4E");

    public RgbColor HighlightedPrimaryKeyHeaderBackgroundColor { get; } = new RgbColor("#6B6410");

    public RgbColor HighlightedUniqueKeyHeaderBackgroundColor { get; } = new RgbColor("#3F6D85");

    public RgbColor HighlightedForeignKeyHeaderBackgroundColor { get; } = new RgbColor("#5A5A5A");

    public RgbColor TableBorderColor { get; } = new RgbColor("#6F6F6F");

    public RgbColor HeaderBorderColor { get; } = new RgbColor("#6F6F6F");

    public RgbColor FooterBorderColor { get; } = new RgbColor("#6F6F6F");

    public RgbColor PrimaryKeyHeaderBorderColor { get; } = new RgbColor("#6F6F6F");

    public RgbColor UniqueKeyHeaderBorderColor { get; } = new RgbColor("#6F6F6F");

    public RgbColor ForeignKeyHeaderBorderColor { get; } = new RgbColor("#6F6F6F");

    public RgbColor HighlightedTableBorderColor { get; } = new RgbColor("#8A8A8A");

    public RgbColor HighlightedHeaderBorderColor { get; } = new RgbColor("#8A8A8A");

    public RgbColor HighlightedFooterBorderColor { get; } = new RgbColor("#8A8A8A");

    public RgbColor HighlightedPrimaryKeyHeaderBorderColor { get; } = new RgbColor("#8A8A8A");

    public RgbColor HighlightedUniqueKeyHeaderBorderColor { get; } = new RgbColor("#8A8A8A");

    public RgbColor HighlightedForeignKeyHeaderBorderColor { get; } = new RgbColor("#8A8A8A");
}
