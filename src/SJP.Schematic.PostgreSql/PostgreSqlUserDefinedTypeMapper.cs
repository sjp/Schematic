using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// Maps the raw <c>pg_type</c> rows returned by the PostgreSQL catalog onto the core model. The
/// 'all types' and 'single type' queries project the same columns, so both paths share this mapping.
/// </summary>
internal static class PostgreSqlUserDefinedTypeMapper
{
    private const string DomainTypeKind = "d";
    private const string EnumTypeKind = "e";
    private const string CompositeTypeKind = "c";
    private const string RangeTypeKind = "r";

    public static IDatabaseUserDefinedType MapType(
        Identifier typeName,
        IUserDefinedTypeDefinitionRow row,
        IReadOnlyList<IDatabaseColumn> attributes,
        IReadOnlyCollection<IDatabaseCheckConstraint> checks,
        IDbTypeProvider typeProvider
    )
    {
        ArgumentNullException.ThrowIfNull(typeName);
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(attributes);
        ArgumentNullException.ThrowIfNull(checks);
        ArgumentNullException.ThrowIfNull(typeProvider);

        var kind = row.TypeKind switch
        {
            DomainTypeKind => UserDefinedTypeKind.Domain,
            EnumTypeKind => UserDefinedTypeKind.Enum,
            CompositeTypeKind => UserDefinedTypeKind.Composite,
            RangeTypeKind => UserDefinedTypeKind.Range,
            _ => UserDefinedTypeKind.Unknown,
        };

        // only a domain and a range are defined over another type
        var baseType = !row.BaseTypeName.IsNullOrWhiteSpace()
            ? Option<IDbType>.Some(typeProvider.CreateColumnType(new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(row.BaseTypeSchema, row.BaseTypeName),
                MaxLength = PostgreSqlColumnTypeMetadata.CreateMaxLength(row.CharacterMaximumLength, row.NumericPrecision, row.NumericPrecisionRadix),
                NumericPrecision = PostgreSqlColumnTypeMetadata.CreateNumericPrecision(row.NumericPrecision, row.NumericScale, row.NumericPrecisionRadix),
                Collation = !row.CollationName.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.CollationName))
                    : Option<Identifier>.None,
            }))
            : Option<IDbType>.None;

        var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
            ? Option<string>.Some(row.DefaultValue)
            : Option<string>.None;

        return new DatabaseUserDefinedType(
            typeName,
            kind,
            baseType,
            row.EnumLabels ?? [],
            attributes,
            checks,
            !row.IsNotNull,
            defaultValue,
            // PostgreSQL stores no textual definition for a type, only the catalog rows describing it
            Option<string>.None
        );
    }

    public static IReadOnlyList<IDatabaseColumn> MapAttributes(IEnumerable<IUserDefinedTypeAttributeRow> rows, IDbTypeProvider typeProvider)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(typeProvider);

        return rows.Select(row =>
        {
            var typeMetadata = PostgreSqlColumnTypeMetadata.Create(
                typeProvider,
                new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
                    row.DataType,
                    row.UdtSchema,
                    row.UdtName,
                    // an attribute is never reported as being defined over a domain; a domain-typed
                    // attribute names the domain in its udt columns
                    null,
                    null,
                    row.TypeKind,
                    row.ElementTypeSchema,
                    row.ElementTypeName,
                    row.ElementTypeKind,
                    row.EnumLabels
                ),
                !row.CollationName.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.CollationName))
                    : Option<Identifier>.None,
                PostgreSqlColumnTypeMetadata.CreateMaxLength(row.CharacterMaximumLength, row.NumericPrecision, row.NumericPrecisionRadix),
                PostgreSqlColumnTypeMetadata.CreateNumericPrecision(row.NumericPrecision, row.NumericScale, row.NumericPrecisionRadix)
            );

            var isNullable = !string.Equals(row.IsNullable, Constants.No, StringComparison.OrdinalIgnoreCase);
            var defaultValue = !row.AttributeDefault.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.AttributeDefault)
                : Option<string>.None;

            return new DatabaseColumn(
                Identifier.CreateQualifiedIdentifier(row.AttributeName!),
                typeProvider.CreateColumnType(typeMetadata),
                isNullable,
                defaultValue,
                Option<IAutoIncrement>.None);
        }).ToList();
    }

    public static IReadOnlyCollection<IDatabaseCheckConstraint> MapChecks(IEnumerable<IUserDefinedTypeCheckRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return rows
            .Where(static row => !row.ConstraintName.IsNullOrWhiteSpace() && !row.Definition.IsNullOrWhiteSpace())
            .Select(static row => new PostgreSqlCheckConstraint(
                Identifier.CreateQualifiedIdentifier(row.ConstraintName!),
                row.Definition!,
                row.IsValidated
            ))
            .ToList();
    }

    private static class Constants
    {
        public const string No = "NO";
    }
}
