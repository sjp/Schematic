using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Versions.V12
{
    /// <summary>
    /// A database table provider for PostgreSQL v12 and higher.
    /// </summary>
    /// <seealso cref="V11.PostgreSqlRelationalDatabaseTableProvider" />
    public class PostgreSqlRelationalDatabaseTableProvider : V11.PostgreSqlRelationalDatabaseTableProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlRelationalDatabaseTableProvider"/> class.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">A database identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
        public PostgreSqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
            : base(connection, identifierDefaults, identifierResolver)
        {
        }

        /// <summary>
        /// Retrieves the columns for a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An ordered collection of columns.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected override Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadColumnsAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var query = await DbConnection.QueryAsync<ColumnDataV12>(
                ColumnsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(Constants.PgCatalog, row.DataType),
                    Collation = !row.CollationName.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.CollationCatalog, row.CollationSchema, row.CollationName))
                        : Option<Identifier>.None,
                    MaxLength = row.CharacterMaximumLength > 0
                        ? row.CharacterMaximumLength
                        : CreatePrecisionFromBase(row.NumericPrecision, row.NumericPrecisionRadix),
                    NumericPrecision = row.NumericPrecisionRadix > 0
                        ? Option<INumericPrecision>.Some(CreatePrecisionWithScaleFromBase(row.NumericPrecision, row.NumericScale, row.NumericPrecisionRadix))
                        : Option<INumericPrecision>.None
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);

                var isAutoIncrement = string.Equals(row.IsIdentity, Constants.Yes, StringComparison.Ordinal);
                var autoIncrement = isAutoIncrement
                    && decimal.TryParse(row.IdentityStart, NumberStyles.Float, CultureInfo.InvariantCulture, out var seqStart)
                    && decimal.TryParse(row.IdentityIncrement, NumberStyles.Float, CultureInfo.InvariantCulture, out var seqIncr)
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(seqStart, seqIncr))
                    : Option<IAutoIncrement>.None;

                var isSerialAutoIncrement = !isAutoIncrement && !row.SerialSequenceSchemaName.IsNullOrWhiteSpace() && !row.SerialSequenceLocalName.IsNullOrWhiteSpace();
                if (isSerialAutoIncrement)
                    autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(1, 1));

                var defaultValue = !row.ColumnDefault.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.ColumnDefault)
                    : Option<string>.None;
                var isNullable = string.Equals(row.IsNullable, Constants.Yes, StringComparison.Ordinal);

                var isComputed = string.Equals(row.IsGenerated, Constants.Always, StringComparison.Ordinal);
                var computedDefinition = isComputed
                    ? Option<string>.Some(row.GenerationExpression ?? string.Empty)
                    : Option<string>.None;

                var column = isComputed
                    ? new DatabaseComputedColumn(columnName, columnType, isNullable, defaultValue, computedDefinition)
                    : new DatabaseColumn(columnName, columnType, isNullable, defaultValue, autoIncrement);
                result.Add(column);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves column definitions.
        /// </summary>
        /// <value>A SQL query.</value>
        protected override string ColumnsQuery => ColumnsQuerySql;

        // a little bit convoluted due to the quote_ident() being required.
        // when missing, case folding will occur (we should have guaranteed that this is already done)
        // additionally the default behaviour misses the schema which may be necessary
        private static readonly string ColumnsQuerySql = @$"
select
    column_name as ""{ nameof(ColumnDataV12.ColumnName) }"",
    ordinal_position as ""{ nameof(ColumnDataV12.OrdinalPosition) }"",
    column_default as ""{ nameof(ColumnDataV12.ColumnDefault) }"",
    is_nullable as ""{ nameof(ColumnDataV12.IsNullable) }"",
    data_type as ""{ nameof(ColumnDataV12.DataType) }"",
    character_maximum_length as ""{ nameof(ColumnDataV12.CharacterMaximumLength) }"",
    character_octet_length as ""{ nameof(ColumnDataV12.CharacterOctetLength) }"",
    numeric_precision as ""{ nameof(ColumnDataV12.NumericPrecision) }"",
    numeric_precision_radix as ""{ nameof(ColumnDataV12.NumericPrecisionRadix) }"",
    numeric_scale as ""{ nameof(ColumnDataV12.NumericScale) }"",
    datetime_precision as ""{ nameof(ColumnDataV12.DatetimePrecision) }"",
    interval_type as ""{ nameof(ColumnDataV12.IntervalType) }"",
    collation_catalog as ""{ nameof(ColumnDataV12.CollationCatalog) }"",
    collation_schema as ""{ nameof(ColumnDataV12.CollationSchema) }"",
    collation_name as ""{ nameof(ColumnDataV12.CollationName) }"",
    domain_catalog as ""{ nameof(ColumnDataV12.DomainCatalog) }"",
    domain_schema as ""{ nameof(ColumnDataV12.DomainSchema) }"",
    domain_name as ""{ nameof(ColumnDataV12.DomainName) }"",
    udt_catalog as ""{ nameof(ColumnDataV12.UdtCatalog) }"",
    udt_schema as ""{ nameof(ColumnDataV12.UdtSchema) }"",
    udt_name as ""{ nameof(ColumnDataV12.UdtName) }"",
    dtd_identifier as ""{ nameof(ColumnDataV12.DtdIdentifier) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[1] as ""{ nameof(ColumnDataV12.SerialSequenceSchemaName) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[2] as ""{ nameof(ColumnDataV12.SerialSequenceLocalName) }"",
    is_identity as ""{ nameof(ColumnDataV12.IsIdentity) }"",
    identity_generation as ""{ nameof(ColumnDataV12.IdentityGeneration) }"",
    identity_start as ""{ nameof(ColumnDataV12.IdentityStart) }"",
    identity_increment as ""{ nameof(ColumnDataV12.IdentityIncrement) }"",
    identity_maximum as ""{ nameof(ColumnDataV12.IdentityMaximum) }"",
    identity_minimum as ""{ nameof(ColumnDataV12.IdentityMinimum) }"",
    identity_cycle as ""{ nameof(ColumnDataV12.IdentityCycle) }"",
    is_generated as ""{ nameof(ColumnDataV12.IsGenerated) }"",
    generation_expression as ""{ nameof(ColumnDataV12.GenerationExpression) }""
from information_schema.columns
where table_schema = @SchemaName and table_name = @TableName
order by ordinal_position";

        /// <summary>
        /// Retrieves check constraints defined on a given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of check constraints.</returns>
        protected override async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            var checks = await DbConnection.QueryAsync<CheckConstraintData>(
                ChecksQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            const string checkPrefix = "CHECK (";
            const string checkSuffix = ")";
            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                var definition = checkRow.Definition;
                if (definition.IsNullOrWhiteSpace())
                    continue;

                if (definition.StartsWith(checkPrefix, StringComparison.OrdinalIgnoreCase))
                    definition = definition.Substring(checkPrefix.Length);
                if (definition.EndsWith(')') && definition.Length > 0) // check suffix
                    definition = definition.Substring(0, definition.Length - checkSuffix.Length);

                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);

                var check = new PostgreSqlCheckConstraint(constraintName, definition);
                result.Add(check);
            }

            return result;
        }

        /// <summary>
        /// A SQL query that retrieves check constraint information for a table.
        /// </summary>
        /// <value>A SQL query.</value>
        protected override string ChecksQuery => ChecksQuerySql;

        private static readonly string ChecksQuerySql = @$"
select
    c.conname as ""{ nameof(CheckConstraintData.ConstraintName) }"",
    pg_catalog.pg_get_constraintdef(c.oid) as ""{ nameof(CheckConstraintData.Definition) }""
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
where
    c.contype = 'c'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";
    }
}
