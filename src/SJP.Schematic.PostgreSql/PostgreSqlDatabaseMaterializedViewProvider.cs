using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDatabaseMaterializedViewProvider : IDatabaseViewProvider
    {
        public PostgreSqlDatabaseMaterializedViewProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver, IDbTypeProvider typeProvider)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        protected IDbTypeProvider TypeProvider { get; }

        public virtual async Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default)
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery, cancellationToken).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .Select(QualifyViewName)
                .ToList();

            var views = new List<IDatabaseView>();

            foreach (var viewName in viewNames)
            {
                var view = await LoadViewAsyncCore(viewName, cancellationToken).ConfigureAwait(false);
                views.Add(view);
            }

            return views;
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = @"
select schemaname as SchemaName, matviewname as ObjectName
from pg_catalog.pg_matviews
where schemaname not in ('pg_catalog', 'information_schema')";

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadView(candidateViewName, cancellationToken);
        }

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

        protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = candidateViewName.Schema, ViewName = candidateViewName.LocalName },
                cancellationToken
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select schemaname as SchemaName, matviewname as ObjectName
from pg_catalog.pg_matviews
where schemaname = @SchemaName and matviewname = @ViewName
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

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
            var columns = await LoadColumnsAsync(viewName, cancellationToken).ConfigureAwait(false);
            var definition = await LoadDefinitionAsync(viewName, cancellationToken).ConfigureAwait(false);

            return new DatabaseMaterializedView(viewName, definition, columns);
        }

        protected virtual Task<string> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Connection.ExecuteScalarAsync<string>(
                DefinitionQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            );
        }

        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select definition
from pg_catalog.pg_matviews
where schemaname = @SchemaName and matviewname = @ViewName";

        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadColumnsAsyncCore(viewName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(
                ColumnsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.data_type),
                    Collation = row.collation_name.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.collation_catalog, row.collation_schema, row.collation_name),
                    //TODO -- need to fix max length as it's different for char-like objects and numeric
                    //MaxLength = row.,
                    // TODO: numeric_precision has a base, can be either binary or decimal, need to use the correct one
                    NumericPrecision = new NumericPrecision(row.numeric_precision, row.numeric_scale)
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.column_name);
                var autoIncrement = Option<IAutoIncrement>.None;
                var defaultValue = !row.column_default.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.column_default)
                    : Option<string>.None;

                var column = new DatabaseColumn(columnName, columnType, row.is_nullable == Constants.Yes, defaultValue, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string ColumnsQuery => ColumnsQuerySql;

        // taken largely from information_schema.sql for postgres (but modified to work with matviews)
        private const string ColumnsQuerySql = @"
SELECT
    a.attname AS column_name,
    a.attnum AS ordinal_position,
    pg_catalog.pg_get_expr(ad.adbin, ad.adrelid) AS column_default,
    CASE WHEN a.attnotnull OR (t.typtype = 'd' AND t.typnotnull) THEN 'NO' ELSE 'YES' END
        AS is_nullable,

    CASE WHEN t.typtype = 'd' THEN
    CASE WHEN bt.typelem <> 0 AND bt.typlen = -1 THEN 'ARRAY'
        WHEN nbt.nspname = 'pg_catalog' THEN format_type(t.typbasetype, null)
        ELSE 'USER-DEFINED' END
    ELSE
    CASE WHEN t.typelem <> 0 AND t.typlen = -1 THEN 'ARRAY'
        WHEN nt.nspname = 'pg_catalog' THEN format_type(a.atttypid, null)
        ELSE 'USER-DEFINED' END
    END
    AS data_type,

    " + PgCharMaxLength + @"
    AS character_maximum_length,

    " + PgCharOctetLength + @"
    AS character_octet_length,

    " + PgNumericPrecision + @"
    AS numeric_precision,

    " + PgNumericPrecisionRadix + @"
    AS numeric_precision_radix,

    " + PgNumericScale + @"
    AS numeric_scale,

    " + PgDatetimePrecision + @"
    AS datetime_precision,

    " + PgIntervalType + @"
    AS interval_type,
    null AS interval_precision,

    CASE WHEN nco.nspname IS NOT NULL THEN current_database() END AS collation_catalog,
    nco.nspname AS collation_schema,
    co.collname AS collation_name,

    CASE WHEN t.typtype = 'd' THEN current_database() ELSE null END
        AS domain_catalog,
    CASE WHEN t.typtype = 'd' THEN nt.nspname ELSE null END
        AS domain_schema,
    CASE WHEN t.typtype = 'd' THEN t.typname ELSE null END
        AS domain_name,

    current_database() AS udt_catalog,
    coalesce(nbt.nspname, nt.nspname) AS udt_schema,
    coalesce(bt.typname, t.typname) AS udt_name,

    a.attnum AS dtd_identifier

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
        AND nc.nspname = @SchemaName and c.relname = @ViewName
ORDER BY a.attnum -- ordinal_position";

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
}
