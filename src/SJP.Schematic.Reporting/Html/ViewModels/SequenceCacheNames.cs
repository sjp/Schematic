using System.Globalization;
using LanguageExt;
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
    /// <param name="cacheMode">The cache mode reported for the sequence.</param>
    /// <param name="cacheSize">The number of pre-allocated values, when the size is known.</param>
    /// <returns>A display name, or an empty string when the caching is unknown.</returns>
    public static string GetName(SequenceCacheMode cacheMode, Option<int> cacheSize)
    {
        return cacheMode switch
        {
            SequenceCacheMode.None => "None",
            SequenceCacheMode.EngineDefault => "Database default",
            SequenceCacheMode.Sized => cacheSize.Match(
                static size => size.ToString(CultureInfo.InvariantCulture),
                static () => string.Empty
            ),
            _ => string.Empty,
        };
    }
}
