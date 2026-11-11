using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// Maps the raw <c>sys.types</c> rows returned by the SQL Server catalog onto the core model. The
/// 'all types' and 'single type' queries project the same columns, so both paths share this mapping.
/// </summary>
internal static class SqlServerUserDefinedTypeMapper
{
    public static DatabaseUserDefinedType MapType(
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

        // only a table type carries attributes and checks
        if (row.IsTableType)
            return new DatabaseUserDefinedType.Table(typeName, attributes, checks);

        // a CLR type has no textual definition in the catalog, only the class implementing it
        if (row.IsAssemblyType)
        {
            var externalName = !row.AssemblyName.IsNullOrWhiteSpace() && !row.AssemblyClass.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.AssemblyName + "." + row.AssemblyClass)
                : Option<string>.None;
            return new DatabaseUserDefinedType.Clr(typeName, externalName);
        }

        // every other user-defined type is an alias type, defined over a built-in type; one whose
        // built-in type cannot be read cannot be described as an alias, so it is reported as unknown
        if (row.BaseTypeName.IsNullOrWhiteSpace())
            return new DatabaseUserDefinedType.Unknown(typeName);

        var aliasedType = typeProvider.CreateColumnType(new ColumnTypeMetadata
        {
            TypeName = Identifier.CreateQualifiedIdentifier(Constants.SystemTypeSchema, row.BaseTypeName),
            MaxLength = row.MaxLength,
            NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
            Collation = !row.Collation.IsNullOrWhiteSpace()
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                : Option<Identifier>.None,
        });

        // only an alias type can be bound to a default, with sp_bindefault
        var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
            ? Option<string>.Some(row.DefaultValue)
            : Option<string>.None;

        return new DatabaseUserDefinedType.Alias(typeName, aliasedType, row.IsNullable, defaultValue);
    }

    public static IReadOnlyList<IDatabaseColumn> MapAttributes(IEnumerable<IUserDefinedTypeAttributeRow> rows, IDbTypeProvider typeProvider)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(typeProvider);

        return rows.Select(row =>
        {
            var typeMetadata = new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                MaxLength = row.MaxLength,
                NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
            };

            // sys.default_constraints is joined for the definition alone, so an attribute default
            // never carries the name of the constraint holding it
            var defaultValue = SqlServerDefaultValueParser.Parse(row.DefaultValue, null);
            var valueSource = SqlServerCatalogMapper.MapColumnValueSource(
                row.IsComputed,
                row.ComputedColumnDefinition,
                row.ComputedColumnIsPersisted,
                row.IsIdentity,
                row.IdentitySeed,
                row.IdentityIncrement);

            return new DatabaseColumn(
                Identifier.CreateQualifiedIdentifier(row.ColumnName),
                typeProvider.CreateColumnType(typeMetadata),
                row.IsNullable,
                defaultValue,
                valueSource);
        }).ToList();
    }

    public static IReadOnlyCollection<IDatabaseCheckConstraint> MapChecks(IEnumerable<IUserDefinedTypeCheckRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return rows.Select(static row => new DatabaseCheckConstraint(
            Identifier.CreateQualifiedIdentifier(row.ConstraintName),
            row.Definition,
            !row.IsDisabled,
            !row.IsNotTrusted,
            ConstraintDeferrability.NotDeferrable
        )).ToList();
    }

    private static class Constants
    {
        public const string SystemTypeSchema = "sys";
    }
}
