using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Display names for the index structures reported by the dialects.
/// </summary>
internal static class IndexTypeNames
{
    /// <summary>
    /// Describes an index structure for display. An unknown structure has no name, so that a
    /// database that does not report one shows nothing instead of a placeholder.
    /// </summary>
    /// <param name="indexType">An index structure.</param>
    /// <returns>A display name, or an empty string when the structure is unknown.</returns>
    public static string GetName(IndexType indexType)
    {
        return indexType switch
        {
            IndexType.BTree => "B-Tree",
            IndexType.Clustered => "Clustered",
            IndexType.Hash => "Hash",
            IndexType.Bitmap => "Bitmap",
            IndexType.ColumnStore => "Columnstore",
            IndexType.FullText => "Full-Text",
            IndexType.Spatial => "Spatial",
            IndexType.Xml => "XML",
            IndexType.Gin => "GIN",
            IndexType.Gist => "GiST",
            IndexType.Brin => "BRIN",
            IndexType.Other => "Other",
            _ => string.Empty,
        };
    }
}
