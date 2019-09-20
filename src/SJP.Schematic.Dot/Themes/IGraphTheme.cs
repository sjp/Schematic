namespace SJP.Schematic.Dot.Themes
{
    /// <summary>
    /// Components that describe a theme for customizing the display of a DOT graph
    /// </summary>
    public interface IGraphTheme
    {
        /// <summary>
        /// The background color to apply to the entire graph
        /// </summary>
        RgbColor BackgroundColor { get; }

        /// <summary>
        /// The color to apply to any edges (i.e. relationships).
        /// </summary>
        RgbColor EdgeColor { get; }

        /// <summary>
        /// The color of the text within a table
        /// </summary>
        RgbColor TableForegroundColor { get; }

        /// <summary>
        /// The color of the text within a table header
        /// </summary>
        RgbColor HeaderForegroundColor { get; }

        /// <summary>
        /// The color of the text within a table footer
        /// </summary>
        RgbColor FooterForegroundColor { get; }

        /// <summary>
        /// The color of the text within a primary key section header
        /// </summary>
        RgbColor PrimaryKeyHeaderForegroundColor { get; }

        /// <summary>
        /// The color of the text within a unique key section header
        /// </summary>
        RgbColor UniqueKeyHeaderForegroundColor { get; }

        /// <summary>
        /// The color of the text within a foreign key section header
        /// </summary>
        RgbColor ForeignKeyHeaderForegroundColor { get; }

        /// <summary>
        /// The color of the text within a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedTableForegroundColor { get; }

        /// <summary>
        /// The color of the text within a table header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedHeaderForegroundColor { get; }

        /// <summary>
        /// The color of the text within a table footer for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedFooterForegroundColor { get; }

        /// <summary>
        /// The color of the text within a primary key header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedPrimaryKeyHeaderForegroundColor { get; }

        /// <summary>
        /// The color of the text within a unique key header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedUniqueKeyHeaderForegroundColor { get; }

        /// <summary>
        /// The color of the text within a foreign key header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedForeignKeyHeaderForegroundColor { get; }

        /// <summary>
        /// The background color of a table
        /// </summary>
        RgbColor TableBackgroundColor { get; }

        /// <summary>
        /// The background color for a table header
        /// </summary>
        RgbColor HeaderBackgroundColor { get; }

        /// <summary>
        /// The background color for a table footer
        /// </summary>
        RgbColor FooterBackgroundColor { get; }

        /// <summary>
        /// The background color for a primary key header
        /// </summary>
        RgbColor PrimaryKeyHeaderBackgroundColor { get; }

        /// <summary>
        /// The background color for a unique key header
        /// </summary>
        RgbColor UniqueKeyHeaderBackgroundColor { get; }

        /// <summary>
        /// The background color for a foreign key header
        /// </summary>
        RgbColor ForeignKeyHeaderBackgroundColor { get; }

        /// <summary>
        /// The background color for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedTableBackgroundColor { get; }

        /// <summary>
        /// The background color of a table header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedHeaderBackgroundColor { get; }

        /// <summary>
        /// The background color of a table footer for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedFooterBackgroundColor { get; }

        /// <summary>
        /// The background color of a primary key header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedPrimaryKeyHeaderBackgroundColor { get; }

        /// <summary>
        /// The background color of a unique key header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedUniqueKeyHeaderBackgroundColor { get; }

        /// <summary>
        /// The background color of a foreign key header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedForeignKeyHeaderBackgroundColor { get; }

        /// <summary>
        /// The color of a border applied to a table
        /// </summary>
        RgbColor TableBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a table header
        /// </summary>
        RgbColor HeaderBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a table footer
        /// </summary>
        RgbColor FooterBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a primary key header section
        /// </summary>
        RgbColor PrimaryKeyHeaderBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a unique key header section
        /// </summary>
        RgbColor UniqueKeyHeaderBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a foreign key header section
        /// </summary>
        RgbColor ForeignKeyHeaderBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a table for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedTableBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a table header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedHeaderBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a table footer for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedFooterBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a primary key header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedPrimaryKeyHeaderBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a unique key header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedUniqueKeyHeaderBorderColor { get; }

        /// <summary>
        /// The color of a border applied to a foreign key header for a table that is highlighted (if available)
        /// </summary>
        RgbColor HighlightedForeignKeyHeaderBorderColor { get; }
    }
}