using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class UserDefinedTypeModelMapper
{
    /// <summary>
    /// Maps a user-defined type to its detail payload.
    /// </summary>
    /// <param name="userDefinedType">The type to map.</param>
    /// <param name="userDefinedTypeTargets">Resolves the type's base type, and each attribute's declared type, to the page of the user-defined type it names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="userDefinedType"/> holds no type, or <paramref name="userDefinedTypeTargets"/> is <see langword="null" />.</exception>
    public UserDefinedType Map(DatabaseUserDefinedType userDefinedType, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        if (userDefinedType.Value is null)
            throw new ArgumentNullException(nameof(userDefinedType));
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

        var details = UserDefinedTypeDetails.From(userDefinedType);

        var attributes = details.Attributes
            .Select((attribute, i) => new UserDefinedType.Attribute(
                attribute.Name.LocalName,
                i + 1,
                attribute.IsNullable,
                attribute.Type.Definition,
                userDefinedTypeTargets.GetTypeUrl(attribute.Type),
                attribute.DefaultValue
            ))
            .ToList();

        var checks = details.Checks
            .Select(static check => new UserDefinedType.Check(check.Name, check.Definition))
            .ToList();

        return new UserDefinedType(
            userDefinedType.Name,
            UserDefinedTypeKindNames.GetName(userDefinedType),
            details.BaseType,
            details.BaseType.Bind(baseType => userDefinedTypeTargets.GetTypeUrl(baseType)),
            details.IsNullable,
            details.DefaultValue,
            details.Definition,
            details.EnumValues.ToList(),
            attributes,
            checks
        );
    }
}
