using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Display names for the kinds of type a user can declare.
/// </summary>
internal static class UserDefinedTypeKindNames
{
    /// <summary>
    /// Describes a user-defined type's kind for display. A dialect that does not say what kind of
    /// type it reported gets no name, so that nothing is shown in place of a kind that is not known.
    /// </summary>
    /// <param name="userDefinedType">The type whose kind is described.</param>
    /// <returns>A display name, or an empty string when the kind is unknown.</returns>
    public static string GetName(DatabaseUserDefinedType userDefinedType) => userDefinedType switch
    {
        DatabaseUserDefinedType.Unknown => string.Empty,
        DatabaseUserDefinedType.Alias => "Alias",
        DatabaseUserDefinedType.Domain => "Domain",
        DatabaseUserDefinedType.Enum => "Enum",
        DatabaseUserDefinedType.Composite => "Composite",
        DatabaseUserDefinedType.Range => "Range",
        DatabaseUserDefinedType.Table => "Table",
        DatabaseUserDefinedType.Clr => "CLR",
        DatabaseUserDefinedType.Collection => "Collection",
    };
}
