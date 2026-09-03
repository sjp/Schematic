using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// Maps the raw <c>ALL_TYPES</c> rows returned by the Oracle catalog onto the core model. The 'all
/// types' and 'single type' queries project the same columns, so both paths share this mapping.
/// </summary>
internal static class OracleUserDefinedTypeMapper
{
    private const string ObjectTypeCode = "OBJECT";
    private const string CollectionTypeCode = "COLLECTION";

    public static IDatabaseUserDefinedType MapType(
        Identifier typeName,
        IUserDefinedTypeDefinitionRow row,
        IReadOnlyList<IDatabaseColumn> attributes,
        Option<string> definition,
        IDbTypeProvider typeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(typeName);
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(attributes);
        ArgumentNullException.ThrowIfNull(typeProvider);

        var kind = row.TypeCode switch
        {
            ObjectTypeCode => UserDefinedTypeKind.Composite,
            CollectionTypeCode => UserDefinedTypeKind.Collection,
            _ => UserDefinedTypeKind.Unknown,
        };

        // only a collection type names an element type, which becomes the type it is defined over
        var baseType = !row.ElementTypeName.IsNullOrWhiteSpace()
            ? Option<IDbType>.Some(typeProvider.CreateColumnType(new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(row.ElementTypeSchema, row.ElementTypeName),
                MaxLength = row.ElementLength,
                NumericPrecision = row.ElementPrecision > 0 || row.ElementScale > 0
                    ? Option<INumericPrecision>.Some(new NumericPrecision(row.ElementPrecision, row.ElementScale))
                    : Option<INumericPrecision>.None,
                Collation = !row.ElementCollation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.ElementCollation))
                    : Option<Identifier>.None,
            }))
            : Option<IDbType>.None;

        return new DatabaseUserDefinedType(
            typeName,
            kind,
            baseType,
            [],
            attributes,
            // Oracle constrains an object type through the methods declared on it, never through a
            // check constraint, so there is nothing to report here
            [],
            // an Oracle type is always nullable; there is no way to declare otherwise
            true,
            Option<string>.None,
            definition
        );
    }

    public static IReadOnlyList<IDatabaseColumn> MapAttributes(IEnumerable<IUserDefinedTypeAttributeRow> rows, IDbTypeProvider typeProvider)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(typeProvider);

        return rows.Select(row =>
        {
            var typeMetadata = new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(row.AttributeTypeSchema, row.AttributeTypeName),
                MaxLength = row.DataLength,
                NumericPrecision = row.Precision > 0 || row.Scale > 0
                    ? Option<INumericPrecision>.Some(new NumericPrecision(row.Precision, row.Scale))
                    : Option<INumericPrecision>.None,
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
            };

            // ALL_TYPE_ATTRS records neither a nullability rule nor a default for an attribute
            return new OracleDatabaseColumn(
                Identifier.CreateQualifiedIdentifier(row.AttributeName!),
                typeProvider.CreateColumnType(typeMetadata),
                true,
                Option<IDatabaseDefaultValue>.None);
        }).ToList();
    }
}
