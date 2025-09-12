using System;
using SJP.Schematic.Core;
using SJP.Schematic.Dot.Themes;

namespace SJP.Schematic.Dot;

/// <summary>
/// Options for rendering a DOT graph
/// </summary>
public class DotRenderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether a column's data type should be visible.
    /// </summary>
    /// <value><see langword="true" /> when the column data type should be visible; otherwise <see langword="false" />.</value>
    public bool ShowColumnDataType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the set of visible columns should be truncated.
    /// </summary>
    /// <value><see langword="true" /> when truncation should occur; otherwise <see langword="false" />.</value>
    public bool IsReducedColumnSet { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether row counts should be shown.
    /// </summary>
    /// <value><see langword="true" /> when a table's row counts should be visible; otherwise <see langword="false" />.</value>
    public bool ShowRowCounts { get; set; } = true;

    /// <summary>
    /// Gets or sets the highlighted table.
    /// </summary>
    /// <value>
    /// The highlighted table.
    /// </value>
    public Identifier? HighlightedTable { get; set; }

    /// <summary>
    /// Gets or sets the theme.
    /// </summary>
    /// <value>A graph theme.</value>
    /// <exception cref="ArgumentNullException">value</exception>
    public IGraphTheme Theme
    {
        get => _theme;
        set => _theme = value ?? throw new ArgumentNullException(nameof(value));
    }

    private IGraphTheme _theme = new DefaultTheme();

    /// <summary>
    /// Gets the default theme.
    /// </summary>
    /// <value>The default theme.</value>
    public static DotRenderOptions Default => new();
}