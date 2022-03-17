﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.PostgreSql.QueryResult;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A materialized view provider for PostgreSQL.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
public class PostgreSqlDatabaseMaterializedViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseMaterializedViewProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <param name="identifierResolver">An identifier resolver that enables non-strict name resolution.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
    public PostgreSqlDatabaseMaterializedViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
    }

    /// <summary>
    /// A database connection that is specific to a given PostgreSQL database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Gets an identifier resolver that enables more relaxed matching against database object names.
    /// </summary>
    /// <value>An identifier resolver.</value>
    protected IIdentifierResolutionStrategy IdentifierResolver { get; }

    /// <summary>
    /// A database connection factory used to query the database.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Gets all materialized views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of materialized views.</returns>
    public virtual async IAsyncEnumerable<IDatabaseView> GetAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryResult = await DbConnection.QueryAsync<GetAllMaterializedViewNamesQueryResult>(ViewsQuery, cancellationToken).ConfigureAwait(false);
        var viewNames = queryResult
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName);

        foreach (var viewName in viewNames)
            yield return await LoadViewAsyncCore(viewName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// A SQL query that retrieves the names of materialized views available in the database.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ViewsQuery => ViewsQuerySql;

    private const string ViewsQuerySql = @$"
select schemaname as ""{ nameof(GetAllMaterializedViewNamesQueryResult.SchemaName) }"", matviewname as ""{ nameof(GetAllMaterializedViewNamesQueryResult.ViewName) }""
from pg_catalog.pg_matviews
where schemaname not in ('pg_catalog', 'information_schema')
order by schemaname, matviewname
";

    /// <summary>
    /// Gets a materialized view.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A materialized view in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        var candidateViewName = QualifyViewName(viewName);
        return LoadView(candidateViewName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the materialized view. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A materialized view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(viewName)
            .Select(QualifyViewName);

        return resolvedNames
            .Select(name => GetResolvedViewNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the materialized view without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="viewName">A materialized view name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A materialized view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        var candidateViewName = QualifyViewName(viewName);
        var qualifiedViewName = DbConnection.QueryFirstOrNone<GetMaterializedViewNameQueryResult>(
            ViewNameQuery,
            new GetMaterializedViewNameQuery { SchemaName = candidateViewName.Schema!, ViewName = candidateViewName.LocalName },
            cancellationToken
        );

        return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ViewName));
    }

    /// <summary>
    /// A SQL query that retrieves the resolved name of a materialized view in the database.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ViewNameQuery => ViewNameQuerySql;

    private const string ViewNameQuerySql = @$"
select schemaname as ""{ nameof(GetMaterializedViewNameQueryResult.SchemaName) }"", matviewname as ""{ nameof(GetMaterializedViewNameQueryResult.ViewName) }""
from pg_catalog.pg_matviews
where schemaname = @{ nameof(GetMaterializedViewNameQuery.SchemaName) } and matviewname = @{ nameof(GetMaterializedViewNameQuery.ViewName) }
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

    /// <summary>
    /// Retrieves a database view, if available.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view definition, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected virtual OptionAsync<IDatabaseView> LoadView(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        var candidateViewName = QualifyViewName(viewName);
        return GetResolvedViewName(candidateViewName, cancellationToken)
            .MapAsync(name => LoadViewAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var columnsTask = LoadColumnsAsync(viewName, cancellationToken);
        var definitionTask = LoadDefinitionAsync(viewName, cancellationToken);

        await Task.WhenAll(columnsTask, definitionTask).ConfigureAwait(false);

        var columns = await columnsTask.ConfigureAwait(false);
        var definition = await definitionTask.ConfigureAwait(false);

        return new DatabaseMaterializedView(viewName, definition, columns);
    }

    /// <summary>
    /// Retrieves the definition of a view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A string representing the definition of a view.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected virtual Task<string> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        return DbConnection.ExecuteScalarAsync<string>(
            DefinitionQuery,
            new GetMaterializedViewDefinitionQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
    }

    /// <summary>
    /// A SQL query that retrieves the definition of a materialized view.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string DefinitionQuery => DefinitionQuerySql;

    private const string DefinitionQuerySql = @$"
select definition
from pg_catalog.pg_matviews
where schemaname = @{ nameof(GetMaterializedViewDefinitionQuery.SchemaName) } and matviewname = @{ nameof(GetMaterializedViewDefinitionQuery.ViewName) }";

    /// <summary>
    /// Retrieves the columns for a given materialized view.
    /// </summary>
    /// <param name="viewName">A materialized view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        return LoadColumnsAsyncCore(viewName, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var query = await DbConnection.QueryAsync<GetMaterializedViewColumnsQueryResult>(
            ColumnsQuery,
            new GetMaterializedViewColumnsQuery { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        var result = new List<IDatabaseColumn>();

        foreach (var row in query)
        {
            var typeMetadata = new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(row.DataType),
                Collation = !row.CollationName.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.CollationCatalog, row.CollationSchema, row.CollationName))
                    : Option<Identifier>.None,
                //TODO -- need to fix max length as it's different for char-like objects and numeric
                //MaxLength = row.,
                // TODO: numeric_precision has a base, can be either binary or decimal, need to use the correct one
                NumericPrecision = new NumericPrecision(row.NumericPrecision, row.NumericScale)
            };

            var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);
            var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
            var autoIncrement = Option<IAutoIncrement>.None;
            var defaultValue = !row.ColumnDefault.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.ColumnDefault)
                : Option<string>.None;

            var column = new DatabaseColumn(columnName, columnType, string.Equals(row.IsNullable, Constants.Yes, StringComparison.Ordinal), defaultValue, autoIncrement);
            result.Add(column);
        }

        return result;
    }

    /// <summary>
    /// A SQL query that retrieves column definitions.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string ColumnsQuery => ColumnsQuerySql;

    // taken largely from information_schema.sql for postgres (but modified to work with matviews)
    private const string ColumnsQuerySql = @$"
SELECT
    a.attname AS ""{ nameof(GetMaterializedViewColumnsQueryResult.ColumnName) }"",
    a.attnum AS ""{ nameof(GetMaterializedViewColumnsQueryResult.OrdinalPosition) }"",
    pg_catalog.pg_get_expr(ad.adbin, ad.adrelid) AS ""{ nameof(GetMaterializedViewColumnsQueryResult.ColumnDefault) }"",
    CASE WHEN a.attnotnull OR (t.typtype = 'd' AND t.typnotnull) THEN 'NO' ELSE 'YES' END
        AS ""{ nameof(GetMaterializedViewColumnsQueryResult.IsNullable) }"",

    CASE WHEN t.typtype = 'd' THEN
    CASE WHEN bt.typelem <> 0 AND bt.typlen = -1 THEN 'ARRAY'
        WHEN nbt.nspname = 'pg_catalog' THEN format_type(t.typbasetype, null)
        ELSE 'USER-DEFINED' END
    ELSE
    CASE WHEN t.typelem <> 0 AND t.typlen = -1 THEN 'ARRAY'
        WHEN nt.nspname = 'pg_catalog' THEN format_type(a.atttypid, null)
        ELSE 'USER-DEFINED' END
    END
    AS ""{ nameof(GetMaterializedViewColumnsQueryResult.DataType) }"",

    " + PgCharMaxLength + @$"
    AS ""{ nameof(GetMaterializedViewColumnsQueryResult.CharacterMaximumLength) }"",

    " + PgCharOctetLength + @$"
    AS ""{ nameof(GetMaterializedViewColumnsQueryResult.CharacterOctetLength) }"",

    " + PgNumericPrecision + @$"
    AS ""{ nameof(GetMaterializedViewColumnsQueryResult.NumericPrecision) }"",

    " + PgNumericPrecisionRadix + @$"
    AS ""{ nameof(GetMaterializedViewColumnsQueryResult.NumericPrecisionRadix) }"",

    " + PgNumericScale + @$"
    AS ""{ nameof(GetMaterializedViewColumnsQueryResult.NumericScale) }"",

    " + PgDatetimePrecision + @$"
    AS ""{ nameof(GetMaterializedViewColumnsQueryResult.DatetimePrecision) }"",

    " + PgIntervalType + @$"
    AS ""{ nameof(GetMaterializedViewColumnsQueryResult.IntervalType) }"",

    CASE WHEN nco.nspname IS NOT NULL THEN current_database() END AS ""{ nameof(GetMaterializedViewColumnsQueryResult.CollationCatalog) }"",
    nco.nspname AS ""{ nameof(GetMaterializedViewColumnsQueryResult.CollationSchema) }"",
    co.collname AS ""{ nameof(GetMaterializedViewColumnsQueryResult.CollationName) }"",

    CASE WHEN t.typtype = 'd' THEN current_database() ELSE null END
        AS ""{ nameof(GetMaterializedViewColumnsQueryResult.DomainCatalog) }"",
    CASE WHEN t.typtype = 'd' THEN nt.nspname ELSE null END
        AS ""{ nameof(GetMaterializedViewColumnsQueryResult.DomainSchema) }"",
    CASE WHEN t.typtype = 'd' THEN t.typname ELSE null END
        AS ""{ nameof(GetMaterializedViewColumnsQueryResult.DomainName) }"",

    current_database() AS ""{ nameof(GetMaterializedViewColumnsQueryResult.UdtCatalog) }"",
    coalesce(nbt.nspname, nt.nspname) AS ""{ nameof(GetMaterializedViewColumnsQueryResult.UdtSchema) }"",
    coalesce(bt.typname, t.typname) AS ""{ nameof(GetMaterializedViewColumnsQueryResult.UdtName) }"",

    a.attnum AS ""{ nameof(GetMaterializedViewColumnsQueryResult.DtdIdentifier) }""

FROM (pg_catalog.pg_attribute a LEFT JOIN pg_catalog.pg_attrdef ad ON attrelid = adrelid AND attnum = adnum)
    JOIN (pg_catalog.pg_class c JOIN pg_catalog.pg_namespace nc ON (c.relnamespace = nc.oid)) ON a.attrelid = c.oid
    JOIN (pg_catalog.pg_type t JOIN pg_catalog.pg_namespace nt ON (t.typnamespace = nt.oid)) ON a.atttypid = t.oid
    LEFT JOIN (pg_catalog.pg_type bt JOIN pg_catalog.pg_namespace nbt ON (bt.typnamespace = nbt.oid))
    ON (t.typtype = 'd' AND t.typbasetype = bt.oid)
    LEFT JOIN (pg_catalog.pg_collation co JOIN pg_catalog.pg_namespace nco ON (co.collnamespace = nco.oid))
    ON a.attcollation = co.oid AND (nco.nspname, co.collname) <> ('pg_catalog', 'default')

WHERE (NOT pg_catalog.pg_is_other_temp_schema(nc.oid))

        AND a.attnum > 0 AND NOT a.attisdropped
        AND c.relkind = 'm' -- m = matview

        AND (pg_catalog.pg_has_role(c.relowner, 'USAGE')
            OR has_column_privilege(c.oid, a.attnum,
                                    'SELECT, INSERT, UPDATE, REFERENCES'))
        AND nc.nspname = @{ nameof(GetMaterializedViewColumnsQuery.SchemaName) } and c.relname = @{ nameof(GetMaterializedViewColumnsQuery.ViewName) }
ORDER BY a.attnum -- ordinal_position";

    /// <summary>
    /// Qualifies the name of the view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <returns>A view name is at least as qualified as the given view name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected Identifier QualifyViewName(Identifier viewName)
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));

        var schema = viewName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
    }

    // In order to cleanly build up an equivalent of information_schema.columns view
    // we also need to have some functions available. However, because we do not want
    // to modify any existing schema, we will build up this query via string replacement.
    // Normally this is unsafe, but we will have *no user input* for the query, and any
    // parameters are already parameterised

    private const string PgTrueTypId = "CASE WHEN t.typtype = 'd' THEN t.typbasetype ELSE a.atttypid END";
    private const string PgTrueTypMod = "CASE WHEN t.typtype = 'd' THEN t.typtypmod ELSE a.atttypmod END";

    private const string PgCharMaxLength = "CASE WHEN " + PgTrueTypMod + @" = -1 /* default typmod */
       THEN null
       WHEN " + PgTrueTypId + @" IN (1042, 1043) /* char, varchar */
       THEN " + PgTrueTypMod + @" - 4
       WHEN " + PgTrueTypId + @" IN (1560, 1562) /* bit, varbit */
       THEN " + PgTrueTypMod + @"
       ELSE null
  END";

    private const string PgCharOctetLength = "CASE WHEN " + PgTrueTypId + @" IN (25, 1042, 1043) /* text, char, varchar */
       THEN CASE WHEN " + PgTrueTypMod + @" = -1 /* default typmod */
                 THEN CAST(2^30 AS integer)
                 ELSE " + PgCharMaxLength + @" *
                      pg_catalog.pg_encoding_max_length((SELECT encoding FROM pg_catalog.pg_database WHERE datname = pg_catalog.current_database()))
            END
       ELSE null
  END";

    private const string PgNumericPrecision = "CASE " + PgTrueTypId + @"
         WHEN 21 /*int2*/ THEN 16
         WHEN 23 /*int4*/ THEN 32
         WHEN 20 /*int8*/ THEN 64
         WHEN 1700 /*numeric*/ THEN
              CASE WHEN " + PgTrueTypMod + @" = -1
                   THEN null
                   ELSE ((" + PgTrueTypMod + @" - 4) >> 16) & 65535
                   END
         WHEN 700 /*float4*/ THEN 24 /*FLT_MANT_DIG*/
         WHEN 701 /*float8*/ THEN 53 /*DBL_MANT_DIG*/
         ELSE null
  END";

    private const string PgNumericPrecisionRadix = "CASE WHEN " + PgTrueTypId + @" IN (21, 23, 20, 700, 701) THEN 2
       WHEN " + PgTrueTypId + @" IN (1700) THEN 10
       ELSE null
  END";

    private const string PgNumericScale = "CASE WHEN " + PgTrueTypId + @" IN (21, 23, 20) THEN 0
       WHEN " + PgTrueTypId + @" IN (1700) THEN
            CASE WHEN " + PgTrueTypMod + @" = -1
                 THEN null
                 ELSE (" + PgTrueTypMod + @" - 4) & 65535
                 END
       ELSE null
  END";

    private const string PgDatetimePrecision = "CASE WHEN " + PgTrueTypId + @" IN (1082) /* date */
           THEN 0
       WHEN " + PgTrueTypId + @" IN (1083, 1114, 1184, 1266) /* time, timestamp, same + tz */
           THEN CASE WHEN " + PgTrueTypMod + " < 0 THEN 6 ELSE " + PgTrueTypMod + @" END
       WHEN " + PgTrueTypId + @" IN (1186) /* interval */
           THEN CASE WHEN " + PgTrueTypMod + " < 0 OR " + PgTrueTypMod + " & 65535 = 65535 THEN 6 ELSE " + PgTrueTypMod + @" & 65535 END
       ELSE null
  END";

    private const string PgIntervalType = "CASE WHEN " + PgTrueTypId + @" IN (1186) /* interval */
           THEN pg_catalog.upper(substring(pg_catalog.format_type(" + PgTrueTypId + ", " + PgTrueTypMod + @") from 'interval[()0-9]* #"" %#""' for '#'))
       ELSE null
  END";

    private static class Constants
    {
        public const string Yes = "YES";
    }
}
