using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

/// <summary>
/// The parts of a user-defined type that the report shows, flattened out of whichever kind of type
/// it is. The report's payloads have one field for each part, so a part that a kind of type does not
/// have is left empty.
/// </summary>
/// <param name="BaseType">The type this type is defined in terms of: the aliased type of an alias or domain, the subtype of a range, or the element type of a collection.</param>
/// <param name="IsNullable">Whether a value of the type can be <see langword="null" />. A kind of type that declares no nullability rule is nullable.</param>
/// <param name="DefaultValue">The default declared for the type, for an alias or domain.</param>
/// <param name="Definition">The textual definition of the type, or the external name of a CLR type.</param>
/// <param name="EnumValues">The permitted values of an enum.</param>
/// <param name="Attributes">The attributes of a composite type, or the columns of a table type.</param>
/// <param name="Checks">The check constraints of a domain or table type.</param>
internal sealed record UserDefinedTypeDetails(
    Option<IDbType> BaseType,
    bool IsNullable,
    Option<string> DefaultValue,
    Option<string> Definition,
    IReadOnlyList<string> EnumValues,
    IReadOnlyList<IDatabaseColumn> Attributes,
    IReadOnlyCollection<IDatabaseCheckConstraint> Checks
)
{
    private static readonly UserDefinedTypeDetails None = new(Option<IDbType>.None, true, Option<string>.None, Option<string>.None, [], [], []);

    /// <summary>
    /// Flattens a user-defined type into the parts the report shows.
    /// </summary>
    /// <param name="userDefinedType">A user-defined type.</param>
    /// <returns>The parts of the type that the report shows.</returns>
    public static UserDefinedTypeDetails From(DatabaseUserDefinedType userDefinedType) => userDefinedType switch
    {
        DatabaseUserDefinedType.Unknown type => None with { Definition = type.Definition },
        DatabaseUserDefinedType.Alias type => None with
        {
            BaseType = Option<IDbType>.Some(type.AliasedType),
            IsNullable = type.IsNullable,
            DefaultValue = type.DefaultValue,
        },
        DatabaseUserDefinedType.Domain type => None with
        {
            BaseType = Option<IDbType>.Some(type.AliasedType),
            IsNullable = type.IsNullable,
            DefaultValue = type.DefaultValue,
            Checks = type.Checks,
        },
        DatabaseUserDefinedType.Enum type => None with { EnumValues = type.Values },
        DatabaseUserDefinedType.Composite type => None with { Attributes = type.Attributes, Definition = type.Definition },
        DatabaseUserDefinedType.Range type => None with { BaseType = Option<IDbType>.Some(type.Subtype) },
        DatabaseUserDefinedType.Table type => None with { Attributes = type.Columns, Checks = type.Checks },
        DatabaseUserDefinedType.Clr type => None with { Definition = type.ExternalName },
        DatabaseUserDefinedType.Collection type => None with
        {
            BaseType = Option<IDbType>.Some(type.ElementType),
            Definition = type.Definition,
        },
    };
}
