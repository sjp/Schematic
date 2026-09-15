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
    /// <exception cref="ArgumentNullException"><paramref name="userDefinedType"/> or <paramref name="userDefinedTypeTargets"/> is <see langword="null" />.</exception>
    public UserDefinedType Map(IDatabaseUserDefinedType userDefinedType, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        ArgumentNullException.ThrowIfNull(userDefinedType);
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

        var attributes = userDefinedType.Attributes
            .Select((attribute, i) => new UserDefinedType.Attribute(
                attribute.Name.LocalName,
                i + 1,
                attribute.IsNullable,
                attribute.Type.Definition,
                userDefinedTypeTargets.GetTypeUrl(attribute.Type),
                attribute.DefaultValue
            ))
            .ToList();

        var checks = userDefinedType.Checks
            .Select(static check => new UserDefinedType.Check(check.Name, check.Definition))
            .ToList();

        return new UserDefinedType(
            userDefinedType.Name,
            userDefinedType.Kind,
            userDefinedType.BaseType,
            userDefinedType.BaseType.Bind(baseType => userDefinedTypeTargets.GetTypeUrl(baseType)),
            userDefinedType.IsNullable,
            userDefinedType.DefaultValue,
            userDefinedType.Definition,
            userDefinedType.EnumValues.ToList(),
            attributes,
            checks
        );
    }
}
