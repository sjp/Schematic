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

        var kind = row.IsTableType
            ? UserDefinedTypeKind.Table
            : row.IsAssemblyType
                ? UserDefinedTypeKind.Clr
                : UserDefinedTypeKind.Alias;

        // only an alias type is defined over a built-in type; a table or assembly type is not
        var baseType = !row.IsTableType && !row.IsAssemblyType && !row.BaseTypeName.IsNullOrWhiteSpace()
            ? Option<IDbType>.Some(typeProvider.CreateColumnType(new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(Constants.SystemTypeSchema, row.BaseTypeName),
                MaxLength = row.MaxLength,
                NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
            }))
            : Option<IDbType>.None;

        // a CLR type has no textual definition in the catalog, only the class implementing it
        var definition = row.IsAssemblyType && !row.AssemblyName.IsNullOrWhiteSpace() && !row.AssemblyClass.IsNullOrWhiteSpace()
            ? Option<string>.Some(row.AssemblyName + "." + row.AssemblyClass)
            : Option<string>.None;

        var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
            ? Option<string>.Some(row.DefaultValue)
            : Option<string>.None;

        return new DatabaseUserDefinedType(
            typeName,
            kind,
            baseType,
            [],
            attributes,
            checks,
            row.IsNullable,
            defaultValue,
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
                TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                MaxLength = row.MaxLength,
                NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
            };

            // sys.identity_columns reports the seed and increment as sql_variant, which can be
            // null for a column whose identity was defined without them; SQL Server's own
            // defaults are 1 and 1.
            var identitySeed = row.IdentitySeed ?? 1;
            var identityIncrement = row.IdentityIncrement is long incr && incr != 0 ? incr : 1;
            var autoIncrement = row.IsIdentity
                ? Option<IAutoIncrement>.Some(new AutoIncrement(identitySeed, identityIncrement))
                : Option<IAutoIncrement>.None;
            var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.DefaultValue)
                : Option<string>.None;
            var computedColumnDefinition = !row.ComputedColumnDefinition.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.ComputedColumnDefinition)
                : Option<string>.None;
            var computedStorage = row.ComputedColumnIsPersisted == true
                ? ComputedColumnStorage.Stored
                : ComputedColumnStorage.Virtual;

            return new DatabaseColumn(
                Identifier.CreateQualifiedIdentifier(row.ColumnName),
                typeProvider.CreateColumnType(typeMetadata),
                row.IsNullable,
                defaultValue,
                autoIncrement,
                row.IsComputed,
                computedColumnDefinition,
                computedStorage);
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
