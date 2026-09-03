using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Display names for the identity generation strategies reported by the dialects.
/// </summary>
internal static class IdentityGenerationNames
{
    /// <summary>
    /// Describes a generation strategy for display. An unknown strategy has no name, so that a
    /// database that does not report one shows nothing instead of a placeholder.
    /// </summary>
    /// <param name="generation">A generation strategy.</param>
    /// <returns>A display name, or an empty string when the strategy is unknown.</returns>
    public static string GetName(IdentityGeneration generation)
    {
        return generation switch
        {
            IdentityGeneration.ByDefault => "By Default",
            IdentityGeneration.ByDefaultOnNull => "By Default On Null",
            IdentityGeneration.Always => "Always",
            _ => string.Empty,
        };
    }
}
