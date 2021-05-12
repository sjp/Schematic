using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.PostgreSql.QueryResult;

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
            var query = await DbConnection.QueryAsync<GetV10TableColumnsQueryResult>(
                ColumnsQuery,
                new GetV10TableColumnsQuery { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
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
    column_name as ""{ nameof(GetV10TableColumnsQueryResult.ColumnName) }"",
    ordinal_position as ""{ nameof(GetV10TableColumnsQueryResult.OrdinalPosition) }"",
    column_default as ""{ nameof(GetV10TableColumnsQueryResult.ColumnDefault) }"",
    is_nullable as ""{ nameof(GetV10TableColumnsQueryResult.IsNullable) }"",
    data_type as ""{ nameof(GetV10TableColumnsQueryResult.DataType) }"",
    character_maximum_length as ""{ nameof(GetV10TableColumnsQueryResult.CharacterMaximumLength) }"",
    character_octet_length as ""{ nameof(GetV10TableColumnsQueryResult.CharacterOctetLength) }"",
    numeric_precision as ""{ nameof(GetV10TableColumnsQueryResult.NumericPrecision) }"",
    numeric_precision_radix as ""{ nameof(GetV10TableColumnsQueryResult.NumericPrecisionRadix) }"",
    numeric_scale as ""{ nameof(GetV10TableColumnsQueryResult.NumericScale) }"",
    datetime_precision as ""{ nameof(GetV10TableColumnsQueryResult.DatetimePrecision) }"",
    interval_type as ""{ nameof(GetV10TableColumnsQueryResult.IntervalType) }"",
    collation_catalog as ""{ nameof(GetV10TableColumnsQueryResult.CollationCatalog) }"",
    collation_schema as ""{ nameof(GetV10TableColumnsQueryResult.CollationSchema) }"",
    collation_name as ""{ nameof(GetV10TableColumnsQueryResult.CollationName) }"",
    domain_catalog as ""{ nameof(GetV10TableColumnsQueryResult.DomainCatalog) }"",
    domain_schema as ""{ nameof(GetV10TableColumnsQueryResult.DomainSchema) }"",
    domain_name as ""{ nameof(GetV10TableColumnsQueryResult.DomainName) }"",
    udt_catalog as ""{ nameof(GetV10TableColumnsQueryResult.UdtCatalog) }"",
    udt_schema as ""{ nameof(GetV10TableColumnsQueryResult.UdtSchema) }"",
    udt_name as ""{ nameof(GetV10TableColumnsQueryResult.UdtName) }"",
    dtd_identifier as ""{ nameof(GetV10TableColumnsQueryResult.DtdIdentifier) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[1] as ""{ nameof(GetV10TableColumnsQueryResult.SerialSequenceSchemaName) }"",
    (pg_catalog.parse_ident(pg_catalog.pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name)))[2] as ""{ nameof(GetV10TableColumnsQueryResult.SerialSequenceLocalName) }"",
    is_identity as ""{ nameof(GetV10TableColumnsQueryResult.IsIdentity) }"",
    identity_generation as ""{ nameof(GetV10TableColumnsQueryResult.IdentityGeneration) }"",
    identity_start as ""{ nameof(GetV10TableColumnsQueryResult.IdentityStart) }"",
    identity_increment as ""{ nameof(GetV10TableColumnsQueryResult.IdentityIncrement) }"",
    identity_maximum as ""{ nameof(GetV10TableColumnsQueryResult.IdentityMaximum) }"",
    identity_minimum as ""{ nameof(GetV10TableColumnsQueryResult.IdentityMinimum) }"",
    identity_cycle as ""{ nameof(GetV10TableColumnsQueryResult.IdentityCycle) }""
from information_schema.columns
where table_schema = @{ nameof(GetV10TableColumnsQuery.SchemaName) }  and table_name = @{ nameof(GetV10TableColumnsQuery.TableName) }
order by ordinal_position";
    }
}
