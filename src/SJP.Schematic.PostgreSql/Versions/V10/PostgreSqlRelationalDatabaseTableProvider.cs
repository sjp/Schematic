using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Versions.V10
{
    /// <summary>
    /// A database table provider for PostgreSQL v10 and higher.
    /// </summary>
    /// <seealso cref="V11.PostgreSqlRelationalDatabaseTableProvider" />
    public class PostgreSqlRelationalDatabaseTableProvider : PostgreSqlRelationalDatabaseTableProviderBase
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
            var query = await DbConnection.QueryAsync<ColumnDataV10>(
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

                var column = new DatabaseColumn(columnName, columnType, isNullable, defaultValue, autoIncrement);
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
    column_name as ""{ nameof(ColumnDataV10.ColumnName) }"",
    ordinal_position as ""{ nameof(ColumnDataV10.OrdinalPosition) }"",
    column_default as ""{ nameof(ColumnDataV10.ColumnDefault) }"",
    is_nullable as ""{ nameof(ColumnDataV10.IsNullable) }"",
    data_type as ""{ nameof(ColumnDataV10.DataType) }"",
    character_maximum_length as ""{ nameof(ColumnDataV10.CharacterMaximumLength) }"",
    character_octet_length as ""{ nameof(ColumnDataV10.CharacterOctetLength) }"",
    numeric_precision as ""{ nameof(ColumnDataV10.NumericPrecision) }"",
    numeric_precision_radix as ""{ nameof(ColumnDataV10.NumericPrecisionRadix) }"",
    numeric_scale as ""{ nameof(ColumnDataV10.NumericScale) }"",
    datetime_precision as ""{ nameof(ColumnDataV10.DatetimePrecision) }"",
    interval_type as ""{ nameof(ColumnDataV10.IntervalType) }"",
    collation_catalog as ""{ nameof(ColumnDataV10.CollationCatalog) }"",
    collation_schema as ""{ nameof(ColumnDataV10.CollationSchema) }"",
    collation_name as ""{ nameof(ColumnDataV10.CollationName) }"",
    domain_catalog as ""{ nameof(ColumnDataV10.DomainCatalog) }"",
    domain_schema as ""{ nameof(ColumnDataV10.DomainSchema) }"",
    domain_name as ""{ nameof(ColumnDataV10.DomainName) }"",
    udt_catalog as ""{ nameof(ColumnDataV10.UdtCatalog) }"",
    udt_schema as ""{ nameof(ColumnDataV10.UdtSchema) }"",
    udt_name as ""{ nameof(ColumnDataV10.UdtName) }"",
    dtd_identifier as ""{ nameof(ColumnDataV10.DtdIdentifier) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[1] as ""{ nameof(ColumnDataV10.SerialSequenceSchemaName) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[2] as ""{ nameof(ColumnDataV10.SerialSequenceLocalName) }"",
    is_identity as ""{ nameof(ColumnDataV10.IsIdentity) }"",
    identity_generation as ""{ nameof(ColumnDataV10.IdentityGeneration) }"",
    identity_start as ""{ nameof(ColumnDataV10.IdentityStart) }"",
    identity_increment as ""{ nameof(ColumnDataV10.IdentityIncrement) }"",
    identity_maximum as ""{ nameof(ColumnDataV10.IdentityMaximum) }"",
    identity_minimum as ""{ nameof(ColumnDataV10.IdentityMinimum) }"",
    identity_cycle as ""{ nameof(ColumnDataV10.IdentityCycle) }""
from information_schema.columns
where table_schema = @SchemaName and table_name = @TableName
order by ordinal_position";
    }
}
