using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Display names for the constraint state reported by the dialects.
/// </summary>
internal static class ConstraintStateNames
{
    /// <summary>
    /// Describes when a constraint is checked. A constraint that cannot be deferred has no name, so
    /// that the common case shows nothing instead of a placeholder.
    /// </summary>
    /// <param name="deferrability">A deferrability value.</param>
    /// <returns>A display name, or an empty string when the constraint cannot be deferred.</returns>
    public static string GetDeferrabilityName(ConstraintDeferrability deferrability)
    {
        return deferrability switch
        {
            ConstraintDeferrability.DeferrableInitiallyImmediate => "DEFERRABLE INITIALLY IMMEDIATE",
            ConstraintDeferrability.DeferrableInitiallyDeferred => "DEFERRABLE INITIALLY DEFERRED",
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Describes how a foreign key matches partially <c>null</c> child rows. The simple behaviour is
    /// both the SQL default and all that most dialects implement, so it has no name.
    /// </summary>
    /// <param name="matchType">A foreign key match type.</param>
    /// <returns>A display name, or an empty string for the default behaviour.</returns>
    public static string GetMatchTypeName(ForeignKeyMatchType matchType)
    {
        return matchType switch
        {
            ForeignKeyMatchType.Partial => "MATCH PARTIAL",
            ForeignKeyMatchType.Full => "MATCH FULL",
            _ => string.Empty,
        };
    }
}
