using System;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

// Column types built around the name they declare, because the type-link mappers resolve a type by
// name. TestDbTypes is for the opposite case: a test that needs a type but does not exercise one.
internal static class UserDefinedDbTypes
{
    /// <summary>A scalar type of the given name, whose definition is the name as it is displayed.</summary>
    public static IDbType Named(Identifier typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return new ColumnDataType(
            typeName,
            DataType.Unknown,
            typeName.ToVisibleName(),
            typeof(object),
            false,
            -1,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );
    }

    /// <summary>An array type of the given name, holding elements of the given type.</summary>
    public static IDbType ArrayOf(Identifier arrayTypeName, IDbType elementType)
    {
        ArgumentNullException.ThrowIfNull(arrayTypeName);
        ArgumentNullException.ThrowIfNull(elementType);

        return new ColumnDataType(
            arrayTypeName,
            DataType.Array,
            elementType.Definition + "[]",
            typeof(object),
            false,
            -1,
            Option<INumericPrecision>.None,
            Option<Identifier>.None,
            Option<IDbType>.Some(elementType),
            [],
            Option<IDbType>.None,
            false
        );
    }
}
