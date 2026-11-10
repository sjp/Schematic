using System.Globalization;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Display names for the value caching reported for a sequence.
/// </summary>
internal static class SequenceCacheNames
{
    /// <summary>
    /// Describes a sequence's caching for display. A database that does not report how a sequence
    /// caches gets no name, so that nothing is shown in place of a size that is not known.
    /// </summary>
    /// <param name="cache">The cache reported for the sequence.</param>
    /// <returns>A display name, or an empty string when the caching is unknown.</returns>
    public static string GetName(SequenceCache cache) => cache switch
    {
        SequenceCache.Unknown => string.Empty,
        SequenceCache.None => "None",
        SequenceCache.Sized sized => sized.Size.ToString(CultureInfo.InvariantCulture),
        SequenceCache.EngineDefault => "Database default",
    };
}
