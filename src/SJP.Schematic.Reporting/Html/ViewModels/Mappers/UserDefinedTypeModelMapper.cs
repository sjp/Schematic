using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class UserDefinedTypeModelMapper
{
    public UserDefinedType Map(IDatabaseUserDefinedType userDefinedType)
    {
        ArgumentNullException.ThrowIfNull(userDefinedType);

        var attributes = userDefinedType.Attributes
            .Select(static (attribute, i) => new UserDefinedType.Attribute(
                attribute.Name.LocalName,
                i + 1,
                attribute.IsNullable,
                attribute.Type.Definition,
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
            userDefinedType.IsNullable,
            userDefinedType.DefaultValue,
            userDefinedType.Definition,
            userDefinedType.EnumValues.ToList(),
            attributes,
            checks
        );
    }
}
