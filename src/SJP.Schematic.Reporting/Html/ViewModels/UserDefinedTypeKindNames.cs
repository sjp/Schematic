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
    /// <param name="kind">The kind of the type.</param>
    /// <returns>A display name, or an empty string when the kind is unknown.</returns>
    public static string GetName(UserDefinedTypeKind kind)
    {
        return kind switch
        {
            UserDefinedTypeKind.Alias => "Alias",
            UserDefinedTypeKind.Domain => "Domain",
            UserDefinedTypeKind.Enum => "Enum",
            UserDefinedTypeKind.Composite => "Composite",
            UserDefinedTypeKind.Range => "Range",
            UserDefinedTypeKind.Table => "Table",
            UserDefinedTypeKind.Clr => "CLR",
            UserDefinedTypeKind.Collection => "Collection",
            _ => string.Empty,
        };
    }
}
