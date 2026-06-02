namespace SJP.Schematic.Dot.Themes;

/// <summary>
/// The built-in graph themes. Exposes the otherwise-internal theme implementations so callers (e.g. a
/// report renderer) can pair a light theme with its dark counterpart without re-declaring the palette.
/// </summary>
public static class GraphThemes
{
    /// <summary>
    /// The default light theme: white surfaces with pastel header and key-section accents.
    /// </summary>
    public static IGraphTheme Default { get; } = new DefaultTheme();

    /// <summary>
    /// The dark counterpart to <see cref="Default"/>, intended for use under a dark colour scheme.
    /// </summary>
    public static IGraphTheme Dark { get; } = new DarkTheme();
}
