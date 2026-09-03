using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Display names for the table kinds reported by the dialects.
/// </summary>
internal static class TableKindNames
{
    /// <summary>
    /// Describes a table kind for display. An ordinary table has no name, so that the common case
    /// shows nothing instead of a badge that says nothing.
    /// </summary>
    /// <param name="kind">A table kind.</param>
    /// <returns>A display name, or an empty string for an ordinary table.</returns>
    public static string GetName(TableKind kind)
    {
        return kind switch
        {
            TableKind.Temporary => "Temporary",
            TableKind.PartitionParent => "Partitioned",
            TableKind.Partition => "Partition",
            TableKind.History => "History",
            TableKind.Virtual => "Virtual",
            TableKind.External => "External",
            TableKind.IndexOrganized => "Index-Organized",
            _ => string.Empty,
        };
    }
}
