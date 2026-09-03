using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Display names for the storage applied to a computed column.
/// </summary>
internal static class ComputedColumnStorageNames
{
    /// <summary>
    /// Describes the storage of a computed column for display. Unknown storage has no name, so that
    /// a database that does not report one shows nothing instead of a placeholder.
    /// </summary>
    /// <param name="storage">The storage applied to a computed column.</param>
    /// <returns>A display name, or an empty string when the storage is unknown.</returns>
    public static string GetName(ComputedColumnStorage storage)
    {
        return storage switch
        {
            ComputedColumnStorage.Virtual => "Virtual",
            ComputedColumnStorage.Stored => "Stored",
            _ => string.Empty,
        };
    }
}
